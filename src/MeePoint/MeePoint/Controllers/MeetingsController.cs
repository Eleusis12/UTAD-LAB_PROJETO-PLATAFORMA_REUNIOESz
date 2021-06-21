using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MeePoint.Data;
using MeePoint.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Hosting;
using Vereyon.Web;
using Microsoft.AspNetCore.SignalR;
using MeePoint.Services;

namespace MeePoint.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHostEnvironment _he;
        private readonly IFlashMessage _iFlashMessage;

        public MeetingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHostEnvironment host, IFlashMessage flashMessage)
        {
            _context = context;
            _userManager = userManager;
            _he = host;
            _iFlashMessage = flashMessage;
        }

        // GET: Meetings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Meetings.Include(m => m.Group);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Attendance
        public async Task<IActionResult> Attendance(int? id)
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Schedule(int? id)
        {
            // Obtém o utilizador que está autenticado
            IdentityUser applicationUser = await _userManager.GetUserAsync(User);
            string email = applicationUser?.Email; // will give the user's Email
            var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

            // Vamos verificar se o utilizador que pretende agendar a reunião é responsável ou co-responsável pelo grupo
            string roleUser = _context.GroupMembers.Where(m => m.GroupID == id).FirstOrDefault(m => m.User.Email == email)?.Role;

            // Aqui fazemos a verificação se é manager ou comanager do grupo
            if ((roleUser.ToLower() != "manager" && roleUser.ToLower() != "comanager") || roleUser == null)
            {
                return NotFound();
            }

            List<int> expectedDuration = new List<int>() {
                5,10,15,20,30,40,60,120,
            };

            ViewBag.GroupId = id;
            ViewBag.ExpectedDuration = new SelectList(expectedDuration);
            ViewBag.GUID = Guid.NewGuid().ToString();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(Meeting meeting, string guid, string timeDay)
        {
            // Se o utilizador não escolheu a hora de marcação, volta para página
            if (timeDay == null)
                return View(meeting);

            // timeDay tem o seguinte formato "12:30"
            // Vamos fazer o split de acordo com os dois pontos
            string[] partsTime = timeDay.Split(':');
            DateTime meetingDate = new DateTime(meeting.MeetingDate.Year, meeting.MeetingDate.Month, meeting.MeetingDate.Day, Int32.Parse(partsTime[0]), Int32.Parse(partsTime[1]), 0);

            // Atribuir a data atualizada ao objeto meeting
            meeting.MeetingDate = meetingDate;

            // Assim como no get, temos que fazer a verificação se o utilizador é manager do grupo
            // Obtém o utilizador que está autenticado
            IdentityUser applicationUser = await _userManager.GetUserAsync(User);
            string email = applicationUser?.Email; // will give the user's Email
            var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

            // Vamos verificar se o utilizador que pretende agendar a reunião é responsável ou co-responsável pelo grupo
            var groupMember = _context.GroupMembers.Include(m => m.Group).ThenInclude(m => m.Entity).Where(m => m.GroupID == meeting.GroupID).FirstOrDefault(m => m.User.Email == email);
            string roleUser = groupMember?.Role;

            // Aqui fazemos a verificação se é manager ou comanager do grupo
            if ((roleUser.ToLower() != "manager" && roleUser.ToLower() != "comanager") || roleUser == null)
            {
                return StatusCode(403);
            }

            // Obter a que entidade está associado o grupo
            string entityName = groupMember.Group.Entity.Name;

            meeting.GroupID = groupMember.Group.GroupID;
            meeting.Group = groupMember.Group;

            ModelState.Clear();

            // Primeiro vamos adicionar à base de dados, para gerar o id da reunião
            // TODO: Codificar a funcionalidade de reuniões recurrentes
            if (TryValidateModel(meeting))
            {
                _context.Meetings.Add(meeting);
                await _context.SaveChangesAsync();
            }

            // Depois vamos associar os ficheiros submetidos
            List<Document> documents = new List<Document>();

            string sourcePath = Path.Combine(_he.ContentRootPath, "wwwroot/", entityName, "Meetings/", "Temp/", email, guid);
            string targetPath = Path.Combine(_he.ContentRootPath, "wwwroot/", entityName, "Meetings/", meeting.MeetingID + "/");
            string directory = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string fileName = string.Empty;
            string destFile = string.Empty;

            if (System.IO.Directory.Exists(sourcePath))
            {
                string[] getfiles = System.IO.Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in getfiles)
                {
                    // Use static Path methods to extract only the file name from the path.
                    fileName = System.IO.Path.GetFileName(s);
                    destFile = System.IO.Path.Combine(targetPath, fileName);
                    System.IO.File.Copy(s, destFile, true);

                    documents.Add(new Document()
                    {
                        DocumentPath = destFile.Substring(destFile.IndexOf(entityName) - 1),
                        MeetingID = meeting.MeetingID,
                        Meeting = meeting
                    });

                    // Apagar ficheiro na pasta temporária
                    System.IO.File.Delete(s);
                }
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
            }

            await _context.Documents.AddRangeAsync(documents);
            meeting.Documents = documents;
            _context.Meetings.Update(meeting);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Groups", new { id = meeting.GroupID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadDocuments(ICollection<IFormFile> files, int? id, string guid)
        {
            // Obtém o utilizador que está autenticado
            IdentityUser applicationUser = await _userManager.GetUserAsync(User);
            string email = applicationUser?.Email; // will give the user's Email
            var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

            // Vamos verificar se o utilizador que pretende agendar a reunião é responsável ou co-responsável pelo grupo
            var groupMember = _context.GroupMembers.Include(m => m.Group).ThenInclude(m => m.Entity).Where(m => m.GroupID == id).FirstOrDefault(m => m.User.Email == email);
            string roleUser = groupMember?.Role;

            // Aqui fazemos a verificação se é manager ou comanager do grupo
            if ((roleUser.ToLower() != "manager" && roleUser.ToLower() != "comanager") || roleUser == null)
            {
                return NotFound();
            }

            // Obter a que entidade está associado o grupo
            string entityName = groupMember.Group.Entity.Name;

            foreach (var file in files)
            {
                // Agora temos que escrever no ficheiro as credenciais de autenticação
                string destination = Path.Combine(_he.ContentRootPath, "wwwroot/", entityName, "Meetings", "Temp", email, guid, file.FileName);
                string directory = Path.GetDirectoryName(destination);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Creates a filestream to store the file listing
                FileStream fs = new FileStream(destination, FileMode.Create);

                try
                {
                    file.CopyTo(fs);
                    fs.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return Ok(Json("success"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadDocument(int? id)
        {
            // Obtém o utilizador que está autenticado
            IdentityUser applicationUser = await _userManager.GetUserAsync(User);
            string email = applicationUser?.Email; // will give the user's Email
            var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

            var document = _context.Documents.FirstOrDefault(m => m.DocumentID == id);

            // Antes de devolver o ficheiro: primeiro temos que verificar se o user que está a pedir o ficheiro faz parte do grupo
            var meeting = _context.Meetings.Include(m => m.Group)
                .ThenInclude(m => m.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefault(m => m.MeetingID == document.MeetingID);

            if (!meeting.Group.Members.Any(m => m.User.RegisteredUserID == user.RegisteredUserID))
            {
                return NoContent();
            }

            // Se o documento por alguma razão não está na bd, retorna erro
            if (document == null)
            {
                return NoContent();
            }

            var file = Path.Combine(_he.ContentRootPath, "wwwroot/", document.DocumentPath.TrimStart(new char[] { '/' }));
            // Se o ficheiro existir, vamos devolver esse ficheiro, caso contrário, vamos simplesmente devolver error
            if (System.IO.File.Exists(file))
            {
                // Lê todos os bytes do ficheiro
                byte[] fileBytes = System.IO.File.ReadAllBytes(file);
                return File(fileBytes, "application/x-msdownload", file);
            }
            else
            {
                return NoContent();
            }
        }

        // GET: Meetings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings
                .Include(m => m.Group)
                .ThenInclude(m => m.Members)
                .ThenInclude(m => m.User)
                .Include(m => m.Documents)
                .Include(m => m.Convocations)
                .FirstOrDefaultAsync(m => m.MeetingID == id);

            if (meeting == null)
            {
                return NotFound();
            }

            // Assim como no get, temos que fazer a verificação se o utilizador é manager do grupo
            // Obtém o utilizador que está autenticado
            IdentityUser applicationUser = await _userManager.GetUserAsync(User);
            string email = applicationUser?.Email; // will give the user's Email
            var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

            // Vamos verificar se o utilizador que pretende agendar a reunião é responsável ou co-responsável pelo grupo
            var groupMember = _context.GroupMembers.Include(m => m.Group).ThenInclude(m => m.Entity).Where(m => m.GroupID == meeting.GroupID).FirstOrDefault(m => m.User.Email == email);
            string roleUser = groupMember?.Role;

            // Aqui fazemos a verificação se é manager ou comanager do grupo
            if ((roleUser.ToLower() == "manager" || roleUser.ToLower() != "comanager"))
            {
                ViewData["Role"] = true;
            }
            return View(meeting);
        }

        // GET: Meetings/Create
        public IActionResult Create()
        {
            ViewData["GroupID"] = new SelectList(_context.Groups, "GroupID", "Name");
            return View();
        }

        // POST: Meetings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MeetingID,GroupID,Quorum,MeetingDate")] Meeting meeting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(meeting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GroupID"] = new SelectList(_context.Groups, "GroupID", "Name", meeting.GroupID);
            return View(meeting);
        }

        // GET: Meetings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
            {
                return NotFound();
            }
            ViewData["GroupID"] = new SelectList(_context.Groups, "GroupID", "Name", meeting.GroupID);
            return View(meeting);
        }

        // POST: Meetings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MeetingID,GroupID,Quorum,MeetingDate")] Meeting meeting)
        {
            if (id != meeting.MeetingID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meeting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingExists(meeting.MeetingID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GroupID"] = new SelectList(_context.Groups, "GroupID", "Name", meeting.GroupID);
            return View(meeting);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostPoneMeeting(int? id, DateTime dateTime, string timeDay)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Se o utilizador não escolheu a hora de marcação, volta para página
            if (timeDay == null)
                return View();

            var meeting = await _context.Meetings
                .Include(m => m.Group)
                .FirstOrDefaultAsync(m => m.MeetingID == id);
            if (meeting == null)
            {
                return NotFound();
            }

            // timeDay tem o seguinte formato "12:30"
            // Vamos fazer o split de acordo com os dois pontos
            string[] partsTime = timeDay.Split(':');
            DateTime meetingDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, Int32.Parse(partsTime[0]), Int32.Parse(partsTime[1]), 0);

            // Atribuir a data atualizada ao objeto meeting
            meeting.MeetingDate = meetingDate;

            // Guardar na base de dados
            _context.Meetings.Update(meeting);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Meetings", new { id = meeting.MeetingID });
        }

        // GET: Meetings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings
                .Include(m => m.Group)
                .FirstOrDefaultAsync(m => m.MeetingID == id);
            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // POST: Meetings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> StartMeeting(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings
                .Include(m => m.Group)
                .Include(m => m.Messages)
                .FirstOrDefaultAsync(m => m.MeetingID == id);

            meeting.MeetingStarted = DateTime.Now;

            if (meeting == null)
            {
                return NotFound();
            }

            await _context.SaveChangesAsync();
            return View(meeting);
        }

        [HttpPost, ActionName("SendMessage")]
        public async Task<IActionResult> SendMessage(int meetingID, string msg, 
            [FromServices] IHubContext<ChatHub> chatHub)
        {

            var sender = await _context.RegisteredUsers.FirstAsync(u => u.Email == User.Identity.Name);

            var message = new ChatMessage
            {
                Sender = sender.Name,
                MeetingID = meetingID,
                Text = msg,
                Timestamp = DateTime.Now

            };

            try
            {
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                await chatHub.Clients.Group(meetingID.ToString())
                    .SendAsync("ReceiveMessage", new
                    {
                        Text = message.Text,
                        Name = message.Sender,
                        Timestamp = message.Timestamp
                    });

                _iFlashMessage.Confirmation("Message Sent!");
                return Json(new { url = this.Url.Action("StartMeeting", new { id = meetingID }) });
            }
            catch
            {
                _iFlashMessage.Warning("ERRO!");
                return Json(new { url = this.Url.Action("StartMeeting", new { id = meetingID }) });
            }

            
        }

        private bool MeetingExists(int id)
        {
            return _context.Meetings.Any(e => e.MeetingID == id);
        }
    }
}
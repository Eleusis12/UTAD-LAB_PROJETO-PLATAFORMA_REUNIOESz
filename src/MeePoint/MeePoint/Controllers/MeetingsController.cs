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
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Borders;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.SignalR;
using MeePoint.Services;

namespace MeePoint.Controllers
{
	public class MeetingsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IHostEnvironment _he;

		public MeetingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHostEnvironment host)
		{
			_context = context;
			_userManager = userManager;
			_he = host;
		}

		// GET: Meetings
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.Meetings.Include(m => m.Group);
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: Attendance
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Attendance(int? id)
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

			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Vamos verificar se o utilizador que pretende agendar a reunião é responsável ou co-responsável pelo grupo
			string roleUser = _context.GroupMembers.Where(m => m.GroupID == meeting.GroupID).FirstOrDefault(m => m.User.Email == email)?.Role;

			// Aqui fazemos a verificação se é manager ou comanager do grupo
			if ((roleUser.ToLower() != "manager" && roleUser.ToLower() != "comanager") || roleUser == null)
			{
				return NotFound();
			}

			return View(meeting);
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

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> DownloadMinute(int? id)
		{
			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Antes de devolver o ficheiro: primeiro temos que verificar se o user que está a pedir o ficheiro faz parte do grupo
			var meeting = _context.Meetings.Include(m => m.Group)
				.ThenInclude(m => m.Members)
				.ThenInclude(m => m.User)
				.FirstOrDefault(m => m.MeetingID == id);

			if (!meeting.Group.Members.Any(m => m.User.RegisteredUserID == user.RegisteredUserID))
			{
				return NoContent();
			}

			// Se a ata por alguma razão não está na bd, retorna erro
			if (meeting.AtaPath == string.Empty)
			{
				return NoContent();
			}

			var file = Path.Combine(_he.ContentRootPath, "wwwroot/", meeting.AtaPath.TrimStart(new char[] { '/' }));

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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Present(int? meetingID, int? userID, bool present, bool missed)
		{
			if (meetingID == null || userID == null)
			{
				return BadRequest();
			}

			var meeting = await _context.Meetings
				.Include(m => m.Group)
				.ThenInclude(m => m.Members)
				.ThenInclude(m => m.User)
				.Include(m => m.Documents)
				.Include(m => m.Convocations)
				.FirstOrDefaultAsync(m => m.MeetingID == meetingID);

			try
			{
				// Atualizar a presença do utilizador
				var convocation = meeting.Convocations.FirstOrDefault(m => m.UserID == userID);
				convocation.Answer = present;
				_context.Convocations.Update(convocation);
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{
				return NoContent();
			}

			return View();
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
                        Timestamp = message.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
					});

                return Json(new { url = this.Url.Action("StartMeeting", new { id = meetingID }) });
            }
            catch
            {
                return Json(new { url = this.Url.Action("StartMeeting", new { id = meetingID }) });
            }

            
        }
		private bool MeetingExists(int id)
		{
			return _context.Meetings.Any(e => e.MeetingID == id);
		}

		protected void GenerateMinutePDF(Meeting meeting, string entityName)
		{
			if (meeting.MeetingID < 0)
				return;

			// Obter o caminho onde vai ser armazenado o ficheiro
			string targetPath = Path.Combine(_he.ContentRootPath, "wwwroot/", entityName, "Meetings/", meeting.MeetingID + "/");
			string destFile = System.IO.Path.Combine(targetPath, "ata.pdf");

			// Criar Ficheiro
			FileInfo file = new FileInfo(destFile);
			if (!file.Directory.Exists) file.Directory.Create();

			// Must have write permissions to the path folder
			PdfWriter writer = new PdfWriter(destFile);
			PdfDocument pdf = new PdfDocument(writer);
			iText.Layout.Document document = new iText.Layout.Document(pdf);
			Paragraph header = new Paragraph("Atas de Reunião")
			   .SetTextAlignment(TextAlignment.CENTER)
			   .SetFontSize(20);

			document.Add(header);

			Paragraph subheader = new Paragraph(entityName)
			   .SetTextAlignment(TextAlignment.CENTER)
			   .SetFontSize(15);

			document.Add(subheader);

			// Line separator
			LineSeparator ls = new LineSeparator(new SolidLine());
			document.Add(ls);

			AddTitle(document, "Abertura");
			AddText(document, $"A reunião ordinária da {entityName}, devidamente convocada e realizada em {DateTime.Now.ToString("d MMM")}, começando às {DateTime.Now.ToString("HH:mm")}. ");
			AddTitle(document, "Estiveram Presentes");

			foreach (var convocation in meeting.Convocations.Where(m => m.Answer == true))
			{
				AddParticipant(document, convocation.User.Name);
			}

			AddTitle(document, "Assuntos Discutidos");

			AddText(document, meeting.Name);

			// Obter o nome do Responsável
			string manager = meeting.Group.Members.FirstOrDefault(m => m.Role.ToLower() == "manager").User.Name;

			AddSignature(document, manager, "Co-Responsável");

			meeting.AtaPath = destFile.Substring(destFile.IndexOf(entityName) - 1);

			// Não queremos perturbar os outros dados relacionados com a reunião
			// De forma a assegurar que apenas o campo da ata é alterada, temos que fazer assim
			_context.Meetings.Attach(meeting).Property(x => x.AtaPath).IsModified = true;
			_context.SaveChanges();

			document.Close();
		}

		private static void AddTitle(iText.Layout.Document document, string titleName)
		{
			PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

			Paragraph paragraph1 = new Paragraph(titleName)
				.SetTextAlignment(TextAlignment.LEFT)
				.SetFont(font)
				.SetFontSize(14);
			document.Add(paragraph1);
		}

		private static void AddText(iText.Layout.Document document, string text)
		{
			PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);

			Paragraph paragraph1 = new Paragraph(text)
				.SetTextAlignment(TextAlignment.JUSTIFIED)
				.SetFont(font)
				.SetFontSize(12); ;

			document.Add(paragraph1);
		}

		private static void AddParticipant(iText.Layout.Document document, string text)
		{
			PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ITALIC);

			Paragraph paragraph1 = new Paragraph(text)
				.SetTextAlignment(TextAlignment.LEFT)
				.SetFont(font)
				.SetFontSize(12); ;

			document.Add(paragraph1);
		}

		private static void AddSignature(iText.Layout.Document document, string manager, string coManager)
		{
			PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);

			Table table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth().SetMarginTop(40);

			// Adiciona o espaço para cada um dos responsáveis poder assinar depois de imprimir
			Paragraph signatureManager = new Paragraph(manager).SetFont(font)
				.SetFontSize(11);

			Paragraph signatureCoManager = new Paragraph(coManager).SetFont(font)
				.SetFontSize(11);

			LineSeparator dottedline = new LineSeparator(new DottedLine()).SetMaxWidth(200).SetMarginTop(10);

			AddCell(table, signatureManager);
			AddCell(table, signatureCoManager);

			AddCell(table, dottedline);
			AddCell(table, dottedline);

			document.Add(table);
			document.Close();

			static void AddCell(Table table, IBlockElement signature)
			{
				Cell cell = new Cell();
				cell.Add(signature);
				cell.SetBorder(Border.NO_BORDER);
				table.AddCell(cell);
			}
		}
	}
}
using MeePoint.Filters;
using MeePoint.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MeePoint.Interfaces;
using MeePoint.ViewModels;
using MeePoint.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace MeePoint.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "EntityManager,User")]
        public ActionResult MainPage()
        {

            return View();
        }

        [Authorize(Roles = "EntityManager,User")]
        public async Task<ActionResult> GetCalendarData()
        {
            // Obtém o utilizador que está autenticado
            IdentityUser applicationUser = await _userManager.GetUserAsync(User);
            string email = applicationUser?.Email; // will give the user's Email
            var user = await _context.RegisteredUsers
                .Include(m => m.Convocations)
                .ThenInclude(m => m.Meeting)
                .ThenInclude(m => m.Group)
                .FirstOrDefaultAsync(m => m.Email == email);

            // Queremos obter a lista de reuniões agendadas para o específico user
            ICollection<Convocation> convocations = user.Convocations;

            // Inicializar lista de reuniões a enviar para a View
            List<CalendarEvent> events = new List<CalendarEvent>();
            var random = new Random();

            try
            {
                foreach (Convocation conv in convocations)
                {
                    // Para cada grupo queremos enviar cores diferentes, de forma a poder distinguir as reuniões de acordo com o grupo
                    var color = String.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9"

                    events.Add(new CalendarEvent()
                    {
                        title = conv.Meeting.Group.Name,
                        description = conv.Meeting.Name,
                        id = conv.MeetingID,
                        backgroundColor = color,
                        borderColor = color,
                        allDay = "false",
                        start = conv.Meeting.MeetingDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        end = (conv.Meeting.MeetingDate.AddMinutes(conv.Meeting.ExpectedDuration)).ToString("yyyy-MM-ddTHH:mm:ss")
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new List<CalendarEvent>());
            }

            return Json(events);
        }

        public IActionResult MeePoint()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
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

		[Authorize]
		public ActionResult MainPage()
		{
			//// Obtém o utilizador que está autenticado
			//IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			//string email = applicationUser?.Email; // will give the user's Email
			//var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			//var entity = _context.Groups.FirstOrDefault(m => m.Name.ToLower() == "main")
			//MainPageViewModel mainPageViewModel = new MainPageViewModel()
			//{
			//	AmountGroups = _context.GroupMembers.Where(m => m.UserID == user.RegisteredUserID).Count(),
			//	//AmountMeetingsHeldLastMonth = _context.Meetings.Where(m=> m.GroupID).Where(m => m.MeetingDate >= DateTime.Now.AddDays(-30))
			//	EntityName = _context.Entities.FirstOrDefault(m => m.EntityID == user.Groups)
			//}

			return View();
		}

		public async Task<ActionResult> GetCalendarData()
		{
			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Queremos obter a lista de reuniões agendadas para o específico user
			Dictionary<int, ICollection<Meeting>> meetings = _context.GroupMembers
				.Include(m => m.Group)
				.ThenInclude(m => m.Meetings)
				.Where(m => m.UserID == user.RegisteredUserID)
				.Select(x => new KeyValuePair<int, ICollection<Meeting>>(x.GroupID, x.Group.Meetings))
				.ToDictionary(x => x.Key, x => x.Value);

			// Inicializar lista de reuniões a enviar para a View
			List<CalendarEvent> events = new List<CalendarEvent>();
			var random = new Random();

			try
			{
				foreach (KeyValuePair<int, ICollection<Meeting>> keyPair in meetings)
				{
					// Para cada grupo queremos enviar cores diferentes, de forma a poder distinguir as reuniões de acordo com o grupo
					var color = String.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9"

					foreach (Meeting meeting in keyPair.Value)
					{
						events.Add(new CalendarEvent()
						{
							title = meeting.Group.Name,
							description = meeting.Name,
							id = meeting.MeetingID,
							backgroundColor = color,
							borderColor = color,
							allDay = "false",
							start = meeting.MeetingDate.ToString("yyyy-MM-ddTHH:mm:ss"),
							end = (meeting.MeetingDate.AddMinutes(meeting.ExpectedDuration)).ToString("yyyy-MM-ddTHH:mm:ss")
						});
					}
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
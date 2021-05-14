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

namespace MeePoint.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		[Authorize]
		public IActionResult MainPage()
		{
			return View();
		}

		public ActionResult GetCalendarData()
		{
			List<CalendarEvent> events = new List<CalendarEvent>();
			CalendarEvent calendarEvent = new CalendarEvent()
			{
				title = "Ola mundo",
				description = "Reunião muito divertida",
				id = 2,
				backgroundColor = "000000",
				borderColor = "ffffff",
				allDay = "false",
				end = DateTime.Now.AddDays(4).ToString("yyyy-MM-ddTHH:mm:ss"),
				start = DateTime.Now.AddDays(4).ToString("yyyy-MM-ddTHH:mm:ss")
			};

			CalendarEvent calendarEvent2 = new CalendarEvent()
			{
				title = "asddsa",
				description = "Reunião muito divertida",
				id = 2,
				backgroundColor = "000000",
				borderColor = "ffffff",
				allDay = "false",
				end = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"),
				start = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss")
			};

			events.Add(calendarEvent);
			events.Add(calendarEvent2);

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
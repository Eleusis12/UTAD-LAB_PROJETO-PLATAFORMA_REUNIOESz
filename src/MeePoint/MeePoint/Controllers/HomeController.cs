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
			List<AvailibilityDto> data = new List<AvailibilityDto>();

			//Statically create list and add data
			AvailibilityDto infoObj1 = new AvailibilityDto();
			infoObj1.Id = 1;
			infoObj1.Title = "I am available";
			infoObj1.Desc = "Description 1";
			infoObj1.Start_Date = "2020-08-16 22:37:22.467";
			infoObj1.End_Date = "2020-08-16 23:30:22.467";
			data.Add(infoObj1);

			AvailibilityDto infoObj2 = new AvailibilityDto();
			infoObj2.Id = 2;
			infoObj2.Title = "Available";
			infoObj2.Desc = "Description 1";
			infoObj2.Start_Date = "2020-08-17 10:00:22.467";
			infoObj2.End_Date = "2020-08-17 11:00:22.467";
			data.Add(infoObj2);

			AvailibilityDto infoObj3 = new AvailibilityDto();
			infoObj3.Id = 3;
			infoObj3.Title = "Meeting";
			infoObj3.Desc = "Description 1";
			infoObj3.Start_Date = "2020-08-18 07:30:22.467";
			infoObj3.End_Date = "2020-08-18 08:00:22.467";
			data.Add(infoObj3);

			return Json(data);
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
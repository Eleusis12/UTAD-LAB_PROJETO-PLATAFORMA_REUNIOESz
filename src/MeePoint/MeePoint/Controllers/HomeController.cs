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

namespace MeePoint.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IEmailService _emailService;

		public HomeController(ILogger<HomeController> logger, IEmailService emailService)
		{
			_logger = logger;
			_emailService = emailService;
		}

		[Authorize]
		public IActionResult MainPage()
		{
			return View();
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
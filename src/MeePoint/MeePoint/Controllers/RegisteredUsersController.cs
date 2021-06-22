using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MeePoint.Data;
using MeePoint.Models;

namespace MeePoint.Controllers
{
	public class RegisteredUsersController : Controller
	{
		private readonly ApplicationDbContext _context;

		public RegisteredUsersController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: RegisteredUsers/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var registeredUser = await _context.RegisteredUsers
				.FirstOrDefaultAsync(m => m.RegisteredUserID == id);
			if (registeredUser == null)
			{
				return NotFound();
			}

			return View(registeredUser);
		}
	}
}
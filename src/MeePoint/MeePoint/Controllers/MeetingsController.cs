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

namespace MeePoint.Controllers
{
	public class MeetingsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public MeetingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
		{
			_context = context;
			_userManager = userManager;
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
			string roleUser = _context.GroupMembers.Where(m => m.GroupID == id).FirstOrDefault(m => m.User.Email == email).Role;

			//if (roleUser.ToLower() != "manager" && roleUser.ToLower() != "comanager")
			//{
			//	return NotFound();
			//}

			List<int> expectedDuration = new List<int>() {
				5,10,15,20,30,40,60,120,
			};

			ViewBag.GroupId = id;
			ViewBag.ExpectedDuration = new SelectList(expectedDuration);

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Schedule(Meeting meeting)
		{
			return Content(meeting.ToString());
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
				.FirstOrDefaultAsync(m => m.MeetingID == id);
			if (meeting == null)
			{
				return NotFound();
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

		private bool MeetingExists(int id)
		{
			return _context.Meetings.Any(e => e.MeetingID == id);
		}
	}
}
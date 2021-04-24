using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MeePoint.Data;
using MeePoint.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MeePoint.Controllers
{
	public class GroupsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;

		public GroupsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
		}

		// GET: Groups
		[Authorize(Roles = "EntityManager")]
		public async Task<IActionResult> Index()
		{
			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Verifica se o utilizador que quer aceder a esta página é de facto um entityManager de alguma entidade
			var entity = _context.Entities.First(m => m.User.Email == user.Email);

			// Entidade == null, significa que o utilizador não tem permissões. FORBIDDEN
			if (entity == null)
				return StatusCode(403);

			var applicationDbContext = _context.Groups.Include(m => m.Entity).Include(m => m.Members).Include("Members.User").Where(m => m.EntityID == entity.EntityID).Where(m => m.Name.ToLower() != "main".ToLower());
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: Groups/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @group = await _context.Groups
				.Include(m => m.Entity)
				.FirstOrDefaultAsync(m => m.GroupID == id);
			if (@group == null)
			{
				return NotFound();
			}

			return View(@group);
		}

		[Authorize(Roles = "EntityManager")]
		public async Task<IActionResult> Create()
		{
			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Verifica se o utilizador que quer aceder a esta página é de facto um entityManager de alguma entidade
			var entity = _context.Entities.First(m => m.User.Email == user.Email);

			// Entidade == null, significa que o utilizador não tem permissões. FORBIDDEN
			if (entity == null)
				return StatusCode(403);

			// Enviamos apenas os membros que fazem parte da Entidade
			// Todos os membros da entidade, estão num grupo com o nome de "main"
			ViewBag.GroupMembers = _context.GroupMembers
			   .Include(m => m.Group)
			   .Include("Group.Entity")
			   .Include(m => m.User)
			   .Where(m => m.Group.EntityID == entity.EntityID)
			   .Where(m => m.Group.Name.ToLower() == "main".ToLower())
			   .Select(c => new SelectListItem()
			   { Text = c.User.Email, Value = c.User.RegisteredUserID.ToString() })
			   .ToList();

			return View();
		}

		// POST: Groups/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to, for
		// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "EntityManager")]
		public async Task<IActionResult> Create(string groupName, int[] managers, int[] coManagers, int[] participants)
		{
			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Obtém a entidade que pretende adicionar um novo grupo
			var entity = _context.Entities.First(m => m.User.Email == user.Email);

			// Proceder à criação do grupo
			Group group = new Group()
			{
				Entity = entity,
				EntityID = entity.EntityID,
				Name = groupName,
				Members = new List<GroupMember>()
			};

			// Adicionar os managers ao membros do grupo
			foreach (var manager in managers)
			{
				group.Members.Add(new GroupMember() { Group = group, GroupID = group.GroupID, UserID = manager, User = _context.RegisteredUsers.FirstOrDefault(m => m.RegisteredUserID == manager), Role = "Manager" });
			}
			// Adicionar os comanagers aos membros do grupo
			foreach (var coManager in coManagers)
			{
				group.Members.Add(new GroupMember() { Group = group, GroupID = group.GroupID, UserID = coManager, User = _context.RegisteredUsers.FirstOrDefault(m => m.RegisteredUserID == coManager), Role = "CoManager" });
			}
			// Adicionar os participantes aos membros do grupo
			foreach (var participant in participants)
			{
				group.Members.Add(new GroupMember() { Group = group, GroupID = group.GroupID, UserID = participant, User = _context.RegisteredUsers.FirstOrDefault(m => m.RegisteredUserID == participant), Role = "Participant" });
			}

			if (TryValidateModel(group))
			{
				_context.Add(group);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Create));
		}

		// GET: Groups/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @group = await _context.Groups.FindAsync(id);
			if (@group == null)
			{
				return NotFound();
			}
			ViewData["EntityID"] = new SelectList(_context.Entities, "EntityID", "Name", @group.EntityID);
			return View(@group);
		}

		// POST: Groups/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to, for
		// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("GroupID,Name,EntityID")] Group @group)
		{
			if (id != @group.GroupID)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(@group);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!GroupExists(@group.GroupID))
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
			ViewData["EntityID"] = new SelectList(_context.Entities, "EntityID", "Name", @group.EntityID);
			return View(@group);
		}

		// GET: Groups/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @group = await _context.Groups
				.Include(m => m.Entity)
				.FirstOrDefaultAsync(m => m.GroupID == id);
			if (@group == null)
			{
				return NotFound();
			}

			return View(@group);
		}

		// POST: Groups/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var @group = await _context.Groups.FindAsync(id);
			_context.Groups.Remove(@group);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool GroupExists(int id)
		{
			return _context.Groups.Any(e => e.GroupID == id);
		}
	}
}
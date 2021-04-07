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
	public class EntitiesController : Controller
	{
		private readonly ApplicationDbContext _context;

		public EntitiesController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET : Apresenta uma lista de Entidades
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.Entities.Include(e => e.User);
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: Apresenta os detalhes de uma determinada entidade
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var entity = await _context.Entities
				.Include(e => e.User)
				.FirstOrDefaultAsync(m => m.EntityID == id);
			if (entity == null)
			{
				return NotFound();
			}

			return View(entity);
		}

		// GET: Apresenta página no qual o administrador possa adicionar uma nova entidade
		public IActionResult Create()
		{
			return View();
		}

		// POST: Permite adicionar uma nova entidade

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("EntityID,NIF,Name,Description,PhoneNumber,ManagerName,StatusEntity,SubscriptionDays,MaxUsers,PostalCode,Address,Manager,User")] Entity entity)
		{
			entity.SubscriptionDateStart = DateTime.Now;
			entity.SubscriptionDateEnd = entity.SubscriptionDateStart.AddDays(entity.SubscriptionDays);

			// Limpar os erros
			ModelState.Clear();

			if (TryValidateModel(entity))
			{
				_context.Add(entity);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["Manager"] = new SelectList(_context.RegisteredUsers, "RegisteredUserID", "Email", entity.Manager);
			return View(entity);
		}

		// GET: Apresenta página no qual o administrador possa alterar dados das entidades
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var entity = await _context.Entities.FindAsync(id);
			if (entity == null)
			{
				return NotFound();
			}
			ViewData["Manager"] = new SelectList(_context.RegisteredUsers, "RegisteredUserID", "Email", entity.Manager);
			return View(entity);
		}

		// POST: Permite alterar dados de uma entidade já existente
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("EntityID,NIF,Name,Description,PhoneNumber,ManagerName,StatusEntity,SubscriptionDays,MaxUsers,PostalCode,Address,Manager,User")] Entity entity)
		{
			if (id != entity.EntityID)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(entity);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!EntityExists(entity.EntityID))
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
			ViewData["Manager"] = new SelectList(_context.RegisteredUsers, "RegisteredUserID", "Email", entity.Manager);
			return View(entity);
		}

		// GET: Apresenta página no qual pede ao administrado a confirmação de eliminiação de uma entidade
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var entity = await _context.Entities
				.Include(e => e.User)
				.FirstOrDefaultAsync(m => m.EntityID == id);
			if (entity == null)
			{
				return NotFound();
			}

			return View(entity);
		}

		// POST: Permite Eliminar uma Entidade
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var entity = await _context.Entities.FindAsync(id);
			_context.Entities.Remove(entity);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool EntityExists(int id)
		{
			return _context.Entities.Any(e => e.EntityID == id);
		}
	}
}
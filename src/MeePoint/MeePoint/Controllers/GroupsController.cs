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
using Vereyon.Web;
using static MeePoint.Models.Group;

namespace MeePoint.Controllers
{
	[Route("[controller]/[action]")]
	public class GroupsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly IFlashMessage _iFlashMessage;

		public GroupsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IFlashMessage flashMessage)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
			_iFlashMessage = flashMessage;
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
		[Authorize(Roles = "EntityManager,User")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var @group = await _context.Groups
				.Include(m => m.Entity)
				.Include(m => m.Members)
				.Include(m => m.Meetings)
				.Include("Entity.User")
				.Include("Members.User")
				.FirstOrDefaultAsync(m => m.GroupID == id);

			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Agora temos que confirmar se o utilizador que pretende aceder a esta informação, pertence ao grupo
			var member = group.Members.SingleOrDefault(m => m.User.RegisteredUserID == user.RegisteredUserID);

			// Verificar se o utilizador é manager da entidade
			var entityManager = group.Entity.User.RegisteredUserID == user.RegisteredUserID;

			// Se o utilizador que estiver a efetuar este pedido não faz parte do grupo ou não é o manager
			if (member == null && entityManager == false)
			{
				// NotFound, porque em teoria se o utilizador n pertence a este grupo, não tem direito de saber sequer da existência dele
				return NotFound();
			}

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
		public async Task<IActionResult> Create(string groupName, int manager, int[] coManagers, int[] participants)
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

			// Primeiro temos que assegurar que não existem números em comum
			// Se é um manager, então não pode ser co-manager ou participant, vamos remover o manager da lista de coManager e participants caso exista
			coManagers = coManagers.Where(val => val != manager).ToArray();
			participants = participants.Where(val => val != manager).ToArray();

			// Agora vamos verificar os elementos comuns entre os coManagers e os participantes, No caso de interseção, esse elementos são retirados da lista participantes (O elemento mantém-se apenas como coManager)

			var participantsList = participants.ToList().Except(coManagers);

			// Agora que as incongruências foram resolvidas, basta apenas adicionar os membros ao grupo
			// Adicionar o manager ao grupo
			group.Members.Add(new GroupMember() { Group = group, GroupID = group.GroupID, UserID = manager, User = _context.RegisteredUsers.FirstOrDefault(m => m.RegisteredUserID == manager), Role = GroupRoles.Manager.ToString() });

			// Adicionar os comanagers aos membros do grupo
			foreach (var coManager in coManagers)
			{
				group.Members.Add(new GroupMember() { Group = group, GroupID = group.GroupID, UserID = coManager, User = _context.RegisteredUsers.FirstOrDefault(m => m.RegisteredUserID == coManager), Role = GroupRoles.CoManager.ToString() });
			}
			// Adicionar os participantes aos membros do grupo
			foreach (var participant in participantsList)
			{
				group.Members.Add(new GroupMember() { Group = group, GroupID = group.GroupID, UserID = participant, User = _context.RegisteredUsers.FirstOrDefault(m => m.RegisteredUserID == participant), Role = GroupRoles.Participant.ToString() });
			}

			if (TryValidateModel(group))
			{
				_context.Add(group);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Create));
		}

		//// GET: Groups/Edit/5
		//[Authorize(Roles = "EntityManager")]
		//public async Task<IActionResult> Edit(int? id)
		//{
		//    if (id == null)
		//    {
		//        return NotFound();
		//    }

		//    var @group = await _context.Groups.FindAsync(id);
		//    if (@group == null)
		//    {
		//        return NotFound();
		//    }
		//    ViewData["EntityID"] = new SelectList(_context.Entities, "EntityID", "Name", @group.EntityID);
		//    return View(@group);
		//}

		//// POST: Groups/Edit/5
		//// To protect from overposting attacks, enable the specific properties you want to bind to, for
		//// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//[Authorize(Roles = "EntityManager")]
		//public async Task<IActionResult> Edit(int id, [Bind("GroupID,Name,EntityID")] Group @group)
		//{
		//    if (id != @group.GroupID)
		//    {
		//        return NotFound();
		//    }

		//    if (ModelState.IsValid)
		//    {
		//        try
		//        {
		//            _context.Update(@group);
		//            await _context.SaveChangesAsync();
		//        }
		//        catch (DbUpdateConcurrencyException)
		//        {
		//            if (!GroupExists(@group.GroupID))
		//            {
		//                return NotFound();
		//            }
		//            else
		//            {
		//                throw;
		//            }
		//        }
		//        return RedirectToAction(nameof(Index));
		//    }
		//    ViewData["EntityID"] = new SelectList(_context.Entities, "EntityID", "Name", @group.EntityID);
		//    return View(@group);
		//}

		// GET: Groups/Delete/5
		[HttpGet("{id}")]
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
		[HttpPost("{id}"), ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var @group = await _context.Groups.FindAsync(id);
			_context.Groups.Remove(@group);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		[HttpPost, ActionName("ManageGroupRole")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "EntityManager, User")]
		public async Task<IActionResult> ManageGroupRole(int id, int groupID, string role)
		{
			var groupMember = await _context.GroupMembers.Include(gm => gm.User).FirstAsync(gm => gm.UserID == id && gm.GroupID == groupID);

			try
			{
				if (role == GroupRoles.Manager.ToString())
				{
					var currentManager = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupID == groupID && gm.Role == GroupRoles.Manager.ToString());

					//Demote current manager to participant
					if (currentManager != null)
					{
						currentManager.Role = GroupRoles.Participant.ToString();
					}
				}

				groupMember.Role = ((GroupRoles)Enum.Parse(typeof(GroupRoles), role)).ToString();

				await _context.SaveChangesAsync();
				_iFlashMessage.Confirmation("Role changed!");
				return Json(new { url = this.Url.Action("Participants", new { id = groupMember.GroupID }) });
			}
			catch
			{
				_iFlashMessage.Warning("ERRO!");
				return Json(new { url = this.Url.Action("Participants", new { id = groupMember.GroupID }) });
			}
		}

		// Assegurar que apenas os membros podem ver esta informação
		[Authorize(Roles = "EntityManager,User")]
		// GET: Groups/Participants
		public async Task<IActionResult> Participants(int? id)
		{
			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Verifica se o int recebido é válido
			if (id == null)
			{
				return NotFound();
			}

			// Incluir os membros do grupo
			var group = _context.Groups.Include(m => m.Entity).Include("Entity.User").Include(m => m.Members).Include("Members.User").FirstOrDefault(m => m.GroupID == id);

			// Não encontrou o grupo na base de dados
			if (group == null)
			{
				return NotFound();
			}

			// Agora temos que confirmar se o utilizador que pretende aceder a esta informação, pertence ao grupo
			var member = group.Members.SingleOrDefault(m => m.User.RegisteredUserID == user.RegisteredUserID);

			// Verificar se o utilizador é manager da entidade
			var entityManager = group.Entity.User.RegisteredUserID == user.RegisteredUserID;

			// Se o utilizador que estiver a efetuar este pedido não faz parte do grupo ou não é o manager
			if (member == null && entityManager == false)
			{
				// NotFound, porque em teoria se o utilizador n pertence a este grupo, não tem direito de saber sequer da existência dele
				return NotFound();
			}

			ViewData["GroupManager"] = group.Members.Where(m => m.Role == "Manager" || m.Role == "CoManager").Select(m => m.User.Email).ToList();

			return View(group);
		}

		[HttpPost, ActionName("RemoveParticipant")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveParticipant(int userID, int groupID)
		{
			var groupMember = await _context.GroupMembers.Include(gm => gm.User).FirstAsync(gm => gm.UserID == userID && gm.GroupID == groupID);

			try
			{
				_context.GroupMembers.Remove(groupMember);

				await _context.SaveChangesAsync();

				_iFlashMessage.Confirmation("Participant Removed!");
				return Json(new { url = this.Url.Action("Participants", new { id = groupMember.GroupID }) });
			}
			catch
			{
				_iFlashMessage.Warning("ERRO!");
				return Json(new { url = this.Url.Action("Participants", new { id = groupMember.GroupID }) });
			}
		}

		[HttpGet("{groupID}")]
		[Authorize(Roles = "EntityManager, User")]
		public async Task<IActionResult> AddParticipants(int? groupID)
		{
			if (groupID == null)
			{
				return NotFound();
			}

			var group = await _context.Groups.FindAsync(groupID);

			if (group == null)
			{
				return NotFound();
			}

			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = await _context.RegisteredUsers.FirstOrDefaultAsync(m => m.Email == email);
			var member = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.UserID == user.RegisteredUserID);

			// Verifica se o utilizador que quer aceder a esta página é de facto o entityManager desta entidade
			var entity = await _context.Entities.FirstAsync(m => m.User.Email == user.Email && m.EntityID == group.EntityID);

			// Se o user nao for manager desta entidade onde nao for manager do grupo
			if (entity == null || member.Role == "Participant")
			{
				return StatusCode(403);
			}

			var currentParticipants = _context.GroupMembers
				.Include(gm => gm.Group)
				.Include("Group.Entity")
				.Include(gm => gm.User)
				.Where(gm => gm.GroupID == groupID)
				.Select(gm => gm.UserID);

			var entityParticipantsNotInGroup = _context.GroupMembers
			   .Include(m => m.Group)
			   .Include("Group.Entity")
			   .Include(m => m.User)
			   .Where(m => m.Group.EntityID == entity.EntityID)
			   .Where(m => m.Group.Name.ToLower() == "main".ToLower())
			   .Where(m => !currentParticipants.Contains(m.UserID));

			ViewBag.GroupMembers = entityParticipantsNotInGroup
			   .Select(c => new SelectListItem()
			   { Text = c.User.Email, Value = c.User.RegisteredUserID.ToString() })
			   .ToList();

			ViewData["EntityID"] = new SelectList(_context.Entities, "EntityID", "Name", group.EntityID);
			return View(group);
		}

		[HttpPost("{groupID}"), ActionName("AddParticipants")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "EntityManager, User")]
		public async Task<IActionResult> AddParticipants(int groupID, int[] newParticipants)
		{
			var group = await _context.Groups
				.Include(g => g.Members)
				.FirstAsync(g => g.GroupID == groupID);

			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = await _context.RegisteredUsers.FirstOrDefaultAsync(m => m.Email == email);
			var member = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.UserID == user.RegisteredUserID);

			// Verifica se o utilizador que quer aceder a esta página é de facto o entityManager desta entidade
			var entity = await _context.Entities.FirstAsync(m => m.User.Email == user.Email && m.EntityID == group.EntityID);

			// Se o user nao for manager desta entidade onde nao for manager do grupo
			if (entity == null || member.Role == "Participant")
			{
				return StatusCode(403);
			}
			foreach (var usr in newParticipants)
			{
				var newParticipant = await _context.RegisteredUsers.FirstOrDefaultAsync(m => m.RegisteredUserID == usr);

				group.Members.Add(new GroupMember() { User = newParticipant, Group = group, Role = "Participant" });
			}

			await _context.SaveChangesAsync();

			return RedirectToAction("Participants", new { id = groupID });
		}

		// Assegurar que apenas os membros podem ver esta informação
		[Authorize(Roles = "EntityManager,User")]
		// GET: Groups/Participants
		public async Task<IActionResult> History(int? id)
		{
			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Verifica se o int recebido é válido
			if (id == null)
			{
				return NotFound();
			}

			// Incluir os membros do grupo
			var group = _context.Groups.Include(m => m.Meetings).Include(m => m.Entity).Include("Entity.User").Include(m => m.Members).Include("Members.User").FirstOrDefault(m => m.GroupID == id);

			// Não encontrou o grupo na base de dados
			if (group == null)
			{
				return NotFound();
			}

			// Agora temos que confirmar se o utilizador que pretende aceder a esta informação, pertence ao grupo
			var member = group.Members.SingleOrDefault(m => m.User.RegisteredUserID == user.RegisteredUserID);

			// Verificar se o utilizador é manager da entidade
			var entityManager = group.Entity.User.RegisteredUserID == user.RegisteredUserID;

			// Se o utilizador que estiver a efetuar este pedido não faz parte do grupo ou não é o manager
			if (member == null && entityManager == false)
			{
				// NotFound, porque em teoria se o utilizador n pertence a este grupo, não tem direito de saber sequer da existência dele
				return NotFound();
			}

			return View(group);
		}

		private bool GroupExists(int id)
		{
			return _context.Groups.Any(e => e.GroupID == id);
		}
	}
}
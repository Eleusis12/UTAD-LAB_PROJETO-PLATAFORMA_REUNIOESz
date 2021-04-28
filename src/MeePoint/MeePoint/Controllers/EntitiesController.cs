using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MeePoint.Data;
using MeePoint.Models;
using MeePoint.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.IO;


namespace MeePoint.Controllers
{
	[Route("[controller]/[action]")]
	public class EntitiesController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IHostEnvironment _he;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;

		
		public EntitiesController(ApplicationDbContext context, IHostEnvironment host,
			UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
		{
			_context = context;
			_he = host;
			_userManager = userManager;
			_signInManager = signInManager;
		}

		// GET : Apresenta uma lista de Entidades
		[Authorize(Roles = "Administrator")]
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.Entities.Include(e => e.User);
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: Apresenta os detalhes de uma determinada entidade
		[Authorize(Roles = "Administrator")]
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
		[Authorize(Roles = "Administrator")]
		public IActionResult Create()
		{
			return View();
		}

		// POST: Permite adicionar uma nova entidade

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Administrator")]
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Administrator")]
		public IActionResult ApproveEntity(int? id)
		{
			// The Post method is incomplete
			if (id == null)
			{
				return BadRequest();
			}

			Entity entity = new Entity();

			// Obtain correct Entity object
			entity = _context.Entities.FirstOrDefault(x => x.EntityID == id);

			// Update Entity status
			entity.StatusEntity = true;

			// Save Changes
			_context.SaveChanges();

			return PartialView("_EntitiesTablePartial", _context.Entities);
		}

		[HttpGet]
		[Authorize(Roles = "EntityManager")]
		public IActionResult AddLogins()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> AddLogins(string emails)
		{
			// Se o utilizador submeteu sem ter preenchido o campo de emails
			if (emails == null)
			{
				return View();
			}

			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Obtém a entidade
			var entity = _context.Entities.Include(m => m.Groups).Include("Groups.Members").First(m => m.User.Email == user.Email);

			// Se a empresa não existe
			if (entity == null)
			{
				return StatusCode(401);
			}
			// Agora queremos efetuar o parsing dos emails recebidos
			// Exemplo: [\"franciscomeireles-10@hotmail.com\",\"franciscomeireles100@gmail.com\"]
			emails = emails.Replace("\"", "");
			emails = emails.Replace("[", "");
			emails = emails.Replace("]", "");
			emails = emails.Replace("\\", "");

			// Agora os emails estão separados apenas por vírgulas
			string[] emailsParsed = emails.Split(',');
			Group group;
			if (entity.Groups == null)
			{

				// TODO: Verificar se isto está correto
				// Não existe o grupo main dentro da entidade
				group = new Group();
				group.Entity = entity;
				group.EntityID = entity.EntityID;
				group.Name = "main";
				List<Group> Groups = new List<Group>();
				Groups.Add(group);
				entity.Groups = Groups;
			}
			else
			{
				// Vamos verificar se a entidade possui o "main" grupo
				group = entity.Groups.FirstOrDefault(m => m.Name == "main");
				if (group == null)
				{
					// Não existe o grupo main dentro da entidade
					group = new Group();
					group.Entity = entity;
					group.EntityID = entity.EntityID;
					group.Name = "main";
					entity.Groups.Add(group);
				}
			}

			IdentityUser IsUserRegistered;
			List<Tuple<string, string>> credentials = new List<Tuple<string, string>>();
			foreach (var singleEmail in emailsParsed)
			{
				IsUserRegistered = await _userManager.FindByEmailAsync(singleEmail);

				// O utilizador já está registado na base de dados, vamos passar para a próxima iteração
				if (IsUserRegistered != null)
				{
					continue;
				}

				// Vamos registar as contas
				string password = await CreateNewRegisteredUserAsync(singleEmail);

				// Adicionar credenciais à lista
				credentials.Add(Tuple.Create(singleEmail, password));
				
				// TODO: Verificar se isto está correto
				if (group.Members == null)
				{
					group.Members = new List<GroupMember>();
				}

				group.Members.Add(new GroupMember
				{
					Group = group,
					GroupID = group.GroupID,
					Role = "User",
					User = _context.RegisteredUsers.FirstOrDefault(m => m.Email == singleEmail),
					UserID = _context.RegisteredUsers.FirstOrDefault(m => m.Email == singleEmail).RegisteredUserID
				});
			}

			// Assegurar que guardamos a base de dados
			await _context.SaveChangesAsync();

			// Agora temos que escrever no ficheiro as credenciais de autenticação
			string destination = Path.Combine(_he.ContentRootPath, "wwwroot/Credenciais/", entity.Name, "Credenciais.txt");
			string directory = Path.GetDirectoryName(destination);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			using (FileStream fs = new FileStream(destination, FileMode.Append, FileAccess.Write))
			using (StreamWriter sw = new StreamWriter(fs))
			{
				foreach (var credential in credentials)
				{
					sw.Write("\n");
					sw.Write("Email:");
					sw.Write(credential.Item1);
					sw.Write("Password:");
					sw.Write(credential.Item2);
					sw.Write("\n");
				}
			}

			return View();
		}

		[HttpPost]
		public async Task<string> CreateNewRegisteredUserAsync(string email)
		{
			ModelState.Clear();
			if (email != string.Empty)
			{
				// Vamos Criar um novo registo para o utilizador
				var user = new IdentityUser { UserName = email, Email = email };
				string password = PasswordGenerator.GenerateRandomPassword();
				var result = await _userManager.CreateAsync(user, password);
				if (result.Succeeded)
				{
					//_logger.LogInformation("User created a new account with password.");

					await _userManager.AddToRoleAsync(user, "User");

					// Criar Utilizador
					RegisteredUser registeredUser = new RegisteredUser();
					registeredUser.Email = email;

					registeredUser.PasswordHash = user.PasswordHash;
					_context.RegisteredUsers.Add(registeredUser);
					await _context.SaveChangesAsync();

					return password;
				}
			}

			return null;
		}

		// GET: Apresenta página no qual o administrador possa alterar dados das entidades
		[Route("{id}")]
		[Authorize(Roles = "Administrator,EntityManager")]
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
		[HttpPost("{id}")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Administrator,EntityManager")]
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
		[Route("{id}")]
		[Authorize(Roles = "Administrator")]
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
		[HttpPost("{id}"), ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Administrator")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			string s = Request.QueryString.ToString();

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
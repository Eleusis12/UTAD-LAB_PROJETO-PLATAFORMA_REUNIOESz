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
using MeePoint.Interfaces;

namespace MeePoint.Controllers
{
	[Route("[controller]/[action]")]
	public class EntitiesController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IHostEnvironment _he;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly IEmailService _emailService;

		public EntitiesController(ApplicationDbContext context, IHostEnvironment host,
			UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailService emailService)
		{
			_context = context;
			_he = host;
			_userManager = userManager;
			_signInManager = signInManager;
			_emailService = emailService;
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
		public async Task<IActionResult> AddLogins()
		{
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
			string filename = Path.Combine(_he.ContentRootPath, "wwwroot/", entity.Name, "Credenciais/", "Credenciais.txt");

			if (System.IO.File.Exists(filename))
			{
				// Devolve para a view o nome do ficheiro
				ViewData["filename"] = Path.GetFileName(filename);
				ViewData["filenameLength"] = new System.IO.FileInfo(filename).Length / (double)1024;
			}
			else
			{
				// Na inexistência de ficheiro
				ViewData["filename"] = "";
				ViewData["filenameLength"] = (double)0;
			}

			return View();
		}

		[HttpPost]
		[Authorize(Roles = "EntityManager")]
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

				// Deu erro
				if (password == null)
				{
					continue;
				}

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
			string destination = Path.Combine(_he.ContentRootPath, "wwwroot/", entity.Name, "Credenciais/", "Credenciais.txt");
			string directory = Path.GetDirectoryName(destination);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			// Apagar todos os dados que estavam previamente no ficheiro
			using (FileStream fs = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write))
			using (StreamWriter sw = new StreamWriter(fs))
			{
				// Certificar que o ficheiro não tem nenhum conteúodo
				foreach (var credential in credentials)
				{
					sw.Write("Email: ");
					sw.WriteLine(credential.Item1);
					sw.Write("Password: ");
					sw.WriteLine(credential.Item2);
					sw.Write("\n");
				}
				// Proceder ao envio de emails
				foreach (var credential in credentials)
				{
					// Enviar o email com os dados da conta para o utilizador em questão
					_emailService.SendAccountCreated("oda.senger@ethereal.email", credential.Item1, entity.Name, credential.Item2);
				}
			}

			return RedirectToAction(nameof(AddLogins));
		}

		[HttpGet]
		[Authorize(Roles = "EntityManager")]
		public async Task<IActionResult> DownloadCredentialsFile()
		{
			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Obtém a entidade
			var entity = _context.Entities.Include(m => m.Groups).Include("Groups.Members").First(m => m.User.Email == user.Email);

			string filename = Path.Combine(_he.ContentRootPath, "wwwroot/", entity.Name, "Credenciais/", "Credenciais.txt");

			// Se o ficheiro existir, vamos devolver esse ficheiro, caso contrário, vamos simplesmente devolver error
			if (System.IO.File.Exists(filename))
			{
				// Lê todos os bytes do ficheiro
				byte[] fileBytes = System.IO.File.ReadAllBytes(filename);

				try
				{
					return File(fileBytes, "application/x-msdownload", filename);
				}
				finally
				{
					// Depois de retornar o ficheiro, queremos apagá-lo do nosso servidor
					// Para assegurar a integridade das contas criadas
					System.IO.File.Delete(filename);
				}
			}
			else
			{
				return NotFound();
			}
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

			// Obter a Entidade
			var entity = _context.Entities.Include(m => m.User)
				.Include(m => m.Groups)
				.ThenInclude(m => m.Members)
				.ThenInclude(m => m.User)
				.FirstOrDefault(m => m.EntityID == id);

			// Guardar temporariamente o valor do username para que possamos apagar os respetivos dados tb nas outras tableas não apenas na tabela entidade
			string email = entity.User.Email;
			IdentityUser user = await _userManager.FindByEmailAsync(email);

			if (user != null)
			{
				// Primeiro Vamos ter que eliminar todas as contas que façam parte desta entidade
				var registeredUsers = entity.Groups.FirstOrDefault(m => m.Name.ToLower() == "main").Members.Select(m => m.User);
				var emails = registeredUsers.Select(m => m.Email);

				// Apagar todas as contas nas tabelas asp net Users
				foreach (var registeredUserEmail in emails)
				{
					IdentityUser aspNetUser = await _userManager.FindByEmailAsync(registeredUserEmail);

					if (aspNetUser == null)
					{
						continue;
					}

					// Apaga as contas
					_context.UserLogins.RemoveRange(_context.UserLogins.Where(ul => ul.UserId == aspNetUser.Id));

					_context.UserRoles.RemoveRange(_context.UserRoles.Where(ur => ur.UserId == aspNetUser.Id));

					_context.Users.Remove(_context.Users.Where(usr => usr.Id == aspNetUser.Id).Single());
				}

				// Apagar todas as contas no Registered USer, o admin já está nesta lista
				_context.RegisteredUsers.RemoveRange(registeredUsers);

				//// Em Seguida Vamos apagar a conta do administrador associado à entidade
				//_context.RegisteredUsers.Remove(entity.User);

				// Apagar Entidade implica apagar todas as informações relacionadas com esta
				// Com o Cascade on Delete ativado, esta única operação deve apagar todos dados relacionados
				_context.Entities.Remove(entity);

				_context.SaveChanges();
			}

			return RedirectToAction(nameof(Index));
		}

		private bool EntityExists(int id)
		{
			return _context.Entities.Any(e => e.EntityID == id);
		}
	}
}
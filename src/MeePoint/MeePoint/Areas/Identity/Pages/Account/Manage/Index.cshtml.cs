using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MeePoint.Data;
using MeePoint.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace MeePoint.Areas.Identity.Pages.Account.Manage
{
	public partial class IndexModel : PageModel
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly IHostEnvironment _he;
		private readonly SignInManager<IdentityUser> _signInManager;

		public IndexModel(
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager,
			ApplicationDbContext context, IHostEnvironment host)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
			_he = host;
		}

		public string Username { get; set; }

		[TempData]
		public string StatusMessage { get; set; }

		public RegisteredUser registeredUser { get; set; }

		[BindProperty]
		public InputRegisteredUserModel InputRegisteredUser { get; set; }

		[BindProperty]
		public InputPaswordModel InputPassword { get; set; }

		public class InputRegisteredUserModel : RegisteredUser
		{
			[Phone]
			[Display(Name = "Phone number")]
			public string PhoneNumber { get; set; }

			[Display(Name = "Imagem de Perfil")]
			public IFormFile ProfilePic { get; set; }
		}

		public class InputPaswordModel
		{
			[Required]
			[DataType(DataType.Password)]
			[Display(Name = "Current password")]
			public string OldPassword { get; set; }

			[Required]
			[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
			[DataType(DataType.Password)]
			[Display(Name = "New password")]
			public string NewPassword { get; set; }

			[DataType(DataType.Password)]
			[Display(Name = "Confirm new password")]
			[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
			public string ConfirmPassword { get; set; }
		}

		private async Task LoadAsync(IdentityUser user)
		{
			var userName = await _userManager.GetUserNameAsync(user);
			var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

			// De relemebrar que o username por defeito é igual ao email na framework Identity
			Username = userName;

			registeredUser = await _context.RegisteredUsers.Include(m => m.Groups).Include("Groups.Group").Include("Groups.Group.Entity").FirstOrDefaultAsync(x => x.Email == userName);

			InputRegisteredUser = new InputRegisteredUserModel
			{
				PhoneNumber = phoneNumber,
				Email = registeredUser.Email,
				Name = registeredUser.Name,
				Photo = registeredUser.Photo,
				RegisteredUserID = registeredUser.RegisteredUserID,
				Username = registeredUser.Username,
			};

			try
			{
				ViewData["entityName"] = registeredUser.Groups.FirstOrDefault(x => x.Group.Name.ToLower() == "main".ToLower()).Group?.Entity?.Name;
			}
			catch (NullReferenceException)
			{
				ViewData["entityName"] = "Não tem entidade associada";
			}
		}

		public async Task<IActionResult> OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			await LoadAsync(user);
			return Page();
		}

		public async Task<IActionResult> OnPostRegistedUserAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}
			// Obter o utilizador
			registeredUser = await _context.RegisteredUsers.Include(m => m.Groups).Include("Groups.Group").Include("Groups.Group.Entity").FirstOrDefaultAsync(x => x.Email == user.UserName);

			// Vamos obter o nome da entidade a que esta conta está associado
			var entity = registeredUser.Groups.FirstOrDefault(x => x.Group.Name.ToLower() == "main".ToLower()).Group.Entity;

			// Se o utilizador inseriu uma foto
			if (InputRegisteredUser.ProfilePic != null)
			{
				// Agora temos que escrever no ficheiro as credenciais de autenticação
				string destination = Path.Combine(_he.ContentRootPath, "wwwroot/", entity.Name, "FotosDePerfil", Convert.ToString(Guid.NewGuid()) + Path.GetExtension(InputRegisteredUser.ProfilePic.FileName));
				string directory = Path.GetDirectoryName(destination);
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				// Creates a filestream to store the file listing
				FileStream fs = new FileStream(destination, FileMode.Create);

				try
				{
					InputRegisteredUser.ProfilePic.CopyTo(fs);
					fs.Close();
				}
				catch (Exception ex)
				{
					throw ex;
				}

				// path para depois guardar na base de dados
				registeredUser.Photo = destination.Substring(destination.IndexOf(entity.Name) - 1);
			}

			// Update values
			// De momento não vamos permitir que o utilizador altere o seu email, isso só complicaria
			//registeredUser.Email = Input.Email;
			//registeredUser.Username = Input.Username;
			registeredUser.Name = InputRegisteredUser.Name;

			ModelState.Clear();

			if (TryValidateModel(registeredUser))
			{
				_context.RegisteredUsers.Update(registeredUser);
				await _context.SaveChangesAsync();
			}
			else
			{
				await LoadAsync(user);
				return Page();
			}

			await _signInManager.RefreshSignInAsync(user);
			StatusMessage = "Your profile has been updated";
			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostPasswordAsync()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var changePasswordResult = await _userManager.ChangePasswordAsync(user, InputPassword.OldPassword, InputPassword.NewPassword);

			if (!changePasswordResult.Succeeded)
			{
				foreach (var error in changePasswordResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return Page();
			}

			await _signInManager.RefreshSignInAsync(user);

			registeredUser = await _context.RegisteredUsers.FirstOrDefaultAsync(x => x.Email == user.UserName);
			registeredUser.PasswordHash = user.PasswordHash;

			StatusMessage = "Your password has been changed.";

			return RedirectToPage();
		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MeePoint.Data;
using MeePoint.Filters;
using MeePoint.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeePoint.Areas.Identity.Pages.Account
{
	[AllowAnonymous]
	public class RegisterModel : PageModel
	{
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ILogger<RegisterModel> _logger;

		//private readonly IEmailSender _emailSender;
		private readonly ApplicationDbContext _context;

		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IHostEnvironment _he;

		public RegisterModel(
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager,
			ILogger<RegisterModel> logger,
			ApplicationDbContext dbContext,
			RoleManager<IdentityRole> roleManager, IHostEnvironment host)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;
			//_emailSender = emailSender;
			_context = dbContext;
			_roleManager = roleManager;
			_he = host;
		}

		[BindProperty]
		public InputModel Input { get; set; }

		public string ReturnUrl { get; set; }

		public IList<AuthenticationScheme> ExternalLogins { get; set; }

		public class InputModel
		{
			public InputModel()
			{
				Entity = new Entity();
			}

			[Required]
			[EmailAddress]
			[Display(Name = "Email")]
			public string Email { get; set; }

			[Required]
			[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
			[DataType(DataType.Password)]
			[Display(Name = "Password")]
			public string Password { get; set; }

			[DataType(DataType.Password)]
			[Display(Name = "Confirm password")]
			[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
			public string ConfirmPassword { get; set; }

			// Entity object in which we will store information about the entity information the applicant is about to submit
			public Entity Entity
			{
				get; set;
			}
		}

		public async Task OnGetAsync(string returnUrl = null)
		{
			List<SelectListItem> PlansList = new List<SelectListItem>
	{
		new SelectListItem() { Text = "Plano Gratuito", Selected = true, Value = "200"},
		new SelectListItem() { Text = "Plano Premium", Selected = false, Value = "2000"},
		new SelectListItem() { Text = "Plano Professional", Selected = false, Value = "20000"}
	};

			ViewData["Plans"] = PlansList;
			ReturnUrl = returnUrl;
			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
		}

		public async Task<IActionResult> OnPostAsync(string returnUrl = null)
		{
			Input.Entity.SubscriptionDateStart = DateTime.Now;
			Input.Entity.SubscriptionDateEnd = Input.Entity.SubscriptionDateStart.AddDays(Input.Entity.SubscriptionDays);

			// Make sure the status is not set to true
			Input.Entity.StatusEntity = false;

			// Clear Model Erros
			ModelState.Clear();

			returnUrl = returnUrl ?? Url.Content("~/");
			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
			// Check for errors
			if (TryValidateModel(Input))
			{
				var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
				var result = await _userManager.CreateAsync(user, Input.Password);
				if (result.Succeeded)
				{
					_logger.LogInformation("User created a new account with password.");

					// The user we added must have entityManager permissions
					await _userManager.AddToRoleAsync(user, "EntityManager");

					// Create Registered User in our database
					RegisteredUser newUser = new RegisteredUser();
					newUser.Name = Input.Entity.ManagerName;
					newUser.Email = Input.Email;
					newUser.PasswordHash = user.PasswordHash;

					// Registered User and Asp Net core Users are linked by their emails

					// Clean Models Errors- At this point there should be no model erros, but just to make sure let's clean it
					ModelState.Clear();

					// Link the newly user created to entity
					Input.Entity.User = newUser;

					// Validate entity model, to check if it respects all requirementes
					if (TryValidateModel(Input.Entity))
					{
						// Add Entity to database
						_context.Add(Input.Entity);
						await _context.SaveChangesAsync();
					}

					var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
					var callbackUrl = Url.Page(
						"/Account/ConfirmEmail",
						pageHandler: null,
						values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
						protocol: Request.Scheme);

					//await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
					//	$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

					if (_userManager.Options.SignIn.RequireConfirmedAccount)
					{
						return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
					}
					else
					{
						await _signInManager.SignInAsync(user, isPersistent: false);
						return LocalRedirect(returnUrl);
					}
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			// If we got this far, something failed, redisplay form
			return Page();
		}
	}
}
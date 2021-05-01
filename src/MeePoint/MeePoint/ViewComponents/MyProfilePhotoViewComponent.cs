using MeePoint.Data;
using MeePoint.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeePoint.ViewComponents
{
	[ViewComponent(Name = "MyProfilePhoto")]
	public class MyProfilePhotoViewComponent : ViewComponent
	{
		private readonly ApplicationDbContext _context;

		public MyProfilePhotoViewComponent(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync(string email)
		{
			// Enviar o user para a view
			var registeredUser = await _context.RegisteredUsers.FirstOrDefaultAsync(m => m.Email == email);
			var profilePhoto = registeredUser.Photo?.ToLower() == "Foto".ToLower() || registeredUser.Photo == null ? "/assets/images/profile.png" : registeredUser.Photo;
			return View(model: profilePhoto);
		}
	}
}
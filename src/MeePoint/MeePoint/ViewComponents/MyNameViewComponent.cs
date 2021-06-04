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
	[ViewComponent(Name = "MyName")]
	public class MyNameViewComponent : ViewComponent
	{
		private readonly ApplicationDbContext _context;

		public MyNameViewComponent(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync(string email)
		{
			// Enviar o user para a view
			var registeredUser = await _context.RegisteredUsers.FirstOrDefaultAsync(m => m.Email == email);
			string userName = registeredUser.Name;

			// Temos que perguntar à base de dados se o utilizador autenticado é um administrador
			var entity = _context.Entities.Include(m => m.User)
				.FirstOrDefault(m => m.User.RegisteredUserID == registeredUser.RegisteredUserID);

			// Se o entity resultante não é null, é porque o utilizador em causa é de facto administrador da entidade
			string roleName = entity == null ? "" : "Administrador";
			Tuple<string, string> model = new Tuple<string, string>(userName, roleName);

			return View(model: model);
		}
	}
}
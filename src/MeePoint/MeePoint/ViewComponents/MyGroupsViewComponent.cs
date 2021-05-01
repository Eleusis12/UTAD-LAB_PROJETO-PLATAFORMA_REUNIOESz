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
	[ViewComponent(Name = "MyGroups")]
	public class MyGroupsViewComponent : ViewComponent
	{
		private readonly ApplicationDbContext _context;

		public MyGroupsViewComponent(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync(string email)
		{
			// Enviar o user para a view
			IEnumerable<GroupMember> groups = _context.RegisteredUsers.Include(m => m.Groups).Include("Groups.Group").FirstOrDefault(m => m.Email == email).Groups.Where(m => m.Group.Name.ToLower() != "main".ToLower());

			return View(groups);
		}
	}
}
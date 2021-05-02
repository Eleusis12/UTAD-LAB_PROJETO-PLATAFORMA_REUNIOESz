using MeePoint.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeePoint.Data
{
	public class DbInitializer
	{
		public async static Task Initialize(ApplicationDbContext _context, UserManager<IdentityUser> _userManager)
		{
			string email = "admin@hotmail.com";
			string password = "teste123";
			string adminName = "Nelson Matos";

			_context.Database.EnsureCreated();

			var admins = await _userManager.GetUsersInRoleAsync("ADMINISTRATOR");

			if (admins.Count() > 0)
				return;

			// Não existem admins na base de dados, então vamos adicionar
			// Vamos criar a conta nas tabelas relacionadas com as tabelas Identity
			var user = new IdentityUser { UserName = email, Email = email };
			var result = await _userManager.CreateAsync(user, password);
			if (result.Succeeded)
			{
				// Adicionar utilizador à lista dos users que são administradores

				// Já adicionámos na base de dados nas tabelas Identity
				// Agora vamos adicionar este utilizador Admin, na tabela registeredUser

				RegisteredUser admin = new RegisteredUser()
				{
					Email = user.Email,
					Name = adminName,
					PasswordHash = user.PasswordHash
				};

				// Guardar na base de dados
				_context.RegisteredUsers.Add(admin);
				await _userManager.AddToRoleAsync(user, "Administrator");

				await _context.SaveChangesAsync();
			}
		}
	}
}
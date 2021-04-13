using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MeePoint.Data
{
    public class SeedRoles
    {
        public static async Task Seed(IServiceProvider svcProvider)
        {

            var roleManager = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Administrator", "EntityManager", "GroupManager", "User" };

            IdentityResult result;

            foreach (var role in roleNames)
            {

                var roleExists = await roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {

                    result = await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace BobDeathmic.Models
{
    public static class SeedData
    {
        public async static Task Initialize(IServiceProvider serviceProvider)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            using (var context = new Data.ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<Data.ApplicationDbContext>>()))
            {
                if (context.ChatUserModels.Any())
                {
                    return;
                }
                var usermanager = serviceProvider.GetRequiredService <UserManager<ChatUserModel>>();

                var user = new ChatUserModel { UserName = "Admin" };
                var dev = new ChatUserModel { UserName = "Dev" };
                var result = await usermanager.CreateAsync(user, "kermit22");
                result = await usermanager.CreateAsync(dev, "kermit22");
                await Models.SeedData.CreateOrAddUserRoles("Admin", "Admin", serviceProvider);
                await Models.SeedData.CreateOrAddUserRoles("Dev", "Dev", serviceProvider);
            }
            return;

        }
        private static async Task CreateOrAddUserRoles(string role, string name, IServiceProvider serviceProvider)
        {
            try
            {
                var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var UserManager = serviceProvider.GetRequiredService<UserManager<Models.ChatUserModel>>();

                IdentityResult roleResult;
                //Adding Admin Role
                var roleCheck = await RoleManager.RoleExistsAsync(role);
                if (!roleCheck)
                {
                    //create the roles and seed them to the database
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(role));
                }
                //Assign Admin role to the main User here we have given our newly registered 
                //login id for Admin management
                Models.ChatUserModel user = await UserManager.FindByNameAsync(name);
                await UserManager.AddToRoleAsync(user, role);
            }
            catch (Exception)
            {
                Console.WriteLine(name);
                Console.WriteLine(role);
            }

        }
    }
}

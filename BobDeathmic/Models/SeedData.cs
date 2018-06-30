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
                var user = new ChatUserModel { UserName = "chromos33" };
                var result = await usermanager.CreateAsync(user, "kermit22");
                Console.WriteLine("test");
            }
            return;

        }
    }
}

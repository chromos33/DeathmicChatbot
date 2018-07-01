using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BobDeathmic.Models;

namespace BobDeathmic.Data
{
    public class ApplicationDbContext : IdentityDbContext<ChatUserModel>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Stream>()
                .HasMany(p => p.StreamProvider)
                .WithOne(s => s.Stream);
        }
        public DbSet<Models.ChatUserModel> ChatUserModels { get; set; }
        public DbSet<Models.Stream> StreamModels { get; set; }
        public DbSet<StreamProvider> StreamProviders { get; set; }
    }
}

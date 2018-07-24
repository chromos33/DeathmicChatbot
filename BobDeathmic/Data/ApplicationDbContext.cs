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
            builder.Entity<ChatUserModel>()
                .HasMany(u => u.StreamSubscriptions)
                .WithOne(ss => ss.User)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ChatUserModel>()
                .HasMany(u => u.OwnedStreams)
                .WithOne(s => s.Owner)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Stream>()
                .HasMany(s => s.StreamSubscriptions)
                .WithOne(ss => ss.Stream)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Stream>().HasOne(s => s.Owner).WithMany(u => u.OwnedStreams);
        }
        public DbSet<Models.ChatUserModel> ChatUserModels { get; set; }
        public DbSet<Models.Stream> StreamModels { get; set; }
        public DbSet<StreamSubscription> StreamSubscriptions { get; set; }
        public DbSet<SecurityToken> SecurityTokens { get; set; }
        public DbSet<Models.Discord.RelayChannels> RelayChannels { get; set; }
    }
}

using BobDeathmic.Data.DBModels.Commands;
using BobDeathmic.Data.DBModels.EventCalendar;
using BobDeathmic.Data.DBModels.EventCalendar.manymany;
using BobDeathmic.Data.DBModels.GiveAway;
using BobDeathmic.Data.DBModels.GiveAway.manymany;
using BobDeathmic.Data.DBModels.Quote;
using BobDeathmic.Data.DBModels.Relay;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Models;
using BobDeathmic.Models.Events;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            builder.Entity<Stream>()
                .HasMany(s => s.Commands)
                .WithOne(sc => sc.stream)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Stream>().HasOne(s => s.Owner).WithMany(u => u.OwnedStreams);

            builder.Entity<GiveAwayItem>()
                .HasOne(gai => gai.Owner)
                .WithMany(u => u.OwnedItems)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.Entity<GiveAwayItem>()
                .HasOne(gai => gai.Receiver)
                .WithMany(u => u.ReceivedItems)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<User_GiveAwayItem>()
                .HasKey(t => new { t.UserID, t.GiveAwayItemID });

            builder.Entity<User_GiveAwayItem>()
                .HasOne(pt => pt.User)
                .WithMany(p => p.AppliedTo)
                .HasForeignKey(pt => pt.UserID);
            builder.Entity<User_GiveAwayItem>()
                .HasOne(pt => pt.GiveAwayItem)
                .WithMany(t => t.Applicants)
                .HasForeignKey(pt => pt.GiveAwayItemID);

            builder.Entity<ChatUserModel>()
                .HasMany(x => x.Calendars)
                .WithOne(x => x.ChatUserModel)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Event>()
                .HasOne(c => c.Admin)
                .WithMany(u => u.AdministratedCalendars)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Event>()
                .HasMany(x => x.EventDates)
                .WithOne(y => y.Event)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<EventDate>()
                .HasOne(x => x.Event)
                .WithMany(y => y.EventDates)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<EventDate>()
                .HasMany(x => x.Teilnahmen)
                .WithOne(x => x.EventDate)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Event>()
                .HasMany(x => x.EventDateTemplates)
                .WithOne(y => y.Event)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<AppointmentRequest>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.AppointmentRequests)
                .OnDelete(DeleteBehavior.ClientSetNull);

        }
        //a foreign key constraint fails (`bobcoreef`.`appointmentrequests`, CONSTRAINT `FK_AppointmentRequests_EventDates_EventDateID` FOREIGN KEY (`EventDateID`) REFERENCES `eventdates` (`ID`)) ---
        public DbSet<ChatUserModel> ChatUserModels { get; set; }
        public DbSet<Stream> StreamModels { get; set; }
        public DbSet<StreamSubscription> StreamSubscriptions { get; set; }
        public DbSet<SecurityToken> SecurityTokens { get; set; }
        public DbSet<RelayChannels> RelayChannels { get; set; }
        public DbSet<DiscordBan> DiscordBans { get; set; }
        public DbSet<StreamCommand> StreamCommand { get; set; }
        public DbSet<GiveAwayItem> GiveAwayItems { get; set; }
        public DbSet<User_GiveAwayItem> User_GiveAway { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<AppointmentRequest> AppointmentRequests { get; set; }
        public DbSet<EventDateTemplate> EventDateTemplates { get; set; }
        public DbSet<EventDate> EventDates { get; set; }
        public DbSet<ChatUserModel_Event> ChatUserModel_Event { get; set; }

        public DbSet<RandomChatUser> RandomChatUser { get; set; }
        public DbSet<Quote> Quotes { get; set; }
    }
}

using BobDeathmic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using BobDeathmic.Data.DBModels.User;
using System.Threading.Tasks;
using NSubstitute;
using BobDeathmic.Eventbus;
using BobDeathmic.Services.Discords;
using Microsoft.Extensions.DependencyInjection;

namespace BobDeathmic.Tests.Services.Discord
{
    [TestFixture]
    public class DiscordServiceTests
    {
        private DbContextOptions<ApplicationDbContext> options;
        private DiscordService Dservice;

        [SetUp]
        public void Init()
        {
            options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test")
                .Options;
            using (var context = new ApplicationDbContext(options))
            {
                context.ChatUserModels.Add(new ChatUserModel());

                context.SaveChanges();
            }
            IEventBus eventbus = Substitute.For<IEventBus>();
            
            IServiceScope scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.GetService(typeof(ApplicationDbContext)).ReturnsForAnyArgs(new ApplicationDbContext(options));

            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            scopefactory.CreateScope().ReturnsForAnyArgs(scope);
            Dservice = new DiscordService(scopefactory, eventbus);

        }

        /* for Setup Testing
        [Test]
        public async Task TestDB()
        {
            Assert.That(() => Dservice.temptest(),Is.EqualTo(true));
        }
        */
    }
}

using BobDeathmic.Args;
using BobDeathmic.ChatCommands.Args;
using BobDeathmic.Data;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.Eventbus;
using BobDeathmic.Services.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BobDeathmic.Tests.Services.Commands
{
    [TestFixture]
    public class CommandServiceTests
    {
        [Test]
        public async Task handleCommand_GameCommand_StreamTitleChangeEventTriggered()
        {
            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            IEventBus eventbus = new EventBusLocal();
            StreamTitleChangeArgs receivedEventsArg = null;
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.StreamTitleChangeRequested += delegate (object sender, StreamTitleChangeArgs e)
            {
                receivedEventsArg = e;
            };
            ChatCommandInputArgs args = new ChatCommandInputArgs()
            {
                Message = "!game neuer spiele titel",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch,"deathmic");

            Assert.AreNotEqual(null, receivedEventsArg);
            Assert.AreEqual("neuer spiele titel", receivedEventsArg.Game);
            Assert.AreEqual("deathmic", receivedEventsArg.StreamName);
            Assert.AreEqual(StreamProviderTypes.Twitch, receivedEventsArg.Type);
        }
        [Test]
        public async Task handleCommand_TitleCommand_StreamTitleChangeEventTriggered()
        {
            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            IEventBus eventbus = new EventBusLocal();
            StreamTitleChangeArgs receivedEventsArgs = null;
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.StreamTitleChangeRequested += delegate (object sender, StreamTitleChangeArgs e)
            {
                receivedEventsArgs=e;
            };
            ChatCommandInputArgs args = new ChatCommandInputArgs()
            {
                Message = "!title neuer spiele titel",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch, "deathmic");

            Assert.AreNotEqual(null, receivedEventsArgs);
            Assert.AreEqual("neuer spiele titel", receivedEventsArgs.Title);
            Assert.AreEqual("deathmic", receivedEventsArgs.StreamName);
            Assert.AreEqual(StreamProviderTypes.Twitch, receivedEventsArgs.Type);
        }
        [Test]
        public async Task handleCommand_Strawpoll_StrawPollRequestedEventTriggered()
        {
            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            IEventBus eventbus = new EventBusLocal();
            StrawPollRequestEventArgs receivedEventsArgs = null;
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.StrawPollRequested += delegate (object sender, StrawPollRequestEventArgs e)
            {
                receivedEventsArgs = e;
            };
            ChatCommandInputArgs args = new ChatCommandInputArgs()
            {
                Message = "!strawpoll Tolle Frage hier? | antwort 1 | antwort2",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch, "deathmic");

            Assert.AreNotEqual(null, receivedEventsArgs);
            Assert.AreEqual("Tolle Frage hier?", receivedEventsArgs.Question);
            Assert.AreEqual(2, receivedEventsArgs.Answers.Length);
            Assert.AreEqual("deathmic", receivedEventsArgs.StreamName);
            Assert.AreEqual(StreamProviderTypes.Twitch, receivedEventsArgs.Type);
        }
        [Test]
        public async Task handleCommand_RegisterRaffle_CommandResponseReceivedEventTriggered()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test")
                .Options;
            IServiceScope scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.GetService(typeof(ApplicationDbContext)).ReturnsForAnyArgs(new ApplicationDbContext(options));

            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            scopefactory.CreateScope().ReturnsForAnyArgs(scope);

            IEventBus eventbus = new EventBusLocal();
            CommandResponseArgs receivedEventsArgs = null;
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.CommandOutputReceived += delegate (object sender, CommandResponseArgs e)
            {
                receivedEventsArgs = e;
            };
            ChatCommandInputArgs args = new ChatCommandInputArgs()
            {
                Message = "!registerraffle",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch, "deathmic");

            Assert.AreNotEqual(null, receivedEventsArgs);
            Assert.AreEqual("chromos33", receivedEventsArgs.Sender);
            Assert.AreEqual("You were added.", receivedEventsArgs.Message);
        }
        [Test]
        public async Task handleCommand_PickNext_CommandResponseReceivedEventTriggered()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test")
                .Options;
            IServiceScope scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.GetService(typeof(ApplicationDbContext)).ReturnsForAnyArgs(new ApplicationDbContext(options));

            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            scopefactory.CreateScope().ReturnsForAnyArgs(scope);

            IEventBus eventbus = new EventBusLocal();
            CommandResponseArgs receivedEventsArgs = null;
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.CommandOutputReceived += delegate (object sender, CommandResponseArgs e)
            {
                receivedEventsArgs = e;
            };
            ChatCommandInputArgs args = new ChatCommandInputArgs()
            {
                Message = "!next",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch, "deathmic");

            Assert.AreNotEqual(null, receivedEventsArgs);
            Assert.AreEqual("chromos33", receivedEventsArgs.Sender);
        }
        [Test]
        public async Task handleCommand_PickNextRand_CommandResponseReceivedEventTriggered()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test")
                .Options;
            IServiceScope scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.GetService(typeof(ApplicationDbContext)).ReturnsForAnyArgs(new ApplicationDbContext(options));

            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            scopefactory.CreateScope().ReturnsForAnyArgs(scope);

            IEventBus eventbus = new EventBusLocal();
            CommandResponseArgs receivedEventsArgs = null;
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.CommandOutputReceived += delegate (object sender, CommandResponseArgs e)
            {
                receivedEventsArgs = e;
            };
            ChatCommandInputArgs args = new ChatCommandInputArgs()
            {
                Message = "!randnext",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch, "deathmic");

            Assert.AreNotEqual(null, receivedEventsArgs);
            Assert.AreEqual("chromos33", receivedEventsArgs.Sender);
        }
        [Test]
        public async Task handleCommand_SkipLast_CommandResponseReceivedEventTriggered()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test")
                .Options;
            IServiceScope scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.GetService(typeof(ApplicationDbContext)).ReturnsForAnyArgs(new ApplicationDbContext(options));

            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            scopefactory.CreateScope().ReturnsForAnyArgs(scope);

            IEventBus eventbus = new EventBusLocal();
            CommandResponseArgs receivedEventsArgs = null;
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.CommandOutputReceived += delegate (object sender, CommandResponseArgs e)
            {
                receivedEventsArgs = e;
            };
            ChatCommandInputArgs args = new ChatCommandInputArgs()
            {
                Message = "!skip",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch, "deathmic");

            Assert.AreNotEqual(null, receivedEventsArgs);
            Assert.AreEqual("chromos33", receivedEventsArgs.Sender);
            Assert.AreEqual("No skippable user found", receivedEventsArgs.Message);
        }
        [Test]
        public async Task handleCommand_ListRandUsers_CommandResponseReceivedEventTriggered()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test")
                .Options;
            IServiceScope scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.GetService(typeof(ApplicationDbContext)).ReturnsForAnyArgs(new ApplicationDbContext(options));

            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            scopefactory.CreateScope().ReturnsForAnyArgs(scope);

            IEventBus eventbus = new EventBusLocal();
            CommandResponseArgs receivedEventsArgs = null;
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.CommandOutputReceived += delegate (object sender, CommandResponseArgs e)
            {
                receivedEventsArgs = e;
            };
            ChatCommandInputArgs args = new ChatCommandInputArgs()
            {
                Message = "!list",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch, "deathmic");

            Assert.AreNotEqual(null, receivedEventsArgs);
            Assert.AreEqual("chromos33", receivedEventsArgs.Sender);
            Assert.AreEqual("No User found/in List", receivedEventsArgs.Message);
        }
    }
}

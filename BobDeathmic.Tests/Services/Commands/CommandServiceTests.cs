using BobDeathmic.Args;
using BobDeathmic.ChatCommands.Args;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.Eventbus;
using BobDeathmic.Services.Commands;
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
            List<StreamTitleChangeArgs> receivedEventsArgs = new List<StreamTitleChangeArgs>();
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.StreamTitleChangeRequested += delegate (object sender, StreamTitleChangeArgs e)
            {
                receivedEventsArgs.Add(e);
            };
            ChatCommandArguments args = new ChatCommandArguments()
            {
                Message = "!game neuer spiele titel",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch,"deathmic");

            Assert.AreEqual(1, receivedEventsArgs.Count);
            Assert.AreEqual("neuer spiele titel", receivedEventsArgs[0].Game);
            Assert.AreEqual("deathmic", receivedEventsArgs[0].StreamName);
            Assert.AreEqual(StreamProviderTypes.Twitch, receivedEventsArgs[0].Type);
        }
        [Test]
        public async Task handleCommand_TitleCommand_StreamTitleChangeEventTriggered()
        {
            IServiceScopeFactory scopefactory = Substitute.For<IServiceScopeFactory>();
            IEventBus eventbus = new EventBusLocal();
            List<StreamTitleChangeArgs> receivedEventsArgs = new List<StreamTitleChangeArgs>();
            //eventbus.TriggerEvent(EventType.).For
            CommandService service = new CommandService(scopefactory, eventbus);
            eventbus.StreamTitleChangeRequested += delegate (object sender, StreamTitleChangeArgs e)
            {
                receivedEventsArgs.Add(e);
            };
            ChatCommandArguments args = new ChatCommandArguments()
            {
                Message = "!title neuer spiele titel",
                Sender = "chromos33",
                ChannelName = "deathmic",
                Type = BobDeathmic.Data.Enums.ChatType.Twitch,
                elevatedPermissions = true
            };
            await service.handleCommand(args, BobDeathmic.Data.Enums.ChatType.Twitch, "deathmic");

            Assert.AreEqual(1, receivedEventsArgs.Count);
            Assert.AreEqual("neuer spiele titel", receivedEventsArgs[0].Title);
            Assert.AreEqual("deathmic", receivedEventsArgs[0].StreamName);
            Assert.AreEqual(StreamProviderTypes.Twitch, receivedEventsArgs[0].Type);
        }

    }
}

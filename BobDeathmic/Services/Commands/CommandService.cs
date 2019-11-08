using BobDeathmic.ChatCommands;
using BobDeathmic.ChatCommands.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data.Enums;
using BobDeathmic.Eventbus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Commands
{
    public class CommandService : BackgroundService,ICommandService
    {
        private List<IfCommand> Commands;
        private IServiceScopeFactory scopefactory;
        private IEventBus eventBus;
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }

        public CommandService(IServiceScopeFactory scopefactory,IEventBus evenbus)
        {
            initCommands();
            this.scopefactory = scopefactory;
            this.eventBus = evenbus;
        }

        private void initCommands()
        {
            Commands = new List<IfCommand>();
            //Commands.Add(new TwitchStreamTitle());
            Commands.Add(new Game());
            Commands.Add(new Title());
            Commands.Add(new Strawpoll());
            //Commands.Add(new RandomChatUserRegisterCommand());
            //Commands.Add(new PickNextRandChatUserForNameCommand());
            //Commands.Add(new PickNextChatUserForNameCommand());
            //Commands.Add(new ListRandUsersInListCommand());
            //Commands.Add(new SkipLastRandUserCommand());
            //Commands.Add(new QuoteCommand());
            //Commands.Add(new Help(Commands));
            
        }
        public List<IfCommand> GetCommands(ChatType type)
        {
            return Commands.Where(x => x.ChatSupported(type) && x.GetType() != typeof(Help)).ToList();
        }
        public async Task handleCommand(ChatCommandArguments args, ChatType chatType, string sender)
        {
            foreach(IfCommand command in Commands.Where(x => x.ChatSupported(chatType) && x.isCommand(args.Message)))
            {
                ChatCommandOutput output = command.execute(args, scopefactory);
                if(output.ExecuteEvent)
                {
                    eventBus.TriggerEvent(output.Type, output.EventData);
                }
            }
        }
    }
}

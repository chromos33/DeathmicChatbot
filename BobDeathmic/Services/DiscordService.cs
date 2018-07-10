using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using BobDeathmic.Args;

namespace BobDeathmic.Services
{
    public interface IDiscordService
    {

    }
    public class DiscordService : BackgroundService, IDiscordService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IEventBus _eventBus;
        DiscordSocketClient client;


        public DiscordService(IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            _eventBus.TwitchMessageReceived += TwitchMessageReceived;
        }

        private void TwitchMessageReceived(object sender, TwitchMessageArgs e)
        {
            Console.WriteLine("Message Received");
            //throw new NotImplementedException();
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConnectToDiscord();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
            return;
        }
        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await client.StopAsync();
            Dispose();
        }
        protected async Task ConnectToDiscord()
        {
            var discordConfig = new DiscordSocketConfig { MessageCacheSize = 100 };
            discordConfig.AlwaysDownloadUsers = true;
            discordConfig.LargeThreshold = 250;
            client = new DiscordSocketClient(discordConfig);
            string token = "";
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var tokens = _context.SecurityTokens.Where(st => st.service == Models.Enum.TokenType.Discord).FirstOrDefault();
                if(tokens != null)
                {
                    if(tokens.token == null)
                    {
                        token = tokens.ClientID;
                    }
                    else
                    {
                        token = tokens.token;
                    }
                    
                }
            }
            if(token != "")
            {
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();
                client.MessageReceived += MessageReceived;
                client.Ready += ClientConnected;
                client.Disconnected += ClientDisconnected;
                client.UserJoined += ClientJoined;
            }
        }

        private async Task ClientJoined(SocketGuildUser arg)
        {

        }

        private async Task ClientDisconnected(Exception arg)
        {
            Console.WriteLine("test");
        }

        private async Task ClientConnected()
        {
            Console.WriteLine("test");
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            Console.WriteLine(arg.Content);
            if(arg.Author.Username != "BobDeathmic")
            {
                if (false)//Commands later on
                {

                }
                else
                {
                    if(arg.Channel.Name.StartsWith("Stream_"))
                    {
                        Args.DiscordMessageArgs args = new Args.DiscordMessageArgs();
                        _eventBus.TriggerEvent(EventType.DiscordMessageReceived, args);
                    }
                }
            }
        }
    }
}

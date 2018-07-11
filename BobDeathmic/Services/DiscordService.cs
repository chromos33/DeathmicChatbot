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
using System.Text.RegularExpressions;

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
            client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault()?.TextChannels.Where(c => c.Name.ToLower() == e.Target.ToLower()).FirstOrDefault()?.SendMessageAsync(e.Message);
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
                client.ChannelCreated += ChannelChanged;
                client.ChannelDestroyed += ChannelChanged;
                
            }
        }

        private async Task ChannelChanged(SocketChannel arg)
        {
            UpdateRelayChannels();
        }

        private async Task ClientJoined(SocketGuildUser arg)
        {
            
        }
        private async Task UpdateRelayChannels()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                List<string> discordChannels = new List<string>();
                foreach(var channel in client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault().Channels)
                {
                    if(Regex.Match(channel.Name.ToLower(), @"stream_").Success)
                    {
                        discordChannels.Add(channel.Name);
                    }
                }
                List<string> savedChannels = new List<string>();
                foreach (var channel in _context.RelayChannels)
                {
                    savedChannels.Add(channel.Name);
                }
                List<string> channelsToBeAdded = discordChannels.Except(savedChannels).ToList();
                List<string> channelsToBeRemoved = savedChannels.Except(discordChannels).ToList();
                if(channelsToBeAdded.Count() > 0)
                {
                    foreach(var channel in channelsToBeAdded)
                    {
                        _context.RelayChannels.Add(new Models.Discord.RelayChannels { Name = channel });
                    }
                }
                if(channelsToBeRemoved.Count() > 0)
                {
                    foreach (var channel in channelsToBeRemoved)
                    {
                        _context.RelayChannels.Remove(_context.RelayChannels.Where(rc => rc.Name == channel).FirstOrDefault());
                    }
                }
                if(channelsToBeAdded.Count() > 0 || channelsToBeRemoved.Count() > 0)
                {
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task ClientDisconnected(Exception arg)
        {
        }

        private async Task ClientConnected()
        {
            UpdateRelayChannels();
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
                    if(arg.Channel.Name.StartsWith("stream_"))
                    {
                        Args.DiscordMessageArgs args = new Args.DiscordMessageArgs();
                        args.Source = arg.Channel.Name;
                        args.Message = arg.Author.Username+": "+arg.Content;
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            var stream = _context.StreamModels.Where(sm => sm.DiscordRelayChannel.ToLower() == arg.Channel.Name.ToLower()).FirstOrDefault();
                            if(stream != null)
                            {
                                args.Target = stream.StreamName;
                                args.StreamType = stream.Type;
                            }
                        }
                        _eventBus.TriggerEvent(EventType.DiscordMessageReceived, args);
                    }
                }
            }
        }
    }
}

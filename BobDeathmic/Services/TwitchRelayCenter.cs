using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;

namespace BobDeathmic.Services
{
    public class TwitchRelayCenter : BackgroundService
    {
        private readonly IEventBus _eventBus;
        private TwitchClient client;
        private Dictionary<string, string> RelayMapping;
        private Dictionary<string, string> MessageQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        public TwitchRelayCenter(IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            _eventBus.StreamChanged += StreamChanged;
            _eventBus.DiscordMessageReceived += DiscordMessageReceived;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Init();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
        

        private void StreamChanged(object sender, StreamEventArgs e)
        {
            //join RelayChannel
        }
        public void Init()
        {
            RelayMapping = new Dictionary<string, string>();
            MessageQueue = new Dictionary<string, string>();
            client = new TwitchClient();
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                SecurityToken token = null;
                while(token == null || token != null && token.token == "")
                {
                    token = _context.SecurityTokens.Where(st => st.service == Models.Enum.TokenType.Twitch).FirstOrDefault();
                    if(token == null || token != null && token.token == "")
                    {
                        Thread.Sleep(5000);
                    }
                }
                
                ConnectionCredentials credentials = new ConnectionCredentials("BotDeathmic", "oauth:"+token.token);
                client.Initialize(credentials);

            }
            client.OnJoinedChannel += onJoinedChannel;
            client.OnMessageReceived += onMessageReceived;
            client.OnConnected += onConnected;
            client.OnIncorrectLogin += onIncorrectLogin;
            client.Connect();
        }

        private void DiscordMessageReceived(object sender, DiscordMessageArgs e)
        {

        }

        private void onIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
            Console.WriteLine("test");
        }

        private void onConnected(object sender, OnConnectedArgs e)
        {
            
        }

        private void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
        }

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
        }
    }
}

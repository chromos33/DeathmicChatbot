using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Services.Helper;
using BobDeathmic.Services.Helper.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        
        //MessageQueue
        private Dictionary<string, List<string>> MessageQueues;
        private readonly IServiceScopeFactory _scopeFactory;
        private System.Timers.Timer _MessageTimer;
        private List<IfCommand> CommandList;
        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            client.Disconnect();
            _eventBus.StreamChanged -= StreamChanged;
            _eventBus.DiscordMessageReceived -= DiscordMessageReceived;
            Dispose();
        }
        public TwitchRelayCenter(IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            _MessageTimer = new System.Timers.Timer(500);
            _MessageTimer.Elapsed += (sender, args) => SendMessages();
            _MessageTimer.Start();
        }

        private void SendMessages()
        {
            if(MessageQueues != null && MessageQueues.Count() > 0)
            {
                foreach (var MessageQueue in MessageQueues.Where(mq => mq.Value.Count > 0))
                {
                    client.SendMessage(MessageQueue.Key, MessageQueue.Value.First());
                    MessageQueue.Value.RemoveAt(0);
                    
                }
            }
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Init();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
            return;
        }

        private bool StreamChangedInProgress = false;
        private async void StreamChanged(object sender, StreamEventArgs e)
        {
            if(e.StreamType == Models.Enum.StreamProviderTypes.Twitch && e.relayactive != Models.Enum.RelayState.NotActivated)
            {
                while (!client.IsConnected)
                {
                    await Task.Delay(1000);
                }
                //join RelayChannel
                if(MessageQueues == null)
                {
                    MessageQueues = new Dictionary<string, List<string>>();
                }
                if (e.state != Models.Enum.StreamState.NotRunning)
                {
                    if (!MessageQueues.ContainsKey(e.stream))
                    {
                        string Relaychannel = await SetRelayChannel(e);
                        if(Relaychannel != "")
                        {
                            e.Notification += $" Das Relay befindet sich in Channel {Relaychannel}";
                        }
                        client.JoinChannel(e.stream);

                        MessageQueues.Add(e.stream, new List<string>());
                    }
                }
                else
                {
                    if (MessageQueues.ContainsKey(e.stream))
                    {
                        MessageQueues.Remove(e.stream);
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            var stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower() == e.channel.ToLower()).FirstOrDefault();
                            if(Regex.Match(stream.DiscordRelayChannel.ToLower(), @"stream_\d+").Success)
                            {
                                stream.DiscordRelayChannel = "An";
                                await _context.SaveChangesAsync();
                            }
                        }
                        //Remove Relay Channel from stream if it is a generic DiscordChannel
                    }
                }
                if(e.PostUpTime)
                {
                    string UpTimeMessage = $"Stream läuft seit {e.Uptime.Hours} Stunden und {e.Uptime.Minutes} Minuten";
                    MessageQueues[e.stream].Add(UpTimeMessage);
                }
            }
            _eventBus.TriggerEvent(EventType.RelayPassed, e);
        }
        private async Task<string> SetRelayChannel(StreamEventArgs e)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower() == e.channel.ToLower()).FirstOrDefault();
                if (stream != null && stream.DiscordRelayChannel == "An")
                {
                    foreach (var RelayChannel in _context.RelayChannels.Where(rc => Regex.Match(rc.Name.ToLower(), @"stream_\d+").Success))
                    {
                        if (_context.StreamModels.Where(sm => sm.DiscordRelayChannel.ToLower() == RelayChannel.Name.ToLower()).Count() == 0)
                        {
                            stream.DiscordRelayChannel = RelayChannel.Name;
                            _context.Update(stream);
                            Console.WriteLine(await _context.SaveChangesAsync());
                            return RelayChannel.Name;
                        }
                    }
                }
            }
            return null;
        }
        public async void Init()
        {
            _eventBus.StreamChanged += StreamChanged;
            _eventBus.DiscordMessageReceived += DiscordMessageReceived;
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
                        await Task.Delay(1000);
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
            if(e.StreamType == Models.Enum.StreamProviderTypes.Twitch)
            {
                MessageQueues[e.Target].Add(e.Message);
            }
        }

        private void onIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
            
        }

        private void onConnected(object sender, OnConnectedArgs e)
        {
            CommandList = CommandBuilder.BuildCommands("twitch");
        }

        private async void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            var twitchchannel = client.JoinedChannels.Where(channel => channel.Channel == e.Channel).FirstOrDefault();
            client.SendMessage(twitchchannel, "Relay Started");
            //Trigger Event to Send "Relay Started" to discord or somesuch
            string target = "";
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower() == e.Channel.ToLower()).FirstOrDefault();
                if(stream != null)
                {
                    target = stream.DiscordRelayChannel;
                }
            }
            if(target != null && target != "")
            {
                _eventBus.TriggerEvent(EventType.TwitchMessageReceived, new TwitchMessageArgs { Source = e.Channel, Message = $"Relay Started ({e.Channel})", Target = target});
            }
            
        }

        private async void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if(!e.ChatMessage.IsMe)
            {
                bool isCommand = false;
                if(CommandList != null)
                {
                    foreach (IfCommand command in CommandList)
                    {
                        Dictionary<String, String> inputargs = new Dictionary<string, string>();
                        inputargs["message"] = e.ChatMessage.Message;
                        inputargs["username"] = e.ChatMessage.Username;
                        inputargs["source"] = "twitch";
                        inputargs["channel"] = e.ChatMessage.Channel;
                        string CommandResult = await command.ExecuteCommandIfApplicable(inputargs);
                        if (CommandResult != "")
                        {
                            isCommand = true;
                            var twitchchannel = client.JoinedChannels.Where(channel => channel.Channel == e.ChatMessage.Channel).FirstOrDefault();
                            client.SendMessage(twitchchannel, CommandResult);
                        }
                        if(e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster)
                        {
                            CommandEventType EventType = await command.EventToBeTriggered(inputargs);
                            switch (EventType)
                            {
                                case CommandEventType.None:
                                    break;
                                case CommandEventType.TwitchTitle:
                                    isCommand = true;
                                    _eventBus.TriggerEvent(Eventbus.EventType.StreamTitleChangeRequested, new StreamTitleChangeArgs { StreamName = e.ChatMessage.Channel, Type = Models.Enum.StreamProviderTypes.Twitch, Message = e.ChatMessage.Message });
                                    break;
                            }
                        }
                    }
                }
                //Trigger Event to Send "Relay Started" to discord or somesuch
                if(!isCommand)
                {
                    string target = "";
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower() == e.ChatMessage.Channel.ToLower()).FirstOrDefault();
                        if (stream != null)
                        {
                            target = stream.DiscordRelayChannel;
                        }
                    }
                    if (target != null && target != "")
                    {
                        _eventBus.TriggerEvent(EventType.TwitchMessageReceived, new TwitchMessageArgs { Source = e.ChatMessage.Channel, Message = e.ChatMessage.Username + ": " + e.ChatMessage.Message, Target = target });
                    }
                }
            }
        }
    }
}

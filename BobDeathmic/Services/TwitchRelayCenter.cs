using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Services.Helper;
using BobDeathmic.Services.Helper.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using System.Diagnostics.Contracts;
using TwitchLib.Communication.Events;

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
        private System.Timers.Timer _AutoCommandTimer;
        private readonly IConfiguration _configuration;
        private List<IfCommand> CommandList;


        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            client.Disconnect();
        }
        public TwitchRelayCenter(IServiceScopeFactory scopeFactory, IEventBus eventBus, IConfiguration configuration)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            CommandList = CommandBuilder.BuildCommands("twitch");

        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitTwitchClient();
            InitRelayBusEvents();
            _MessageTimer = new System.Timers.Timer(50);
            _MessageTimer.Elapsed += (sender, args) => SendMessages();
            _MessageTimer.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
            return;
        }
        private void SendMessages()
        {
            if (MessageQueues != null && client != null && client.IsConnected)
            {
                if (MessageQueues.Count() > 0)
                {
                    foreach (var MessageQueue in MessageQueues.Where(mq => mq.Value.Count > 0))
                    {
                        client.SendMessage(MessageQueue.Key, MessageQueue.Value.First());
                        MessageQueue.Value.RemoveAt(0);
                    }
                }
            }
        }

        private async Task InitTwitchClient()
        {
            client = new TwitchClient();
            client.OnConnected += HasConnected;
            client.OnConnectionError += ConnectionError;
            client.OnDisconnected += Disconnected;
            client.OnIncorrectLogin += LoginAuthFailed;
            client.OnJoinedChannel += ChannelJoined;
            client.OnLeftChannel += ChannelLeft;
            client.OnMessageReceived += MessageReceived;
            client.Initialize(await GetTwitchCredentials());
        }

        private void InitRelayBusEvents()
        {
            _eventBus.StreamChanged += StreamChanged;
            _eventBus.DiscordMessageReceived += DiscordMessageReceived;
        }
        private bool ConnectionChangeInProgress;
        private async Task Connect()
        {
            Console.WriteLine("trying to connect");
            if(!ConnectionChangeInProgress)
            {
                ConnectionChangeInProgress = true;
                //client.Initialize(await GetTwitchCredentials());
                client.Connect();
            }
            
        }
        #region EventHandler
        #region TwitchClientEvents
        private void HasConnected(object sender, OnConnectedArgs e)
        {
            ConnectionChangeInProgress = false;
            Console.WriteLine("TwitchRelayCenter Connected");
            if (_AutoCommandTimer == null)
            {
                _AutoCommandTimer = new System.Timers.Timer();
                _AutoCommandTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
                _AutoCommandTimer.Elapsed += (autocommandsender, args) => ExecuteAutoCommands();
            }
            if(!_AutoCommandTimer.Enabled)
            {
                _AutoCommandTimer.Start();
            }
        }

        private void ExecuteAutoCommands()
        {
            if(MessageQueues.Count() > 0)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    foreach(var messageQueue in MessageQueues)
                    {
                        var commands = _context.StreamCommand.Include(sc => sc.stream).Where(sc => sc.Mode == StreamCommandMode.Auto && sc.stream.StreamName == messageQueue.Key && sc.AutoInverval > 0 && (DateTime.Now - sc.LastExecution).Minutes > sc.AutoInverval);
                        foreach (StreamCommand command in commands)
                        {
                            messageQueue.Value.Add(command.response);
                            command.LastExecution = DateTime.Now;
                        }
                    }
                    _context.SaveChanges();
                }
            }
        }
        private async void LoginAuthFailed(object sender, OnIncorrectLoginArgs e)
        {
            ConnectionChangeInProgress = false;
            Console.WriteLine("Invalid Credentials");
            await RefreshToken();
            client.Disconnect();
            client.SetConnectionCredentials(await GetTwitchCredentials());
        }
        private void Disconnected(object sender, OnDisconnectedEventArgs e)
        {
            ConnectionChangeInProgress = false;
            if (_AutoCommandTimer != null)
            {
                _AutoCommandTimer.Stop();
            }
            RefreshToken();
            Console.WriteLine("TwitchRelayCenter Disconnected");
        }
        private void ConnectionError(object sender, OnConnectionErrorArgs e)
        {
            ConnectionChangeInProgress = false;
            Console.WriteLine("Connection Error");
        }

        

        private void ChannelJoined(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Joined Channel");
            var twitchchannel = client.JoinedChannels.Where(channel => channel.Channel == e.Channel).FirstOrDefault();
            client.SendMessage(twitchchannel, "Relay Started");
            //Trigger Event to Send "Relay Started" to discord or somesuch
            string target = "";
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower() == e.Channel.ToLower()).FirstOrDefault();
                if (stream != null)
                {
                    target = stream.DiscordRelayChannel;
                }
            }
            if (!string.IsNullOrEmpty(target))
            {
                _eventBus.TriggerEvent(EventType.TwitchMessageReceived, new TwitchMessageArgs { Source = e.Channel, Message = $"Relay Started ({e.Channel})", Target = target });
            }
        }
        private void ChannelLeft(object sender, OnLeftChannelArgs e)
        {
            Console.WriteLine("Left Channel");
        }
        private async void MessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if(!e.ChatMessage.IsMe)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    if(MessageQueues.ContainsKey(e.ChatMessage.Channel.ToLower()))
                    {
                        string message = GetManualCommandResponse(e.ChatMessage.Channel, e.ChatMessage.Message);
                        if(message != "")
                        {
                            MessageQueues[e.ChatMessage.Channel].Add(message);
                        }   
                    }
                }
                if (!e.ChatMessage.Message.StartsWith("!", StringComparison.CurrentCulture))
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
                else
                {
                    if (CommandList != null)
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
                                var twitchchannel = client.JoinedChannels.Where(channel => channel.Channel == e.ChatMessage.Channel).FirstOrDefault();
                                client.SendMessage(twitchchannel, CommandResult);
                            }
                            if (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster)
                            {
                                CommandEventType EventType = await command.EventToBeTriggered(inputargs);
                                switch (EventType)
                                {
                                    case CommandEventType.None:
                                        break;
                                    case CommandEventType.TwitchTitle:
                                        _eventBus.TriggerEvent(Eventbus.EventType.StreamTitleChangeRequested, new StreamTitleChangeArgs { StreamName = e.ChatMessage.Channel, Type = Models.Enum.StreamProviderTypes.Twitch, Message = e.ChatMessage.Message });
                                        break;
                                }
                            }
                        }
                    }
                }
                
            }
        }
        private string GetManualCommandResponse(string streamname,string message)
        {

            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var command =  _context.StreamCommand.Include(sc => sc.stream).Where(sc => sc.Mode == StreamCommandMode.Manual && sc.stream.StreamName == streamname && message.Contains(sc.name)).FirstOrDefault();
                if(command != null)
                {
                    return command.response;
                }
                return "";  
            }
        }
        #endregion
        #region RelayBus Events
        private async void StreamChanged(object sender, StreamEventArgs e)
        {
            Console.WriteLine("StreamChanged");
            StreamEventArgs args = e;
            if(e.StreamType == Models.Enum.StreamProviderTypes.Twitch && e.relayactive == Models.Enum.RelayState.Activated)
            {
                if (args.state == Models.Enum.StreamState.NotRunning)
                {
                    if(MessageQueues != null)
                    {
                        RemoveMessageQueue(args);
                        LeaveChannel(args);
                    } 
                }
                else
                {
                    while (ConnectionChangeInProgress)
                    {
                        await Task.Delay(5000);
                    }
                    if (!client.IsConnected)
                    {
                        await Connect();
                    }
                    AddMessageQueue(args);
                    JoinChannel(args);
                    if(args.relayactive == Models.Enum.RelayState.Activated)
                    {
                        string relayChannel = await SetRelayChannel(args);
                        args = UpdateNotification(relayChannel, args);
                    }
                    if (e.PostUpTime)
                    {
                        string UpTimeMessage = $"Stream läuft seit {e.Uptime.Hours} Stunden und {e.Uptime.Minutes} Minuten";
                        MessageQueues[e.stream].Add(UpTimeMessage);
                    }
                }
            }
            if(e.StreamType == Models.Enum.StreamProviderTypes.Twitch && e.state == Models.Enum.StreamState.Started)
            {
                _eventBus.TriggerEvent(EventType.RelayPassed, args);
            }
            
        }

        private void LeaveChannel(StreamEventArgs args)
        {
            if(client.IsConnected && client.JoinedChannels.Any(x => x.Channel == args.stream))
            {
                client.LeaveChannel(args.stream);
            }
            
        }

        private void JoinChannel(StreamEventArgs args)
        {
            if(client.IsConnected && client.JoinedChannels.Where(x => x.Channel == args.stream).Count() == 0)
            {
                try
                {
                    client.JoinChannel(args.stream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private StreamEventArgs UpdateNotification(string relayChannel,StreamEventArgs e)
        {
            if(relayChannel != "")
            {
                e.Notification += $" Das Relay befindet sich in Channel {relayChannel}";
            }
            return e;
        }

        private async Task<string> SetRelayChannel(StreamEventArgs e)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower() == e.stream.ToLower()).FirstOrDefault();
                if (stream != null && stream.DiscordRelayChannel == "An")
                {
                    foreach (var RelayChannel in _context.RelayChannels.Where(rc => Regex.Match(rc.Name.ToLower(), @"stream_\d+").Success))
                    {
                        if (_context.StreamModels.Where(sm => sm.DiscordRelayChannel.ToLower() == RelayChannel.Name.ToLower()).Count() == 0)
                        {
                            stream.DiscordRelayChannel = RelayChannel.Name;
                            await _context.SaveChangesAsync();
                            return RelayChannel.Name;
                        }
                    }
                }
            }
            return "";
        }

        private void DiscordMessageReceived(object sender, DiscordMessageArgs e)
        {
            if (e.StreamType == Models.Enum.StreamProviderTypes.Twitch)
            {
                MessageQueues[e.Target].Add(e.Message);
            }
        }
        #endregion
        #endregion
        private void AddMessageQueue(StreamEventArgs e)
        {
            if(MessageQueues == null)
            {
                MessageQueues = new Dictionary<string, List<string>>();
            }
            if(!MessageQueues.ContainsKey(e.stream))
            {
                MessageQueues.Add(e.stream,new List<string>());
            }
        }
        private void RemoveMessageQueue(StreamEventArgs args)
        {
            if(MessageQueues.ContainsKey(args.stream))
            {
                MessageQueues.Remove(args.stream);
            }
        }
        private async Task<ConnectionCredentials> GetTwitchCredentials()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                SecurityToken token = null;
                while (token == null || token != null && token.token == "")
                {
                    token = _context.SecurityTokens.Where(st => st.service == Models.Enum.TokenType.Twitch).FirstOrDefault();
                    if (token == null || token != null && token.token == "")
                    {
                        await Task.Delay(1000);
                    }
                }
                ConnectionCredentials credentials = new ConnectionCredentials("BobDeathmic", "oauth:" + token.token);
                return credentials;
            }
        }

        private async Task RefreshToken()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var savedtoken = _context.SecurityTokens.Where(x => x.service == Models.Enum.TokenType.Twitch).FirstOrDefault();
                if (savedtoken != null && savedtoken.RefreshToken != null && savedtoken.RefreshToken != "")
                {
                    var httpclient = new HttpClient();
                    string baseUrl = _configuration.GetValue<string>("WebServerWebAddress");
                    string url = $"https://id.twitch.tv/oauth2/token?grant_type=refresh_token&refresh_token={savedtoken.RefreshToken}&client_id={savedtoken.ClientID}&client_secret={savedtoken.secret}";
                    var response = await httpclient.PostAsync(url, new StringContent("", System.Text.Encoding.UTF8, "text/plain"));
                    var responsestring = await response.Content.ReadAsStringAsync();
                    JSONObjects.TwitchRefreshTokenData refresh = JsonConvert.DeserializeObject<JSONObjects.TwitchRefreshTokenData>(responsestring);
                    if (refresh.error == null)
                    {
                        savedtoken.token = refresh.access_token;
                        savedtoken.RefreshToken = refresh.refresh_token;
                        _context.SaveChanges();
                        Console.WriteLine("Refreshed Token");
                    }
                }
            }
        }

    }
}

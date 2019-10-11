using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Services.Helper;
using BobDeathmic.Services.Helper.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        private System.Timers.Timer _RelayCheckTimer;
        private readonly IConfiguration _configuration;
        private List<IfCommand> CommandList;
        private Random random;


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
            random = new Random();
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitTwitchClient();
            InitRelayBusEvents();
            InitMessageQueue();
            _MessageTimer = new System.Timers.Timer(50);
            _MessageTimer.Elapsed += (sender, args) => SendMessages();
            _MessageTimer.Start();
            _RelayCheckTimer = new System.Timers.Timer(5000);
            _RelayCheckTimer.Elapsed += CheckRelays;
            _RelayCheckTimer.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
            return;
        }

        private async void CheckRelays(object sender, ElapsedEventArgs e)
        {

            while (ConnectionChangeInProgress)
            {
                await Task.Delay(5000);
            }
            if (!client.IsConnected)
            {
                await Connect();
            }
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                handleRelayStart(_context);
                handleRelayEnd(_context);
                handleUpTime(_context);
                handleConnectionUpkeep(_context);
            }
        }
        private void handleUpTime(ApplicationDbContext _context)
        {
            foreach(var stream in _context.StreamModels.Where(x => x.UpTimeQueued() && MessageQueues.Keys.Contains(x.StreamName)))
            {
                MessageQueues[stream.StreamName].Add(stream.UptimeMessage());
            }
            _context.SaveChanges();
        }
        private void handleConnectionUpkeep(ApplicationDbContext _context)
        {
            foreach (var stream in _context.StreamModels.Where(x => x.StreamState == Models.Enum.StreamState.Running && MessageQueues.Keys.Contains(x.StreamName.ToLower()) && x.DiscordRelayChannel != "Aus"))
            {
                if (client.JoinedChannels.Where(x => x.Channel.ToLower() == stream.StreamName.ToLower()).Count() == 0)
                {
                    JoinChannel(stream.StreamName);
                }
            }
        }
        private void handleRelayStart(ApplicationDbContext _context)
        {
            foreach (var stream in _context.StreamModels.Where(x => x.StreamState == Models.Enum.StreamState.Running && !MessageQueues.Keys.Contains(x.StreamName.ToLower()) && (x.DiscordRelayChannel != "Aus" && x.DiscordRelayChannel != "" && x.DiscordRelayChannel != null )))
            {
                AddMessageQueue(stream.StreamName);
                JoinChannel(stream.StreamName);
            } 
        }
        private void handleRelayEnd(ApplicationDbContext _context)
        {
            foreach (var stream in _context.StreamModels.Where(x => x.StreamState == Models.Enum.StreamState.NotRunning && MessageQueues.Keys.Contains(x.StreamName.ToLower()) && x.DiscordRelayChannel != "Aus"))
            {
                RemoveMessageQueue(stream.StreamName);
                LeaveChannel(stream.StreamName);
            }
        }
        private async Task SendMessages()
        {
            if (MessageQueues != null && client != null && client.IsConnected)
            {
                if (MessageQueues.Count() > 0)
                {
                    foreach (var MessageQueue in MessageQueues.Where(mq => mq.Value.Count > 0))
                    {
                        await Task.Delay(100);
                        client.SendMessage(MessageQueue.Key, MessageQueue.Value.First());
                        MessageQueue.Value.RemoveAt(0);
                    }
                }
            }
        }
        private void SendPriorityMessage(string channel,string message)
        {
            try
            {
                _MessageTimer.Stop();
                client.SendMessage(channel, message);
            }catch(Exception ex)
            {
                //don't care
            }finally
            {
                _MessageTimer.Start();
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
            _eventBus.RelayMessageReceived += RelayMessageReceived;
        }
        private bool ConnectionChangeInProgress;
        private async Task Connect()
        {
            if (!ConnectionChangeInProgress)
            {
                ConnectionChangeInProgress = true;
                client.Connect();
            }
        }
        #region EventHandler
        #region TwitchClientEvents
        private void HasConnected(object sender, OnConnectedArgs e)
        {
            ConnectionChangeInProgress = false;
            if (_AutoCommandTimer == null)
            {
                _AutoCommandTimer = new System.Timers.Timer();
                _AutoCommandTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
                _AutoCommandTimer.Elapsed += (autocommandsender, args) => ExecuteAutoCommands();
            }
            if (!_AutoCommandTimer.Enabled)
            {
                _AutoCommandTimer.Start();
            }
        }

        private void ExecuteAutoCommands()
        {
            if (MessageQueues.Count() > 0)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    foreach (var messageQueue in MessageQueues)
                    {
                        var commands = _context.StreamCommand.Include(sc => sc.stream).Where(sc => sc.Mode == StreamCommandMode.Auto && sc.stream.StreamName == messageQueue.Key && sc.AutoInverval > 0 && (DateTime.Now - sc.LastExecution).Minutes > sc.AutoInverval);
                        foreach (StreamCommand command in commands)
                        {
                            messageQueue.Value.Add(command.response);
                            command.LastExecution = DateTime.Now;
                        }
                        commands = _context.StreamCommand.Include(sc => sc.stream).Where(sc => sc.Mode == StreamCommandMode.Random && sc.stream.StreamName == messageQueue.Key && sc.AutoInverval > 0 && (DateTime.Now - sc.LastExecution).Minutes > sc.AutoInverval);
                        foreach (StreamCommand command in commands)
                        {
                            string[] Zitate = command.response.Split("|");
                            messageQueue.Value.Add(Zitate[random.Next(Zitate.Count() - 1)]);
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
        }
        private void ConnectionError(object sender, OnConnectionErrorArgs e)
        {
            ConnectionChangeInProgress = false;
        }



        private void ChannelJoined(object sender, OnJoinedChannelArgs e)
        {
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
            
        }
        private async void MessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (!e.ChatMessage.IsMe)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    if (MessageQueues.ContainsKey(e.ChatMessage.Channel.ToLower()))
                    {
                        string message = GetManualCommandResponse(e.ChatMessage.Channel, e.ChatMessage.Message);
                        if (message != "")
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
                            inputargs["elevatedPermissions"] = (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster).ToString();
                            string CommandResult = await command.ExecuteCommandIfApplicable(inputargs, _scopeFactory);
                            if (CommandResult != "")
                            {
                                var twitchchannel = client.JoinedChannels.Where(channel => channel.Channel == e.ChatMessage.Channel).FirstOrDefault();
                                client.SendMessage(twitchchannel, CommandResult);
                            }
                            string WhisperCommandResult = await command.ExecuteWhisperCommandIfApplicable(inputargs, _scopeFactory);
                            if (WhisperCommandResult != "")
                            {
                                client.SendWhisper(e.ChatMessage.Username, WhisperCommandResult);
                            }
                            CommandEventType EventType = await command.EventToBeTriggered(inputargs);
                            switch (EventType)
                            {
                                case CommandEventType.None:
                                    break;
                                case CommandEventType.Strawpoll:
                                    if (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster)
                                    {
                                        _eventBus.TriggerEvent(Eventbus.EventType.StrawPollRequested, new StrawPollRequestEventArgs { StreamName = e.ChatMessage.Channel, Type = Models.Enum.StreamProviderTypes.Twitch, Message = e.ChatMessage.Message });
                                    }
                                    break;
                                case CommandEventType.TwitchTitle:
                                    if (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster)
                                    {

                                        _eventBus.TriggerEvent(Eventbus.EventType.StreamTitleChangeRequested, PrepareStreamTitleChange(e.ChatMessage.Channel, e.ChatMessage.Message));
                                    }
                                    break;
                            }
                        }
                    }
                }

            }
        }
        private StreamTitleChangeArgs PrepareStreamTitleChange(string StreamName, string Message)
        {
            var arg = new StreamTitleChangeArgs();
            arg.StreamName = StreamName;
            arg.Type = Models.Enum.StreamProviderTypes.Twitch;
            if (Message.StartsWith("!stream"))
            {
                var questionRegex = Regex.Match(Message, @"game=\'(.*?)\'");
                var GameRegex = Regex.Match(Message, @"game=\'(.*?)\'");
                string Game = "";
                if (GameRegex.Success)
                {
                    Game = GameRegex.Value;
                }
                var TitleRegex = Regex.Match(Message, @"title=\'(.*?)\'");
                string Title = "";
                if (TitleRegex.Success)
                {
                    Title = TitleRegex.Value;
                }
                arg.Game = Game;
                arg.Title = Title;
            }
            else
            {
                if (Message.StartsWith("!game"))
                {
                    arg.Game = Message.Replace("!game ", "");
                    arg.Title = "";
                }
                if (Message.StartsWith("!title"))
                {
                    arg.Title = Message.Replace("!title ", "");
                    arg.Game = "";
                }
            }
            return arg;
        }
        private string GetManualCommandResponse(string streamname, string message)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var command = _context.StreamCommand.Include(sc => sc.stream).Where(sc => sc.Mode == StreamCommandMode.Manual && sc.stream.StreamName == streamname && message.Contains(sc.name)).FirstOrDefault();
                    if (command != null)
                    {
                        return command.response;
                    }
                    return "";
                }
            }catch(MySqlException e)
            {
                var test = streamname;
                var test2 = message;
                return "";
            }
            
        }
        #endregion
        #region RelayBus Events
        private void LeaveChannel(string name)
        {
            if (client.IsConnected && client.JoinedChannels.Any(x => x.Channel == name))
            {
                SendPriorityMessage(name, "Relay is leaving");
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var stream = _context.StreamModels.Where(x => x.StreamName.ToLower() == name.ToLower()).FirstOrDefault();
                    if(stream != null)
                    {
                        _eventBus.TriggerEvent(EventType.TwitchMessageReceived, new TwitchMessageArgs { Message = $"Relay left ({stream.StreamName})", Target = stream.DiscordRelayChannel });
                    }
                }
                client.LeaveChannel(name);
            }

        }

        private void JoinChannel(string name)
        {
            if (client.IsConnected && client.JoinedChannels.Where(x => x.Channel == name).Count() == 0)
            {
                try
                {
                    client.JoinChannel(name.ToLower());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        
        private void RelayMessageReceived(object sender, RelayMessageArgs e)
        {
            if (e.StreamType == Models.Enum.StreamProviderTypes.Twitch)
            {
                MessageQueues[e.TargetChannel].Add(e.Message);
            }
        }
        #endregion
        #endregion
        private void InitMessageQueue()
        {
            if (MessageQueues == null)
            {
                MessageQueues = new Dictionary<string, List<string>>();
            }
        }
        private void AddMessageQueue(string name)
        {
            if (!MessageQueues.ContainsKey(name))
            {
                MessageQueues.Add(name, new List<string>());
            }
        }
        private void RemoveMessageQueue(string name)
        {
            if (MessageQueues.ContainsKey(name))
            {
                MessageQueues.Remove(name);
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
                    }
                }
            }
        }

    }
}

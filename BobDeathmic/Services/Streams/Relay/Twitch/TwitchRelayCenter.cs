using BobDeathmic;
using BobDeathmic.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Services;
using BobDeathmic.Services.Helper;
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
using BobDeathmic;
using BobDeathmic.Services;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Services.Commands;

namespace BobDeathmic.Services.Streams.Relay.Twitch
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
        //TODO once CommandService Finished Remove next line
        private List<ICommand> CommandList;
        private Random random;
        private ICommandService commandService;


        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            client.Disconnect();
        }
        public TwitchRelayCenter(IServiceScopeFactory scopeFactory, IEventBus eventBus, IConfiguration configuration, ICommandService commandService)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            this.commandService = commandService;
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
            foreach (var stream in _context.StreamModels.Where(x => x.StreamState == StreamState.Running && MessageQueues.Keys.Contains(x.StreamName.ToLower()) && x.DiscordRelayChannel != "Aus"))
            {
                if (client.JoinedChannels.Where(x => x.Channel.ToLower() == stream.StreamName.ToLower()).Count() == 0)
                {
                    JoinChannel(stream.StreamName);
                }
            }
        }
        private void handleRelayStart(ApplicationDbContext _context)
        {
            foreach (var stream in _context.StreamModels.Where(x => x.StreamState == StreamState.Running && !MessageQueues.Keys.Contains(x.StreamName.ToLower()) && (x.DiscordRelayChannel != "Aus" && x.DiscordRelayChannel != "" && x.DiscordRelayChannel != null )))
            {
                AddMessageQueue(stream.StreamName);
                JoinChannel(stream.StreamName);
            } 
        }
        private void handleRelayEnd(ApplicationDbContext _context)
        {
            foreach (var stream in _context.StreamModels.Where(x => x.StreamState == StreamState.NotRunning && MessageQueues.Keys.Contains(x.StreamName.ToLower()) && x.DiscordRelayChannel != "Aus"))
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
            client.OnError += ErrorReceived;
            client.OnMessageThrottled += MessageThrottled;
            client.OnWhisperReceived += WhisperReceived;
            client.Initialize(await GetTwitchCredentials());
        }
        void ErrorReceived(object sender, OnErrorEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }
        void MessageThrottled(object sender, OnMessageThrottledEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
        void WhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            Console.WriteLine(e.WhisperMessage);
        }


        private void InitRelayBusEvents()
        {
            _eventBus.RelayMessageReceived += RelayMessageReceived;
            _eventBus.CommandOutputReceived += handleCommandResponse;
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
                _AutoCommandTimer.Elapsed += (quotesender, args) => HandleAutoQuotes();
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
                    }
                    _context.SaveChanges();
                }
            }
        }
        private void HandleAutoQuotes()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var streamsWithQuoteInterval = _context.StreamModels.Where(x => x.QuoteInterval > 0);
                foreach(var stream in streamsWithQuoteInterval)
                {
                    if(DateTime.Now.Subtract(stream.LastRandomQuote) > TimeSpan.FromMinutes(stream.QuoteInterval))
                    {
                        var QuoteList = _context.Quotes.Where(x => x.Streamer.ToLower() == stream.StreamName.ToLower());
                        var MessageQueue = MessageQueues.Where(x => x.Key == stream.StreamName).FirstOrDefault();
                        if (!MessageQueue.Equals(default(KeyValuePair<string, List<string>>)))
                        {
                            MessageQueue.Value.Add(QuoteList.ToArray()[random.Next(QuoteList.Count() - 1)].ToString());
                            stream.LastRandomQuote = DateTime.Now;
                            _context.SaveChanges();
                        }
                    }
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
                    ChatCommands.Args.ChatCommandInputArgs inputargs = new ChatCommands.Args.ChatCommandInputArgs()
                    {
                        Message = e.ChatMessage.Message,
                        Sender = e.ChatMessage.Username,
                        ChannelName = e.ChatMessage.Channel,
                        elevatedPermissions = (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster),
                        Type = Data.Enums.ChatType.Twitch
                    };
                    commandService.handleCommand(inputargs,Data.Enums.ChatType.Twitch,e.ChatMessage.Username);
                }

            }
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
            if (e.StreamType == StreamProviderTypes.Twitch)
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
        private void AddMessageToQueue(string name,string message)
        {
            if (!MessageQueues.ContainsKey(name))
            {
                AddMessageQueue(name);
            }
            MessageQueues[name].Add(message);
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
                    token = _context.SecurityTokens.Where(st => st.service == TokenType.Twitch).FirstOrDefault();
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
                var savedtoken = _context.SecurityTokens.Where(x => x.service == TokenType.Twitch).FirstOrDefault();
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
        public void handleCommandResponse(object sender, CommandResponseArgs e)
        {
            if (e.Chat == Data.Enums.ChatType.Twitch)
            {
                switch (e.MessageType)
                {
                    case Eventbus.MessageType.ChannelMessage:
                        AddMessageToQueue(e.Channel, e.Message);
                        break;
                    case Eventbus.MessageType.PrivateMessage:
                        client.SendWhisper(e.Sender, e.Message);
                        break;
                }
            }
        }

    }
}

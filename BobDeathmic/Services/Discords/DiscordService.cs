using BobDeathmic;
using BobDeathmic.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.GiveAway.manymany;
using BobDeathmic.Data.DBModels.Relay;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Services;
using BobDeathmic.Services.Commands;
using BobDeathmic.Services.Helper;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Discords
{
    public interface IDiscordService
    {
    }
    public class DiscordService : BackgroundService, IDiscordService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IEventBus _eventBus;
        private DiscordSocketClient _client;
        private List<ICommand> _commandList;
        private Random random;
        private ICommandService commandService;

        public DiscordService(IServiceScopeFactory scopeFactory, IEventBus eventBus,ICommandService commandService)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            _eventBus.TwitchMessageReceived += TwitchMessageReceived;
            _eventBus.PasswordRequestReceived += PasswordRequestReceived;
            _eventBus.DiscordMessageSendRequested += SendMessage;
            _eventBus.CommandResponseReceived += handleCommandResponse;
            this.commandService = commandService;
            SetupClient();
        }
        private void SetupClient()
        {
            var discordConfig = new DiscordSocketConfig { MessageCacheSize = 100 };
            discordConfig.AlwaysDownloadUsers = true;
            discordConfig.LargeThreshold = 250;
            _client = new DiscordSocketClient(discordConfig);
        }

        private void PasswordRequestReceived(object sender, PasswordRequestArgs e)
        {
            var server = _client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault();
            var users = server?.Users.Where(u => u.Username.ToLower() == e.UserName.ToLower() && u.Status != UserStatus.Offline);
            var user = users.FirstOrDefault();
            user?.SendMessageAsync("Dein neues Passwort ist: " + e.TempPassword);
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            random = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / (DateTime.Now.Hour + 1));
            if (await ConnectToDiscord())
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(5000, stoppingToken);
                }
            }
            return;
        }
        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await _client.StopAsync();
            Dispose();
        }
        private async Task<bool> ConnectToDiscord()
        {
            string token = GetDiscordToken();
            if (token != string.Empty)
            {
                await _client.LoginAsync(Discord.TokenType.Bot, token);
                await _client.StartAsync();
                InitializeEvents();
                return true;

            }
            return false;
        }
        private void InitializeEvents()
        {
            _client.MessageReceived += MessageReceived;
            _client.UserJoined += ClientJoined;
            _client.ChannelCreated += ChannelCreated;
            _client.ChannelDestroyed += ChannelDestroyed;
            
        }

        private async Task ChannelDestroyed(SocketChannel arg)
        {
            if(arg.GetType() == typeof(SocketTextChannel))
            {
                SocketTextChannel destroyedchannel = (SocketTextChannel)arg;
                RemoveRelayChannel(destroyedchannel.Name);
            }
        }
        public void RemoveRelayChannel(string name)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var channel = _context.RelayChannels.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
                if (channel != null)
                {
                    _context.RelayChannels.Remove(channel);
                    _context.SaveChanges();
                }
            }
        }

        


        private async Task ChannelCreated(SocketChannel arg)
        {
            if (arg.GetType() == typeof(SocketTextChannel) && Regex.Match(((SocketTextChannel) arg).Name.ToLower(), @"stream_").Success)
            {
                SocketTextChannel createdchannel = (SocketTextChannel)arg;
                AddRelayChannel(createdchannel.Name);
            }
        }
        public void AddRelayChannel(string name)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (_context.RelayChannels.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault() == null)
                {
                    _context.RelayChannels.Add(new RelayChannels(name.ToLower()));
                    _context.SaveChanges();
                }
            }
        }

        private string GetDiscordToken()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var token = string.Empty;
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var tokens = _context.SecurityTokens.Where(st => st.service == Data.Enums.Stream.TokenType.Discord).FirstOrDefault();
                if (tokens != null)
                {
                    if (tokens.token == null)
                    {
                        token = tokens.ClientID;
                    }
                    else
                    {
                        token = tokens.token;
                    }

                }
                return token;
            }
        }

        private async Task ClientJoined(SocketGuildUser arg)
        {
            arg.SendMessageAsync("Um Bot Notifications/Features nutzen zu können müsst ihr euch über den Befehl \"!WebInterfaceLink\" beim Bot registrieren");
        }
        private async Task MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Username != "BobDeathmic")
            {
                string commandresult = string.Empty;
                if (arg.Content.StartsWith("!WebInterfaceLink", StringComparison.CurrentCulture) || arg.Content.StartsWith("!wil", StringComparison.CurrentCulture))
                {
                    SendWebInterfaceLink(arg);
                }
                ChatCommands.Args.ChatCommandInputArgs inputargs = new ChatCommands.Args.ChatCommandInputArgs() { 
                    Message = arg.Content,
                    ChannelName = arg.Channel.Name,
                    elevatedPermissions = false,
                    Sender = arg.Author.Username,
                    Type = Data.Enums.ChatType.Discord
                };
                commandService.handleCommand(inputargs, Data.Enums.ChatType.Discord,arg.Author.Username);
                RelayMessage(arg);
            }
        }
        private async Task SendWebInterfaceLink(SocketMessage arg)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                List<ulong> blocked = _context.DiscordBans.Select(x => x.DiscordID).ToList();
                if (!blocked.Contains(arg.Author.Id))
                {
                    string message = await GetUserLogin(arg);
                    arg.Author.SendMessageAsync(message);
                }
                else
                {
                    await arg.Author.SendMessageAsync($"Bitte Discord Namen ändern und an einen Admin wenden und folgenden Wert mitteilen {arg.Author.Id}");
                }
            }
        }
        private void RelayMessage(SocketMessage arg)
        {
            if (arg.Channel.Name.StartsWith("stream_", StringComparison.CurrentCulture))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var stream = _context.StreamModels.Where(sm => sm.DiscordRelayChannel.ToLower() == arg.Channel.Name.ToLower()).FirstOrDefault();
                    if (stream != null)
                    {
                        _eventBus.TriggerEvent(EventType.RelayMessageReceived, new RelayMessageArgs(arg.Channel.Name, stream.StreamName, stream.Type, arg.Author.Username + ": " + arg.Content));
                    }
                }
            }
        }

        #region AccountStuff
        private async Task<string> GetUserLogin(SocketMessage arg)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var usermanager = scope.ServiceProvider.GetRequiredService<UserManager<ChatUserModel>>();
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = _context.ChatUserModels.Where(cm => cm.ChatUserName.ToLower() == arg.Author.Username.ToLower()).FirstOrDefault();
                var Configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                if (user == null)
                {
                    user = await GenerateUser(arg);
                }
                string Message = "Adresse:";
                Message += Configuration.GetValue<string>("WebServerWebAddress");
                if (usermanager.CheckPasswordAsync(user, user.InitialPassword).Result)
                {
                    Message += Environment.NewLine + "UserName; " + user.UserName;
                    Message += Environment.NewLine + "Initiales Passwort: " + user.InitialPassword;
                }
                return Message;
            }
        }
        private async Task<ChatUserModel> GenerateUser(SocketMessage arg)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                ChatUserModel newUser = new ChatUserModel(arg.Author.Username, _context.StreamModels.Where(x => x.StreamName != null));
                newUser.SetInitialPassword();
                var usermanager = scope.ServiceProvider.GetRequiredService<UserManager<ChatUserModel>>();
                await usermanager.CreateAsync(newUser, newUser.InitialPassword);
                await CreateOrAddUserRoles("User", newUser.UserName);
                return newUser;
            }
        }
        private async Task CreateOrAddUserRoles(string role, string name)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var UserManager = scope.ServiceProvider.GetRequiredService<UserManager<ChatUserModel>>();

                    var roleCheck = await RoleManager.RoleExistsAsync(role);
                    if (!roleCheck)
                    {
                        await RoleManager.CreateAsync(new IdentityRole(role));
                    }
                    ChatUserModel user = await UserManager.FindByNameAsync(name);
                    await UserManager.AddToRoleAsync(user, role);
                }
            }
            catch (Exception)
            {
            }
        }
        private void SendMessage(object sender, MessageArgs e)
        {
            SendPrivateMessage(e.RecipientName.ToLower(), e.Message);
        }
        private void TwitchMessageReceived(object sender, TwitchMessageArgs e)
        {
            try
            {
                SendChannelMessage(e.Target.ToLower(), e.Message);
            }
            catch (InvalidOperationException)
            {
                //just an annoyance on start..
            }
        }
        private void SendChannelMessage(string channel,string message)
        {
            _client.Guilds.Single(g => g.Name.ToLower() == "deathmic")?.TextChannels.Single(c => c.Name.ToLower() == channel.ToLower())?.SendMessageAsync(message);
        }
        private void SendPrivateMessage(string username,string message)
        {
            _client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault()?.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefault()?.SendMessageAsync(message);
        }
        void handleCommandResponse(object sender, CommandResponseArgs e)
        {
            if(e.Chat == Data.Enums.ChatType.Discord)
            {
                switch(e.MessageType)
                {
                    case Eventbus.MessageType.ChannelMessage:
                        SendChannelMessage(e.Channel, e.Message);
                        break;
                    case Eventbus.MessageType.PrivateMessage:
                        SendPrivateMessage(e.Sender, e.Message);
                        break;
                }
            }
        }
        #endregion
    }
}

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
        DiscordSocketClient client;
        List<IfCommand> CommandList;
        private Random random;

        public DiscordService(IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            _eventBus.TwitchMessageReceived += TwitchMessageReceived;
            _eventBus.PasswordRequestReceived += PasswordRequestReceived;
            _eventBus.DiscordMessageSendRequested += SendMessage;
        }

        private void SendMessage(object sender, MessageArgs e)
        {
            try
            {
                client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == e.RecipientName.ToLower()).FirstOrDefault()?.SendMessageAsync(e.Message);
            }
            catch (Exception)
            {

            }
        }

        private void PasswordRequestReceived(object sender, PasswordRequestArgs e)
        {
            var server = client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault();
            var users = server?.Users.Where(u => u.Username.ToLower() == e.UserName.ToLower() && u.Status != UserStatus.Offline);
            var user = users.FirstOrDefault();
            user?.SendMessageAsync("Dein neues Passwort ist: " + e.TempPassword);
        }

        private void TwitchMessageReceived(object sender, TwitchMessageArgs e)
        {
            try
            {
                client.Guilds.Single(g => g.Name.ToLower() == "deathmic")?.TextChannels.Single(c => c.Name.ToLower() == e.Target.ToLower())?.SendMessageAsync(e.Message);
            }
            catch (InvalidOperationException)
            {
                //just an annoyance on start..
            }
        }
        private void InitCommands()
        {
            CommandList = CommandBuilder.BuildCommands("discord");
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            random = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / (DateTime.Now.Hour + 1));
            if (await ConnectToDiscord())
            {
                InitCommands();
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
            await client.StopAsync();
            Dispose();
        }
        protected async Task<bool> ConnectToDiscord()
        {
            var discordConfig = new DiscordSocketConfig { MessageCacheSize = 100 };
            discordConfig.AlwaysDownloadUsers = true;
            discordConfig.LargeThreshold = 250;
            client = new DiscordSocketClient(discordConfig);
            string token = GetDiscordToken();
            if (token != string.Empty)
            {
                await client.LoginAsync(Discord.TokenType.Bot, token);
                await client.StartAsync();
                InitializeEvents();
                return true;

            }
            return false;
        }
        private void InitializeEvents()
        {
            client.MessageReceived += MessageReceived;
            client.Ready += ClientConnected;
            client.Disconnected += ClientDisconnected;
            client.UserJoined += ClientJoined;
            client.ChannelCreated += ChannelChanged;
            client.ChannelDestroyed += ChannelChanged;
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

        private async Task ChannelChanged(SocketChannel arg)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            UpdateRelayChannels();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task ClientJoined(SocketGuildUser arg)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            arg.SendMessageAsync("Um Bot Notifications/Features nutzen zu können müsst ihr euch über den Befehl \"!WebInterfaceLink\" beim Bot registrieren");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        private async Task UpdateRelayChannels()
        {
            List<string> discordChannels = GetDiscordStreamChannels();
            List<string> savedChannels = GetSavedRelayChannels();


            List<string> channelsToBeAdded = discordChannels.Except(savedChannels).ToList();
            List<string> channelsToBeRemoved = savedChannels.Except(discordChannels).ToList();
            if (channelsToBeAdded.Any())
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AddChannelsInListToDB(channelsToBeAdded);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            if (channelsToBeRemoved.Any())
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                RemoveChannelsInListFromDB(channelsToBeRemoved);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
        private async Task AddChannelsInListToDB(List<string> channelsToBeAdded)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                ApplicationDbContext _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                foreach (var channel in channelsToBeAdded)
                {
                    _context.RelayChannels.Add(new RelayChannels { Name = channel });
                }
                await _context.SaveChangesAsync();
            }
        }
        private async Task RemoveChannelsInListFromDB(List<string> channelsToBeRemoved)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                ApplicationDbContext _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                foreach (var channel in channelsToBeRemoved)
                {
                    _context.RelayChannels.Remove(_context.RelayChannels.Where(rc => rc.Name == channel).FirstOrDefault());
                }
                await _context.SaveChangesAsync();
            }
        }
        private List<string> GetDiscordStreamChannels()
        {
            List<string> discordChannels = new List<string>();
            foreach (var channel in client.Guilds.Single(g => g.Name.ToLower() == "deathmic").Channels)
            {
                string[] RandomDiscordRelayChannels = { "stream_1", "stream_2", "stream_3" };
                if (Regex.Match(channel.Name.ToLower(), @"stream_").Success && !RandomDiscordRelayChannels.Contains(channel.Name))
                {
                    discordChannels.Add(channel.Name);
                }
            }
            return discordChannels;
        }
        private List<string> GetSavedRelayChannels()
        {
            List<string> savedChannels = new List<string>();
            ApplicationDbContext _context = null;
            using (var scope = _scopeFactory.CreateScope())
            {
                _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                foreach (var channel in _context.RelayChannels)
                {
                    savedChannels.Add(channel.Name);
                }
            }
            return savedChannels;
        }

        private async Task ClientDisconnected(Exception arg)
        {
        }

        private async Task ClientConnected()
        {
            await UpdateRelayChannels();
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
                if (CommandList != null)
                {
                    commandresult = await ExecuteCommands(arg);
                }

                if (commandresult == string.Empty)//Commands later on
                {
                    RelayMessage(arg);
                }
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
        private async Task<string> ExecuteCommands(SocketMessage arg)
        {
            string commandresult = string.Empty;
            foreach (IfCommand command in CommandList)
            {
                Dictionary<string, string> inputargs = new Dictionary<string, string>();
                inputargs["message"] = arg.Content;
                inputargs["username"] = arg.Author.Username;
                inputargs["source"] = "discord";
                inputargs["channel"] = arg.Channel.Name;
                commandresult = await command.ExecuteCommandIfApplicable(inputargs, _scopeFactory);
                if (commandresult != string.Empty)
                {
                    arg.Channel.SendMessageAsync(commandresult);
                }
            }
            if (arg.Content.StartsWith("!Gapply"))
            {
                ApplyToGiveAway(arg);
            }
            if (arg.Content.StartsWith("!Gcease"))
            {
                CeaseApplication(arg);
            }
            return commandresult;
        }
        private void CeaseApplication(SocketMessage arg)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = _context.ChatUserModels.Where(x => x.ChatUserName == arg.Author.Username).FirstOrDefault();
                var remove = _context.User_GiveAway.Where(x => x.UserID == user.Id).FirstOrDefault();
                if (remove != null)
                {
                    _context.User_GiveAway.Remove(remove);
                    _context.SaveChanges();
                }
            }
        }
        private void ApplyToGiveAway(SocketMessage arg)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var GiveAwayItem = _context.GiveAwayItems.Include(x => x.Applicants).Where(x => x.current).FirstOrDefault();
                if (GiveAwayItem.Applicants == null)
                {
                    GiveAwayItem.Applicants = new List<User_GiveAwayItem>();
                }
                var user = _context.ChatUserModels.Where(x => x.ChatUserName == arg.Author.Username).FirstOrDefault();
                if (GiveAwayItem.Applicants.Where(x => x.UserID == user.Id).Count() == 0)
                {

                    var item = _context.GiveAwayItems.Where(x => x.current).FirstOrDefault();
                    if (user != null && item != null)
                    {
                        User_GiveAwayItem relation = new User_GiveAwayItem(user, item);
                        relation.User = user;
                        if (user.AppliedTo == null)
                        {
                            user.AppliedTo = new List<User_GiveAwayItem>();
                        }
                        if (item.Applicants == null)
                        {
                            item.Applicants = new List<User_GiveAwayItem>();
                        }
                        user.AppliedTo.Add(relation);
                        item.Applicants.Add(relation);
                        arg.Author.SendMessageAsync("Teilnahme erfolgreich");
                    }
                    else
                    {
                        arg.Author.SendMessageAsync("Gibt nichs zum teilnehmen");
                    }
                }
                else
                {
                    arg.Author.SendMessageAsync("Nimmst schon teil.");
                }
                _context.SaveChanges();
            }
        }
        private void RelayMessage(SocketMessage arg)
        {
            if (arg.Channel.Name.StartsWith("stream_", StringComparison.CurrentCulture))
            {
                RelayMessageArgs args = new RelayMessageArgs();
                args.SourceChannel = arg.Channel.Name;
                args.Message = arg.Author.Username + ": " + arg.Content;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var stream = _context.StreamModels.Where(sm => sm.DiscordRelayChannel.ToLower() == arg.Channel.Name.ToLower()).FirstOrDefault();
                    if (stream != null)
                    {
                        args.TargetChannel = stream.StreamName;
                        args.StreamType = stream.Type;
                    }
                }
                _eventBus.TriggerEvent(EventType.RelayMessageReceived, args);
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
        #endregion
    }
}

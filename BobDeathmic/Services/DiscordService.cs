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
using BobDeathmic.Services.Helper;
using Microsoft.AspNetCore.Identity;
using BobDeathmic.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using System.Net;
using Microsoft.EntityFrameworkCore;

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
        List<IfCommand> CommandList;
        private Random random;

        public DiscordService(IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            _eventBus.TwitchMessageReceived += TwitchMessageReceived;
            _eventBus.PasswordRequestReceived += PasswordRequestReceived;
            //Event for Streams that are not Piggy backed through Relay
            _eventBus.StreamChanged += StreamChanged;
            //Piggy backing through Relay
            _eventBus.RelayPassed += RelayPassed;
        }

        private void StreamChanged(object sender, StreamEventArgs e)
        {
            if(e.StreamType == Models.Enum.StreamProviderTypes.Mixer)
            {
                NotifySubscriber(e);
            }
        }

        private async void RelayPassed(object sender, StreamEventArgs e)
        {
            NotifySubscriber(e);
        }
        private async void NotifySubscriber(StreamEventArgs e)
        {
            if (e.state == Models.Enum.StreamState.Started)
            {
                while (client.ConnectionState != ConnectionState.Connected)
                {
                    await Task.Delay(5000);
                }
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    Models.Stream stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower() == e.stream.ToLower()).FirstOrDefault();
                    if(stream != null)
                    {
                        List<ulong> blocked = _context.DiscordBans.Select(x => x.DiscordID).ToList();
                        //var test = client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(u => !blocked.Contains(u.Id)).GroupBy(u => u.Username);
                        foreach (var user in client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(u => !blocked.Contains(u.Id)))
                        {
                            ChatUserModel dbUser = null;
                            try
                            {
                                dbUser = _context.ChatUserModels.Include(chatuser => chatuser.StreamSubscriptions).Where(x => x.UserName == user.Username).FirstOrDefault();
                            }
                            catch (Exception)
                            {
                                _context.DiscordBans.Add(new DiscordBan() { DiscordID = user.Id });
                                await _context.SaveChangesAsync();
                                //ignore temporarily
                            }
                            if (dbUser != null && dbUser.IsSubscribed(stream.StreamName))
                            {
                                try
                                {
                                    await user.SendMessageAsync(e.Notification);
                                    Console.WriteLine(dbUser.ChatUserName+ ": " + e.Notification);
                                    await Task.Delay(100);
                                }catch(Discord.Net.HttpException ex)
                                {
                                    switch(ex.DiscordCode)
                                    {
                                        case 50007:
                                            string message = $"Um Stream Nachrichten zu bekommen bitte BobDeathmic als Freund markieren. {Environment.NewLine} Um diese Nachricht zu deaktivieren einfach in das Webinterface (Link über !WebInterfaceLink) von Bob einloggen und in Benutzer > Subscriptions die Streams deaktivieren "+ user.Mention;
                                            //client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault()?.TextChannels.Where(x => x.Name.ToLower() == "botspam").FirstOrDefault()?.SendMessageAsync(message);
                                            break;
                                        default:
                                            Console.WriteLine(ex.ToString());
                                            break;
                                    }

                                }
                                
                            }
                        }
                        //Console.WriteLine("Last Notification");
                    }
                }
            }
        }

        private void PasswordRequestReceived(object sender, PasswordRequestArgs e)
        {
            client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault()?.Users.Where(u => u.Username.ToLower() == e.UserName).FirstOrDefault()?.SendMessageAsync("Dein neues Passwort ist: " + e.TempPassword);
        }

        private void TwitchMessageReceived(object sender, TwitchMessageArgs e)
        {            
            client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault()?.TextChannels.Where(c => c.Name.ToLower() == e.Target.ToLower()).FirstOrDefault()?.SendMessageAsync(e.Message);
        }
        private void InitCommands()
        {
            CommandList = CommandBuilder.BuildCommands("discord");
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            random = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / (DateTime.Now.Hour+1));
            if(await ConnectToDiscord())
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
            if (token != "")
            {
                await client.LoginAsync(TokenType.Bot, token);
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
                var token = "";
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var tokens = _context.SecurityTokens.Where(st => st.service == Models.Enum.TokenType.Discord).FirstOrDefault();
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
            UpdateRelayChannels();
        }

        private async Task ClientJoined(SocketGuildUser arg)
        {
            arg.SendMessageAsync("Um Bot Notifications/Features nutzen zu können müsst ihr euch über den Befehl \"!WebInterfaceLink\" beim Bot registrieren");
        }
        private async Task UpdateRelayChannels()
        {
            List<string> discordChannels = GetDiscordStreamChannels();
            List<string> savedChannels = GetSavedRelayChannels();
                
                
            List<string> channelsToBeAdded = discordChannels.Except(savedChannels).ToList();
            List<string> channelsToBeRemoved = savedChannels.Except(discordChannels).ToList();
            if(channelsToBeAdded.Count() > 0)
            {
                AddChannelsInListToDB(channelsToBeAdded);
            }
            if(channelsToBeRemoved.Count() > 0)
            {
                RemoveChannelsInListFromDB(channelsToBeRemoved);
            }
        }
        private bool ChangeDetected(List<string> List)
        {
            return List.Count() > 0;
        }
        private async Task AddChannelsInListToDB(List<string> channelsToBeAdded)
        {
            ApplicationDbContext _context = null;
            using (var scope = _scopeFactory.CreateScope())
            {
                _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                foreach (var channel in channelsToBeAdded)
                {
                    _context.RelayChannels.Add(new Models.Discord.RelayChannels { Name = channel });
                }
                await _context.SaveChangesAsync();
            }
        }
        private async Task RemoveChannelsInListFromDB(List<string> channelsToBeRemoved)
        {
            ApplicationDbContext _context = null;
            using (var scope = _scopeFactory.CreateScope())
            {
                _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
            foreach (var channel in client.Guilds.Where(g => g.Name.ToLower() == "deathmic").FirstOrDefault().Channels)
            {
                if (Regex.Match(channel.Name.ToLower(), @"stream_").Success)
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
            if(arg.Author.Username != "BobDeathmic")
            {
                string commandresult = "";
                if (arg.Content.StartsWith("!WebInterfaceLink") || arg.Content.StartsWith("!wil"))
                {
                    SendWebInterfaceLink(arg);
                }
                if (CommandList != null)
                {
                    commandresult = await ExecuteCommands(arg);                    
                }
                
                if (commandresult == "")//Commands later on
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
                    GetUserLogin(arg);
                }
                else
                {
                    arg.Author.SendMessageAsync($"Bitte Discord Namen ändern und an einen Admin wenden und folgenden Wert mitteilen {arg.Author.Id}");
                }
            }
        }
        private async Task<string> ExecuteCommands(SocketMessage arg)
        {
            string commandresult = "";
            foreach (IfCommand command in CommandList)
            {
                Dictionary<String, String> inputargs = new Dictionary<string, string>();
                inputargs["message"] = arg.Content;
                inputargs["username"] = arg.Author.Username;
                inputargs["source"] = "discord";
                inputargs["channel"] = arg.Channel.Name;
                commandresult = await command.ExecuteCommandIfApplicable(inputargs);
                if (commandresult != "")
                {
                    arg.Channel.SendMessageAsync(commandresult);
                }
            }
            return commandresult;
        }
        private void RelayMessage(SocketMessage arg)
        {
            if (arg.Channel.Name.StartsWith("stream_"))
            {
                Args.DiscordMessageArgs args = new Args.DiscordMessageArgs();
                args.Source = arg.Channel.Name;
                args.Message = arg.Author.Username + ": " + arg.Content;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var stream = _context.StreamModels.Where(sm => sm.DiscordRelayChannel.ToLower() == arg.Channel.Name.ToLower()).FirstOrDefault();
                    if (stream != null)
                    {
                        args.Target = stream.StreamName;
                        args.StreamType = stream.Type;
                    }
                }
                _eventBus.TriggerEvent(EventType.DiscordMessageReceived, args);
            }
        }
        #region AccountStuff
        private async Task GetUserLogin(SocketMessage arg)
        {
            using (var scope = _scopeFactory.CreateScope())
            {

                var usermanager = scope.ServiceProvider.GetRequiredService<UserManager<ChatUserModel>>();
                string cleanedname = Regex.Replace(arg.Author.Username.Replace(" ", "").Replace("/", "").Replace("\\", ""), @"[\[\]\\\^\$\.\|\?\*\+\(\)\{\}%,;><!@#&\-\+]", "");
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = _context.ChatUserModels.Where(cm => cm.UserName.ToLower() == cleanedname.ToLower()).FirstOrDefault();
                string password = "";
                var Configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                if (user == null)
                {
                    password = await GenerateUser(arg,cleanedname);
                }
                else
                {
                    if(usermanager.CheckPasswordAsync(user, user.InitialPassword).Result)
                    {
                        password = user.InitialPassword;
                    }
                }
                string Message = "Adresse:";
                Message += Configuration.GetValue<string>("WebServerWebAddress");
                if(password != "")
                {
                    Message += Environment.NewLine + "UserName; " + cleanedname;
                    Message += Environment.NewLine + "Initiales Passwort: " + password;
                }
                arg.Author.SendMessageAsync(Message);
                //IConfiguration
                
            }
        }
        private async Task<string> GenerateUser(SocketMessage arg,string cleanedname)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                Models.ChatUserModel newUser = new Models.ChatUserModel();
                string password = "";
                newUser.UserName = cleanedname;
                newUser.ChatUserName = arg.Author.Username;
                password += arg.Author.Username.Substring(random.Next(0, arg.Author.Username.Length - 1));
                newUser.StreamSubscriptions = new List<Models.StreamSubscription>();
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                foreach (var Stream in _context.StreamModels)
                {
                    password += Stream.StreamName.Substring(random.Next(0, Stream.StreamName.Length - 1));
                    Models.StreamSubscription newSubscription = new Models.StreamSubscription();
                    newSubscription.Subscribed = Models.Enum.SubscriptionState.Subscribed;
                    newSubscription.Stream = Stream;
                    newSubscription.User = newUser;
                    newUser.StreamSubscriptions.Add(newSubscription);
                }
                password += random.Next();
                byte[] salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));


                newUser.InitialPassword = hashed;
                var usermanager = scope.ServiceProvider.GetRequiredService<UserManager<ChatUserModel>>();
                var result = await usermanager.CreateAsync(newUser, hashed);
                await CreateOrAddUserRoles("User", newUser.UserName);
                return hashed;
            }
        }
        private async Task CreateOrAddUserRoles(string role, string name)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var UserManager = scope.ServiceProvider.GetRequiredService<UserManager<Models.ChatUserModel>>();

                    IdentityResult roleResult;
                    //Adding Admin Role
                    var roleCheck = await RoleManager.RoleExistsAsync(role);
                    if (!roleCheck)
                    {
                        //create the roles and seed them to the database
                        roleResult = await RoleManager.CreateAsync(new IdentityRole(role));
                    }
                    //Assign Admin role to the main User here we have given our newly registered 
                    //login id for Admin management
                    Models.ChatUserModel user = await UserManager.FindByNameAsync(name);
                    await UserManager.AddToRoleAsync(user, role);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(name);
                Console.WriteLine(role);
            }

        }
        #endregion
    }
}

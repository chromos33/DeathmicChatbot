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
            random = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / DateTime.Now.Hour);
            await ConnectToDiscord();
            InitCommands();
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
                string commandresult = "";
                if (CommandList != null)
                {
                    //Manual WebPageLinkCommand because otherwise would need to redundantly rework Command Structure
                    if(arg.Content.StartsWith("!WebInterfaceLink"))
                    {
                        GetUserLogin(arg);
                    }
                    else
                    {
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
                    }
                    
                }
                
                if (commandresult == "")//Commands later on
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
                Message += Configuration.GetValue<string>("WebAdress");
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
                    password: "test",
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

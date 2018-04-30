using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Timers;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using BobCore.Commands;
using BobCore.StreamFunctions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using BobCore.Administrative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NLog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace BobCore
{
    class Program
    {
        DebugLogger logger;
        #region Globals
        List<dynamic> Commands;
        List<dynamic> StreamCheckers;
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;
        List<DataClasses.Counter> CounterList;
        TrulyObservableCollection<DataClasses.GiveAway> GiveAwayList;
        DiscordSocketClient client;
        // Discord Lib 0.9 //DiscordClient client;
        List<string> FilesToUpdate = new List<string>();
        Random rnd;
        List<DataClasses.SecurityVault> Security = new List<DataClasses.SecurityVault>();
        public static IConfigurationRoot Configuration { get; set; }
        #endregion

#pragma warning disable RECS0154 // Parameter is never used
        public static void Main(string[] args)
#pragma warning restore RECS0154 // Parameter is never used
            => new Program().MainAsync().GetAwaiter().GetResult();

        private void DataLoading()
        {
            Security = Administrative.XMLFileHandler.readFile("SecurityTokens", "DataClasses.SecurityVault");
            StreamList = Administrative.XMLFileHandler.readFile("Streams", "DataClasses.internalStream");
            UserList = Administrative.XMLFileHandler.readFile("Users", "DataClasses.User");
            CounterList = Administrative.XMLFileHandler.readFile("Counters", "DataClasses.Counter");
            if (Administrative.XMLFileHandler.fileexists("GiveAway"))
            {
                GiveAwayList = new TrulyObservableCollection<DataClasses.GiveAway>(Administrative.XMLFileHandler.readFile("GiveAway", "DataClasses.GiveAway"));
            }
            else
            {
                GiveAwayList = new TrulyObservableCollection<DataClasses.GiveAway>();
                Administrative.XMLFileHandler.writeFile(GiveAwayList, "GiveAway");

            }
        }
        private void CommandSetup()
        {
            Commands = new List<dynamic>();
            var results = from type in Assembly.GetAssembly(typeof(Commands.IFCommand)).GetTypes()
                          where typeof(Commands.IFCommand).IsAssignableFrom(type)
                          select type;
            foreach (var type in results)
            {

                if (type.Name != "IFCommand")
                {
                    Type elementType = Type.GetType(type.Name);
                    var newCommand = Activator.CreateInstance(type);
                    Commands.Add(newCommand);
                }
            }
            foreach (var command in Commands)
            {
                string[] reqs = command.SARequirements;
                foreach (string req in reqs)
                {
                    switch (req)
                    {
                        case "Stream":
                            command.addRequiredList(StreamList, "DataClasses.Stream");
                            break;
                        case "User":
                            command.addRequiredList(UserList, "DataClasses.User");
                            break;
                        case "Counter":
                            command.addRequiredList(CounterList, "DataClasses.Counter");
                            break;
                        case "GiveAway":
                            command.addRequiredList(GiveAwayList, "DataClasses.GiveAway");
                            break;
                        case "Client":
                            command.addRequiredList(client, "Client");
                            break;
                        case "SecurityVault":
                            command.addRequiredList(Security, "DataClasses.SecurityVault");
                            break;
                        default: break;
                    }
                }
            }
        }
        private void StreamCheckerSetup()
        {
            StreamCheckers = new List<dynamic>();
            var streamcheckerclasses = from type in Assembly.GetAssembly(typeof(StreamFunctions.StreamChecker)).GetTypes()
                                       where typeof(StreamFunctions.StreamChecker).IsAssignableFrom(type)
                                       select type;
            foreach (var type in streamcheckerclasses)
            {

                if (type.Name != "StreamChecker")
                {
                    Type elementType = Type.GetType(type.Name);
                    var newStreamChecker = Activator.CreateInstance(type);
                    StreamCheckers.Add(newStreamChecker);
                }
            }
            foreach (BobCore.StreamFunctions.StreamChecker dStreamChecker in StreamCheckers)
            {
                dStreamChecker.StreamOnline += StreamOnlineNotification;
                dStreamChecker.StreamOffline += StreamOfflineEvent;
                dStreamChecker.AddSecurityVaultData(Security.FirstOrDefault());
                dStreamChecker.Start(StreamList);
            }
        }
        private async Task DiscordConnection()
        {
            var discordConfig = new DiscordSocketConfig { MessageCacheSize = 100 };
            discordConfig.AlwaysDownloadUsers = true;
            discordConfig.LargeThreshold = 250;
            client = new DiscordSocketClient(discordConfig);
            Console.WriteLine("Bot is trying to login");
            await client.LoginAsync(TokenType.Bot, Security.FirstOrDefault().DiscordToken);
            Console.WriteLine("Bot is trying is logged in");
            await client.StartAsync();
            Console.WriteLine("Bot is trying is starting");
            client.MessageReceived += MessageReceived;
            client.Ready += ClientConnectd;
            client.Disconnected += ClientDisconnected;

            return;
            //await client.LoginAsync(TokenType.Bot, Properties.Settings.Default.DiscordToken);
            // Initialize StreamRelays
        }

        private async Task ClientDisconnected(Exception arg)
        {
            while (FilesToUpdate.Count() != 0)
            {
                Thread.Sleep(1000);
            }
            Environment.Exit(0);
        }

        private void StreamRelaySetup()
        {
            foreach (var stream in StreamList)
            {
                stream.initStreamRelay(client, StreamList);
            }
        }
        private void TestForceStopRelays()
        {
            foreach (var stream in StreamList)
            {
                stream.stopStreamRelay();
            }
        }
        private void GiveAwaySetup()
        {
            GiveAwayList.CollectionChanged += GiveAwayChanged;
            System.Timers.Timer timer = new System.Timers.Timer(5000);
            timer.Elapsed += UpdateFiles;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task MainAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            InitLogger();
            rnd = new Random();
            DataLoading();
            StreamCheckerSetup();
            await DiscordConnection();
            CommandSetup();
            // Initialize StreamCheckers only when not in dev disable for testing
            StreamRelaySetup();
            GiveAwaySetup();


            await Task.Delay(-1);
        }
        public void InitLogger()
        {
            if (!File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/NLog.config"))
            {
                File.Copy("NLog.config", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/NLog.config");
            }
            var servicesProvider = BuildDi();
            logger = servicesProvider.GetRequiredService<DebugLogger>();
        }
        private Task ClientConnectd()
        {
            Console.WriteLine("Bot is connected!");
            return Task.FromResult(true);
        }
        private void SendTooLongMessagesToAuthor(SocketMessage arg,string result)
        {
            List<string> splitresult = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            string message = "";
            foreach (var subresult in splitresult)
            {
                if (message.Length + subresult.Length > 2000)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    arg.Author.SendMessageAsync(message);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    message = "";
                }
                message += subresult + Environment.NewLine;
            }
            if (message.Length > 0)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                arg.Author.SendMessageAsync(message);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
        private void SendTooLongMessagesToChannel(SocketMessage arg,string result)
        {
            List<string> splitresult = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            string message = "";
            foreach (var subresult in splitresult)
            {
                if (message.Length + subresult.Length > 2000)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    arg.Channel.SendMessageAsync(message);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    message = "";
                }
                message += subresult + Environment.NewLine;
            }
            if (message.Length > 0)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                arg.Channel.SendMessageAsync(message);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private Task MessageReceived(SocketMessage arg)
        {
            if (!arg.Author.IsBot)
            {
                bool bCommand = false;
                foreach (var command in Commands)
                {
                    string result = command.CheckCommandAndExecuteIfApplicable(arg.Content, arg.Author.Username, arg.Channel.Name);
                    //If empty was no command
                    if (result != "")
                    {
                        if (!Useful_Functions.isDebug)
                        {
                            if (command.@private)
                            {
                                if (result.Length >= 2000)
                                {
                                    SendTooLongMessagesToAuthor(arg, result);
                                }
                                else
                                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    arg.Author.SendMessageAsync(result);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                }
                            }
                            else
                            {
                                if (result.Length >= 2000)
                                {
                                    SendTooLongMessagesToChannel(arg, result);
                                }
                                else
                                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    arg.Channel.SendMessageAsync(result);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine(result);
                        }
                        bCommand = true;
                    }
                }
                if (!bCommand)
                {
                    RelayMessage(arg);
                }
                KillToReboot(arg);
            }
            else
            {
                if(arg.Content.Length < 200)
                {
                    //logger.DoNotice("I received my own message: in Channel" + arg.Channel + " with message :'" + arg.Content);
                }
            }
            return Task.FromResult(true);
        }
        private void KillToReboot(SocketMessage arg)
        {
            if (arg.Content.Contains("!rebootbob"))
            {
                //Wait for alle files to be written before closing
                while (FilesToUpdate.Count() != 0)
                {
                    Thread.Sleep(1000);
                }
                Environment.Exit(0);
            }
            if(arg.Content.Contains("!killrelay"))
            {
                TestForceStopRelays();
            }
        }
        private void RelayMessage(SocketMessage arg)
        {
            //Look if there is a stream with an active relay and relay message
            var ActiveRelayStreams = StreamList.Where(x => x.ActiveRelay());
            string message = arg.Content.Replace("\n", " ");
            foreach (var stream in ActiveRelayStreams)
            {
                if(!Useful_Functions.isDebug)
                {
                    stream.RelayMessage(arg.Author.Username + ": " + message, arg.Channel.Name);
                }
            }
        }

        private void UpdateFiles(object sender, ElapsedEventArgs e)
        {
            if (FilesToUpdate.Contains("GiveAwayList"))
            {
                BobCore.Administrative.XMLFileHandler.writeFile(GiveAwayList, "GiveAwayList");
                FilesToUpdate.Remove("GiveAwayList");
                Console.WriteLine("GiveAwayListUpdated");
            }
            if (FilesToUpdate.Contains("GiveAway"))
            {
                BobCore.Administrative.XMLFileHandler.writeFile(GiveAwayList, "GiveAway");
                FilesToUpdate.Remove("GiveAway");
                Console.WriteLine("GiveAwayListUpdated");
            }
        }

        private void GiveAwayChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!FilesToUpdate.Contains("GiveAwayList"))
            {
                FilesToUpdate.Add("GiveAwayList");
            }
            if (!FilesToUpdate.Contains("GiveAway"))
            {
                FilesToUpdate.Add("GiveAway");
            }
        }

        private void StreamOfflineEvent(object sender, StreamEventArgs e)
        {

        }

        private void StreamOnlineNotification(object sender, BobCore.StreamFunctions.StreamEventArgs args)
        {
            DataClasses.internalStream stream = StreamList.Where(x => x.sChannel.ToLower() == args.channel.ToLower()).FirstOrDefault();
            if (stream != null)
            {
                if (client.ConnectionState == ConnectionState.Connected)
                {
                    int i = 0;
                    foreach (var user in client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users)
                    {
                        i++;
                        if(i%15 == 0)
                        {
                            Thread.Sleep(2000);
                        }
                        if (args.state == 1)
                        {
                            
                            if (UserList.Where(x => x.isUser(user.Username.ToLower())).FirstOrDefault() != null && UserList.Where(x => x.isUser(user.Username.ToLower())).FirstOrDefault().isSubscribed(stream.sChannel))
                            {
                                if (!Useful_Functions.isDebug)
                                {
                                    //logger.DoNotice("The User " + user.Username + " was notified of Stream " + stream.sChannel + " at " + DateTime.Now.ToString());
                                    user.SendMessageAsync(stream.StreamStartedMessage());
                                    Thread.Sleep(50);
                                }
                                else
                                {
                                    //Console.WriteLine(stream.StreamStartedMessage());
                                }
                            }
                            else
                            {
                                if(UserList.Where(x => x.isUser(user.Username.ToLower())).FirstOrDefault() == null)
                                {
                                    
                                    user.SendMessageAsync(stream.StreamStartedMessage());
                                    //logger.DoNotice("The User " + user.Username + " was notified of Stream " + stream.sChannel + " at " + DateTime.Now.ToString());
                                    user.SendMessageAsync("Bitte mir '!addmyuser' zurückschreiben das ich dich in die Liste eintrage");
                                    Thread.Sleep(50);
                                }
                            }
                        }
                        if (args.state == 2)
                        {
                            if (UserList.Where(x => x.isUser(user.Username.ToLower())).FirstOrDefault() != null && UserList.Where(x => x.isUser(user.Username.ToLower())).FirstOrDefault().isGlobalAnnouncment(stream.sChannel))
                            {
                                if (!Useful_Functions.isDebug)
                                {
                                    user.SendMessageAsync(stream.StreamRunningMessage());
                                }
                                else
                                {
                                    Console.WriteLine(stream.StreamRunningMessage());
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckStreams()
        {

        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        private static IServiceProvider BuildDi()
        {
            var services = new ServiceCollection();

            //Runner is the custom class
            services.AddTransient<DebugLogger>();

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging((builder) => builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace));

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            //configure NLog
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            loggerFactory.ConfigureNLog("NLog.config");

            return serviceProvider;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using IrcDotNet;
using IrcDotNet.Ctcp;
using System.Threading;
using System.Text.RegularExpressions;
using DeathmicChatbot.Properties;
using DeathmicChatbot.StreamInfo;
using DeathmicChatbot.StreamInfo.Twitch;
using DeathmicChatbot.StreamInfo.Hitbox;
using DeathmicChatbot.LinkParser;
using DeathmicChatbot.Interfaces;
using System.Globalization;
using System.Timers;
using DeathmicChatbot.DataFiles;

namespace DeathmicChatbot.IRC
{
    public class BotDeathmicMessageTarget : IrcDotNet.IIrcMessageTarget // Summary:
    //     Represents the target of a message or notice sent by an IRC client.
    {
        // Summary:
        //     Gets the name of the source, as understood by the IRC protocol.
        public string Name { get; set; }
    }


    // Basic Guide

/*  "Object" = Variable name
 *  IRCClient Object.LocalUser = Bot
 *  Message Functions
 *  Object.LocalUser.SendMessage(Target,Message) (SendMessage and SendNotice work the same way)
 *  Target can be everything from Channelname,Nick of User, array of Nicks of Users (DataType String)
 * 
 * 
 * 
 * 
 * 
 * 
 * */
    public class BotDeathmic : BasicIrcBot
    {
        #region global Definitions
        protected bool debug = false;

        private static bool automaticmessages = false;

        private static StreamProviderManager _streamProviderManager;
        public string clientVersionInfo = "IRC.NET Community Bot";
        XMLProvider xmlprovider = new XMLProvider();
        String[] IgnoreTheseUsers = new String[] {"Q","AUTH","Global","py-ctcp","peer",Properties.Settings.Default.Name.ToString()};
        public IrcClient thisclient;
        public CtcpClient ctcpClient1;
        private static readonly Random Rnd = new Random();

        private AutoResetEvent ctcpClientPingResponseReceivedEvent;
        private AutoResetEvent ctcpClientVersionResponseReceivedEvent;
        private AutoResetEvent ctcpClientTimeResponseReceivedEvent;
        private AutoResetEvent ctcpClientActionReceivedEvent;
        private static TimeSpan clientPingTime;
        private static string clientReceivedTimeInfo;
        private static string clientReceivedVersionInfo;
        private static string clientReceivedActionText;
        private static bool isVoteRunning = false;
        public System.Timers.Timer reconnectimer;
        private static URLExtractor urlExtractor = new URLExtractor();
        private bool Disconnected = false;
        private static List<IURLHandler> handlers = new List<IURLHandler>() { new LinkParser.YoutubeHandler(), new LinkParser.Imgur(), new LinkParser.WebsiteHandler() };
        private static System.Timers.Timer VoteTimer;
        public IList<IIrcMessageTarget> targets;
        #endregion
        #region Constructor
        public BotDeathmic()
            : base()
        {

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            try
            {
                if (!Properties.Settings.Default.DateTimeFormatCorrected)
                {
                    xmlprovider.DateTimeCorrection();
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
            
        }
        

        public override IrcRegistrationInfo RegistrationInfo
        {
            get
            {
                return new IrcUserRegistrationInfo()
                {
                    NickName = Properties.Settings.Default.Name,
                    UserName = Properties.Settings.Default.Name,
                    RealName = Properties.Settings.Default.Name
                };
            }
        }
        #endregion
        #region IRCConnectionEvents
        protected override void OnClientConnect(IrcClient client)
        {
          
        }

        protected override void OnClientDisconnect(IrcClient client)
        {
            Disconnected = true;
        }

        protected override void OnClientRegistered(IrcClient client)
        { 
        }

        

        protected override void OnLocalUserJoinedChannel(IrcLocalUser localUser, IrcChannelEventArgs e)
        {
            
            thisclient.FloodPreventer = new IrcStandardFloodPreventer(2,4000);
            if (xmlprovider.runningVotes().Count() > 0)
            {
                VoteTimer = new System.Timers.Timer(5000);
                VoteTimer.Elapsed += OnVoteTimerEvent;
                VoteTimer.Enabled = true;
            }
            //OnClientRegistered may happen before joined channel thus...
            _streamProviderManager = new StreamProviderManager();
            _streamProviderManager.StreamStarted += OnStreamStarted;
            _streamProviderManager.StreamStopped += OnStreamStopped;
            _streamProviderManager.StreamGlobalNotification += OnStreamGlobalNotification;
            _streamProviderManager.AddStreamProvider(new TwitchProvider());
            _streamProviderManager.AddStreamProvider(new HitboxProvider());

            if(reconnectimer != null)
            {
                reconnectimer.Dispose();
            }

            if (!debug)
            {
                reconnectimer = new System.Timers.Timer(150000);
            }
            else
            {
                reconnectimer = new System.Timers.Timer(240000);
            }
            reconnectimer.Elapsed += OnReconnectTimer;
            reconnectimer.Enabled = true;
        }
        #region Reconnect Stuff
        public static bool ReconnectInbound = false;
        private void Reconnect()
        {
            ReconnectInbound = false;
            Environment.Exit(0);

        }
        private void OnReconnectTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!debug)
            {
                if (!ReconnectInbound && !Disconnected)
                {
                    ReconnectInbound = true;
                    thisclient.LocalUser.SendMessage(Properties.Settings.Default.Name, "!reconnect");
                }
                else
                {
                    Reconnect();
                }
            }
            else
            {
                Reconnect();
            }
            
        }
        private void ReconnectDisableRequester(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            ReconnectInbound = false;
        }
        #endregion
        protected override void OnLocalUserLeftChannel(IrcLocalUser localUser, IrcChannelEventArgs e)
        {
            //
        }

        protected override void OnLocalUserNoticeReceived(IrcLocalUser localUser, IrcMessageEventArgs e)
        {
            if (e.Text.Contains("SubscriptionInit"))
            {
                string[] split = e.Text.Split(' ');
                if (split.Count() == 2)
                {
                    xmlprovider.AddAllStreamsToUser(split[1]);
                }
            }
        }

        protected override void OnLocalUserMessageReceived(IrcLocalUser localUser, IrcMessageEventArgs e)
        {
            ReconnectInbound = false;
            if (e.Text.Contains("SubscriptionInit"))
            {
                string[] split = e.Text.Split(' ');
                if (split.Count() == 2)
                {
                    xmlprovider.AddAllStreamsToUser(split[1]);
                }
            }
        }

        protected override void OnChannelUserJoined(IrcChannel channel, IrcChannelUserEventArgs e)
        {
            ReconnectInbound = false;
            if (xmlprovider == null) { xmlprovider = new XMLProvider(); }
            #region whisperstatsonjoin
            string[] userdata = xmlprovider.UserInfo(e.ChannelUser.User.ToString()).Split(',');
            if(userdata.Count() > 0)
            {
                if(userdata.Count() > 1)
                {
                    String days_since_last_visit = DateTime.Now.Subtract(Convert.ToDateTime(userdata[1])).ToString("d' days 'h':'mm':'ss");
                    string visitstring = "";
                    switch (userdata[0])
                    {
                        case "1": visitstring = userdata[0] + "st"; break;
                        case "2": visitstring = userdata[0] + "nd"; break;
                        case "3": visitstring = userdata[0] + "rd"; break;
                        default: visitstring = userdata[0] + "th"; break;
                    }
                    String output = "This is " + e.ChannelUser.User.ToString() + "'s " + visitstring + " visit. Their last visit was on " + userdata[1] + " (" + days_since_last_visit + " ago)";
                    foreach (var loggingOp in xmlprovider.LoggingUser())
                        thisclient.LocalUser.SendNotice(loggingOp, output);
            #endregion

                    foreach (string streamname in xmlprovider.OnlineStreamList())
                    {
                        string _streamname = streamname.Replace(",", "");
                        if(xmlprovider.CheckSuscription(e.ChannelUser.User.ToString().ToLower(),_streamname))
                        {
                            thisclient.LocalUser.SendNotice(e.ChannelUser.User.ToString(), String.Format(
                                                           "Stream running: _{0}_ ({1}) at {2}",
                                                           _streamname,
                                                           xmlprovider.StreamInfo(_streamname, "game"),
                                                           xmlprovider.StreamInfo(_streamname, "URL")
                                                           ));
                        }
                    }                    
                }
                else
                {
                    String output = "This is " + e.ChannelUser.User.ToString() + "'s first Visit.";
                    foreach (var loggingOp in xmlprovider.LoggingUser())
                        thisclient.LocalUser.SendNotice(loggingOp, output);
                }
            }
            else
            {
                String output = "This is " + e.ChannelUser.User.ToString()+"'s first Visit.";
                foreach (var loggingOp in xmlprovider.LoggingUser())
                    thisclient.LocalUser.SendNotice(loggingOp, output);
            }
            xmlprovider.AddorUpdateUser(e.ChannelUser.User.ToString());
        }

        protected override void OnChannelUserLeft(IrcChannel channel, IrcChannelUserEventArgs e)
        {
            ReconnectInbound = false;
        }

        protected override void OnChannelNoticeReceived(IrcChannel channel, IrcMessageEventArgs e)
        {
            ReconnectInbound = false;
            if (e.Text.Contains("SubscriptionInit"))
            {
                string[] split = e.Text.Split(' ');
                if(split.Count() == 2)
                {
                    xmlprovider.AddAllStreamsToUser(split[1]);
                }
                
            }
        }

        protected override void OnChannelMessageReceived(IrcChannel channel, IrcMessageEventArgs e)
        {
            ReconnectInbound = false;
            try
            {
                IEnumerable<string> urls = urlExtractor.extractYoutubeURLs(e.Text);

                if (urls.Count() > 0)
                {
                }

                foreach (var url in urls)
                {
                    foreach (var handler in handlers)
                    {
                        if (handler.handleURL(url, thisclient))
                            break;
                    }
                } 
            } catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }           
        }
        #endregion
        #region commandinit
        protected override void InitializeChatCommandProcessors()
        {
            base.InitializeChatCommandProcessors();
            this.ChatCommandProcessors.Add("addstream", AddStream);
            this.ChatCommandProcessors.Add("delstream", DelStream);
            this.ChatCommandProcessors.Add("streamcheck", StreamCheck);
            this.ChatCommandProcessors.Add("startvoting", StartVoting);
            this.ChatCommandProcessors.Add("endvoting", EndVoting);
            this.ChatCommandProcessors.Add("pickrandomuser", PickRandomUser);
            this.ChatCommandProcessors.Add("userpicklist", UserPickList);
            this.ChatCommandProcessors.Add("removeuserpicklist", RemoveUserPicklist);
            this.ChatCommandProcessors.Add("roll", Roll);
            this.ChatCommandProcessors.Add("counter", CounterCommand);
            this.ChatCommandProcessors.Add("vote", Vote);
            this.ChatCommandProcessors.Add("listvotings", ListVotings);
            this.ChatCommandProcessors.Add("toggleuserloggin", ToggleUserLogging);
            this.ChatCommandProcessors.Add("sendmessage", SendMessage);
            // Don't add this to commandlist only bot should call it (doesn't matter if others call it but...)
            this.ChatCommandProcessors.Add("reconnect",ReconnectDisableRequester);
            this.ChatCommandProcessors.Add("setpass", SetPassword);
            this.ChatCommandProcessors.Add("changesubscription", ChangeSubscription);
            this.ChatCommandProcessors.Add("resetstreamstate",ResetStreamSate);
            //this.ChatCommandProcessors.Add("testfloodprotection", flood);


        }
        private void flood(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            //client.LocalUser.SendNotice
            List<string> Users = new List<string>();
            foreach (var user in thisclient.Channels.First().Users)
            {
                Users.Add(user.User.ToString());
            }
            client.LocalUser.SendMessage(Users, "msg");
        }
        #endregion
        #region generalfunctions
        private string combineParameters(IList<string> parameters)
        {
            string combined = "";
            int stringcount =0;
            foreach(string insert in parameters)
            {
                if(stringcount < parameters.Count())
                {
                    combined += insert + " ";
                }
                else
                {
                    combined += insert;
                }
                
                stringcount++;
            }
            return combined;
        }
        #endregion
        #region Chattcommands
        #region Streamcommands stuff
        private void AddStream(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (parameters.Count > 0)
            {
                _streamProviderManager.AddStream(parameters[0]);
            }
            // TODO continue when streamprovider stuff implemented
            int message = xmlprovider.AddStream(parameters[0]);
            //_streamProviderManager.AddStream(commandArgs);
            if (message == 1)
            {
                client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), String.Format("{0} added {1} to the streamlist", source.Name, parameters[0]));
            }
            else if (message == 2)

            {
                BotDeathmicMessageTarget target = new BotDeathmicMessageTarget();
                target.Name = Properties.Settings.Default.Channel.ToString();

                string textMessage = "slaps " + source.Name + " around for being an idiot.";
                ctcpClient1.SendAction(target, textMessage);
            }
            else if(message == 0)
            {
                client.LocalUser.SendNotice(source.Name, "There has been an error please report to a programmer.");
            }
            
        }
        private void DelStream(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            string message = xmlprovider.RemoveStream(parameters[0]);
            client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), String.Format(message, source.Name, parameters[0]));
        }
        private void ResetStreamSate(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            xmlprovider.ResetStreamState();
        }

        private void StreamCheck(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            foreach (var stream in xmlprovider.OnlineStreamList())
            {
                string[] streamprovidersplit = stream.Split(',');
                //TODO add provider link completion
                bool userfound = false;
                foreach(var user in thisclient.Channels.First().Users)
                {
                    if (user.User.NickName.ToLower() == source.Name.ToLower())
                    {
                        userfound = true;
                    }
                }
      
                if(xmlprovider.SuscribedUsers(stream, thisclient.Channels.First().Users).Contains(source.Name.ToLower()))
                {
                    client.LocalUser.SendNotice(source.Name, streamprovidersplit[0] + " is currently streaming " + xmlprovider.StreamInfo(streamprovidersplit[0], "game") + " at " + xmlprovider.StreamInfo(streamprovidersplit[0], "URL"));
                }
                else
                {
                    client.LocalUser.SendNotice(source.Name, "No Stream is currently running.");
                }
                
            }
            if (xmlprovider.OnlineStreamList().Count() == 0)
            {
                client.LocalUser.SendNotice(source.Name, "No Stream is currently running.");
            }
        }

        private void OnStreamStopped(object sender, StreamEventArgs args)
        {
            if (xmlprovider == null) { xmlprovider = new XMLProvider(); }
            if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "starttime") != "" && Convert.ToBoolean(xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "running")))
            {
                xmlprovider.StreamStartUpdate(args.StreamData.Stream.Channel, true);
                string duration = DateTime.Now.Subtract(Convert.ToDateTime(xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "starttime"))).ToString("h':'mm':'ss");
                string output = "Stream stopped after " + duration + ": " + args.StreamData.Stream.Channel;
                foreach (string user in xmlprovider.SuscribedUsers(args.StreamData.Stream.Channel, thisclient.Channels.First().Users))
                {
                    //TODO change this from single messages/notices to single to multitarget
                    thisclient.LocalUser.SendNotice(user, output);
                }
            }
        }
        private void OnStreamGlobalNotification(object sender, StreamEventArgs args)
        {
            if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "running") == "true")
            {
                if (xmlprovider.GlobalAnnouncementDue(args.StreamData.Stream.Channel))
                {

                    string game = "";
                    System.Diagnostics.Debug.WriteLine(args.StreamData.StreamProvider.GetType().ToString());
                    if (args.StreamData.StreamProvider.GetType().ToString() == "DeathmicChatbot.StreamInfo.Hitbox.HitboxProvider")
                    {
                        game = args.StreamData.Stream.Message;
                    }
                    else
                    {
                        game = xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "game");
                    }
                    xmlprovider.AddStreamLivedata(args.StreamData.Stream.Channel, args.StreamData.StreamProvider.GetLink() + "/" + args.StreamData.Stream.Channel, game);
                    string output = "Stream started: " + args.StreamData.Stream.Channel + "(" + args.StreamData.Stream.Game + ": " + args.StreamData.Stream.Message + ")" + " at " + args.StreamData.StreamProvider.GetLink() + "/" + args.StreamData.Stream.Channel;
                    foreach (string user in xmlprovider.SuscribedUsers(args.StreamData.Stream.Channel, thisclient.Channels.First().Users))
                    {
                        Thread.Sleep(50);
                        thisclient.LocalUser.SendNotice(user, output);
                    }
                }
            }
        }

        private void OnStreamStarted(object sender, StreamEventArgs args)
        {
            if (xmlprovider == null) { xmlprovider = new XMLProvider(); }
            if (xmlprovider.isinStreamList(args.StreamData.Stream.Channel))
            {
                
                
                if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel,"running") == "false")
                {
                    string game = args.StreamData.Stream.Game;

                    if(args.StreamData.Stream.Message != null)
                    {
                        game = args.StreamData.Stream.Message;
                    }
                    xmlprovider.AddStreamLivedata(args.StreamData.Stream.Channel, args.StreamData.StreamProvider.GetLink() + "/" + args.StreamData.Stream.Channel, game);
                    string output = "Stream started: " + args.StreamData.Stream.Channel +"("+ args.StreamData.Stream.Game +": " + args.StreamData.Stream.Message + ")" +" at "+ args.StreamData.StreamProvider.GetLink()+"/"+ args.StreamData.Stream.Channel;
                    foreach (string user in xmlprovider.SuscribedUsers(args.StreamData.Stream.Channel, thisclient.Channels.First().Users))
                    {
                        Thread.Sleep(50);
                        thisclient.LocalUser.SendNotice(user, output);
                    }
                }
                xmlprovider.StreamStartUpdate(args.StreamData.Stream.Channel);
                xmlprovider.GlobalAnnouncementDue(args.StreamData.Stream.Channel);
            }
            
        }
        #endregion
        #region general stuff
        private void Roll(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var regex = new Regex(@"(^\d+)[wWdD](\d+$)");
            if (!regex.IsMatch(parameters[0]))
            {
                client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(),
                                                   String.Format(
                                                       "Error: Invalid roll request: {0}.",
                                                       parameters[0]));
            }
            else
            {
                var match = regex.Match(parameters[0]);

                UInt64 numberOfDice;
                UInt64 sidesOfDice;

                try
                {
                    sidesOfDice = Convert.ToUInt64(match.Groups[2].Value);
                    numberOfDice = Convert.ToUInt64(match.Groups[1].Value);
                }
                catch (OverflowException)
                {
                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(),
                                                       "Error: Result could make the server explode. Get real, you maniac.");
                    return;
                }

                if (numberOfDice == 0 || sidesOfDice == 0)
                {
                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(),
                                                       string.Format(
                                                           "Error: Can't roll 0 dice, or dice with 0 sides."));
                    return;
                }

                if (sidesOfDice >= Int32.MaxValue)
                {
                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(),
                                                       string.Format(
                                                           "Error: Due to submolecular limitations, a die can't have more than {0} sides.",
                                                           Int32.MaxValue - 1));
                    return;
                }

                UInt64 sum = 0;

                var random = new Random();

                var max = numberOfDice * sidesOfDice;
                if (max / numberOfDice != sidesOfDice)
                {
                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(),
                                                       "Error: Result could make the server explode. Get real, you maniac.");
                    return;
                }

                if (numberOfDice > 100000000)
                {
                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(),
                                                       "Seriously? ... I'll try. But don't expect the result too soon. It's gonna take me a while.");
                }

                for (UInt64 i = 0; i < numberOfDice; i++)
                    sum += (ulong)random.Next(1, Convert.ToInt32(sidesOfDice) + 1);

                client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(),
                                                   String.Format("{0}: {1}",
                                                                 parameters[0],
                                                                 sum));
            }
        }
        
        private void SendMessage(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if(Rnd.Next(101) < 60)
            {
                client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), combineParameters(parameters));
            }
            else
            {
                BotDeathmicMessageTarget target = new BotDeathmicMessageTarget();
                target.Name = Properties.Settings.Default.Channel.ToString();

                string textMessage = "slaps " + source.Name + " around for trying to make him say inapropriate stuff.";
                ctcpClient1.SendAction(target, textMessage);
            }
        }
        #endregion
        #region RandomUserPick stuff
        private int CheckPickRandomUserParam(string param)
        {
            if(param.IndexOf("#") >=0)
            {
                return 1;
            }
            if (param.IndexOf("R_") >= 0 || param.IndexOf("r_") >= 0)
            {
                return 2;
            }
            if (param.IndexOf("Ig_") >= 0 || param.IndexOf("ig_") >= 0 || param.IndexOf("IG_") >= 0)
            {
                return 3;
            }
            return 0;
        }
        private void PickRandomUser(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            try
            {
                List<string> unfilteredTargets = new List<string>();
                var sourceUser = (IrcUser)source;
                var replyTargets = GetDefaultReplyTarget(client, sourceUser, targets);
                foreach (var target in client.Users)
                {
                    if (!IgnoreTheseUsers.Contains(target.NickName))
                    {
                        unfilteredTargets.Add(target.NickName);
                    }
                }

                List<string> checkparams = parameters.ToList();     
                List<string> filteredTargets = new List<string>();
                List<string> pickeduseroutput = new List<string>();
                bool multiple = false;
                string multiplevalue = "";
                bool reason = false;
                string reasonvalue = "";
                bool additionalignores = false;
                string additionalignoresvalue = "";
                if(checkparams.Count() > 0)
                {
                    if (checkparams[0].ToString() == "help")
                    {
                        client.LocalUser.SendNotice(source.Name, "The command to for PickRandomUser looks like this: '!PickRandomUser #[Number of Picks] R_[Reason] Ig_[Ignored User 1],[Ignored User 2]... no space'.");
                        client.LocalUser.SendNotice(source.Name, "All parameter (and Order) are optional. [Reason] saves Picks into XML for later use filtering");
                    }
                    else
                    {
                        if (checkparams.Count() == 0 || checkparams.Count() >= 4)
                        {
                            client.LocalUser.SendNotice(source.Name, "The command to for PickRandomUser looks like this: '!PickRandomUser #[Number of Picks] R_[Reason] Ig_[Ignored User 1],[Ignored User 2]... no space'.");
                            client.LocalUser.SendNotice(source.Name, "All parameter (and Order) are optional. [Reason] saves Picks into XML for later use filtering");
                            return;
                        }
                        if (checkparams[checkparams.Count() - 1] == "")
                        {
                            checkparams.RemoveAt(checkparams.Count() - 1);
                        }
                        for (int i = 0; i < checkparams.Count(); i++)
                        {
                            switch (CheckPickRandomUserParam(checkparams[i].ToString()))
                            {
                                case 1:
                                    multiple = true;
                                    multiplevalue = checkparams[i].ToString().Substring(1,checkparams[i].Length-1);
                                    break;
                                case 2:
                                    reason = true;
                                    reasonvalue = checkparams[i].ToString().Substring(2, checkparams[i].Length - 2);
                                    break;
                                case 3:
                                    additionalignores = true;
                                    additionalignoresvalue = checkparams[i].ToString().Substring(3, checkparams[i].Length - 3);
                                    break;
                                default:
                                    client.LocalUser.SendNotice(source.Name, "The command to for PickRandomUser looks like this: '!PickRandomUser #[Number of Picks] R_[Reason] Ig_[Ignored User 1],[Ignored User 2]... no space'.");
                                    client.LocalUser.SendNotice(source.Name, "All parameter (and Order) are optional. [Reason] saves Picks into XML for later use filtering");
                                    return;
                            }
                        }
                        string pickeduser = "";
                        try
                        {
                            string[] tempIgnoreTheseUsers = IgnoreTheseUsers;
                            if (additionalignores)
                            {
                                foreach (string item in additionalignoresvalue.Split(','))
                                {
                                    tempIgnoreTheseUsers = Useful_Functions.String_Array_Push(tempIgnoreTheseUsers, item);
                                }
                            }
                            foreach (var target in unfilteredTargets)
                            {
                                if (!tempIgnoreTheseUsers.Contains(target))
                                {
                                    filteredTargets.Add(target);
                                }
                            }
                            if (multiple && reason)
                            {
                                int tries = int.Parse(multiplevalue);
                                if (int.Parse(multiplevalue) > filteredTargets.Count())
                                {
                                    tries = filteredTargets.Count();
                                }
                                
                                if(filteredTargets.Count() > 0)
                                {
                                    foreach (string user in filteredTargets.ToList())
                                    {
                                        if (xmlprovider.CheckforUserinPick(reasonvalue, user))
                                        {
                                            filteredTargets.Remove(user);
                                        }
                                        if (filteredTargets.Count() == 0)
                                        {
                                            client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "No Users left that have not been chosen yet or that are in the ignore filter");
                                            return;
                                        }
                                    }
                                    for (int i = 0; tries > i; i++)
                                    {
                                        pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                        xmlprovider.CreateUserPick(reasonvalue, pickeduser);
                                        pickeduseroutput.Add(pickeduser);
                                        filteredTargets.Remove(pickeduser);

                                        if (filteredTargets.Count() == 0)
                                        {
                                            client.LocalUser.SendNotice(source.Name, "Not enough Users left in the Channel to completely fulfill your request.");
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "No Users left after Ignorefilter");
                                    return;
                                } 
                            }
                            else if (multiple)
                            {
                                if (filteredTargets.Count() > 0)
                                {
                                    int tries = int.Parse(multiplevalue);
                                    if (int.Parse(multiplevalue) > filteredTargets.Count())
                                    {
                                        tries = filteredTargets.Count();
                                    }
                                    for (int i = 0; tries > i; i++)
                                    {
                                        pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                        pickeduseroutput.Add(pickeduser);
                                        filteredTargets.Remove(pickeduser);
                                        if (filteredTargets.Count() == 0)
                                        {
                                            client.LocalUser.SendNotice(source.Name, "Not enough Users left in the Channel to completely fulfill your request.");
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "No Users left after Ignorefilter");
                                    return;
                                }

                            }
                            else if (reason)
                            {
                                if (filteredTargets.Count() > 0)
                                {
                                    pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                    if ((xmlprovider.CreateUserPick(reasonvalue, pickeduser)))
                                    {
                                        pickeduseroutput.Add(pickeduser);
                                    }
                                }
                                else
                                {
                                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "No Users left after Ignorefilter");
                                    return;
                                }
                            }
                            else
                            {
                                if (filteredTargets.Count() > 0)
                                {
                                    pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                    if ((xmlprovider.CreateUserPick(reasonvalue, pickeduser)))
                                    {
                                        pickeduseroutput.Add(pickeduser);
                                    }
                                }
                                else
                                {
                                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "No Users left after Ignorefilter");
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            client.LocalUser.SendNotice(source.Name, "The command to for PickRandomUser looks like this: '!PickRandomUser #[Number of Picks] R_[Reason] Ig_[Ignored User 1],[Ignored User 2]... no space'.");
                            client.LocalUser.SendNotice(source.Name, "All parameter (and Order) are optional. [Reason] saves Picks into XML for later use filtering");
                            return;
                        }
                    }
                }
                else
                {
                    if (unfilteredTargets.Count() > 0)
                    {
                        bool add = true;
                        foreach (string element in unfilteredTargets)
                        {
                            if(!IgnoreTheseUsers.Contains(element))
                            {
                                filteredTargets.Add(element);
                            }
                        }
                        if (add)
                        {
                            pickeduseroutput.Add(filteredTargets[Rnd.Next(filteredTargets.Count())]);
                        }
                    }
                    else
                    {
                        //This gets triggered even if there should still be stuff
                        client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "No Users left after Ignorefilter");
                        return;
                    }
                }
                string output = "";
                int j = 1;
                foreach(string finalusers in pickeduseroutput)
                {
                    if(j < pickeduseroutput.Count())
                    {
                        output += finalusers + ",";
                    }
                    else
                    {
                        output += finalusers;
                    }
                    j++;
                }
                if(output != "")
                {
                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "The following User/s have been chosen:" + output);
                } else
                {
                    if(checkparams.Count() >0)
                    {
                        if(!(checkparams[0].ToString() == "help"))
                        {
                            client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "There are no more people, that haven't been chosen already for this Reason:" + reasonvalue);
                        }
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                client.LocalUser.SendNotice(source.Name, "#[Number of Picks] must be a number, or some other error occured");
            }
        }
        private void RemoveUserPicklist(IrcClient client, IIrcMessageSource source, System.Collections.Generic.IList<IIrcMessageTarget> targets, string command, System.Collections.Generic.IList<string> parameters)
        {
            if (parameters.Count() >0)
            {
                List<string> checkparams = parameters.ToList();
                if (checkparams.Count() > 1)
                {
                    if (checkparams[checkparams.Count() - 1] == "")
                    {
                        checkparams.RemoveAt(checkparams.Count() - 1);
                    }
                }
                if (checkparams[0] == "help")
                {
                    client.LocalUser.SendNotice(source.Name, "Command is !removeuserpicklist [nameofpickedlist]");
                }
                else
                {

                    if (xmlprovider.DeletePickData(checkparams[0]))
                    {
                        client.LocalUser.SendNotice(source.Name, "Removal succeeded");
                    }
                    else
                    {
                        client.LocalUser.SendNotice(source.Name, "No such List in the Data");
                    }

                }
            }
            else
            {
                client.LocalUser.SendNotice(source.Name, "You must enter a List to be removed");
            }
            
            
            
        }

        private void UserPickList(IrcClient client, IIrcMessageSource source, System.Collections.Generic.IList<IIrcMessageTarget> targets, string command, System.Collections.Generic.IList<string> parameters)
        {
            try
            {
                string output;
                if(parameters.Count() > 0)
                {
                    if (parameters[0] != "")
                    {
                        output = xmlprovider.ReasonUserList(parameters[0]);
                    }
                    else
                    {
                        output = xmlprovider.ReasonUserList();
                    }
                    if (output == "")
                    {
                        client.LocalUser.SendNotice(source.Name, "No Result from this query.");
                    }
                    else
                    {
                        client.LocalUser.SendNotice(source.Name, output);
                    }
                }
                else
                {
                    client.LocalUser.SendNotice(source.Name, "No Result from this query (Create Pick with Reasons first).");
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            } 
        }

       #endregion
        #region Voting Stuff
        #region Voting Commands

        private int CheckVotingParam(string param)
        {
            if (param.IndexOf("t_") >= 0 || param.IndexOf("T_") >= 0)
            {
                return 1;
            }
            if (param.IndexOf("d_") >= 0 || param.IndexOf("D_") >= 0)
            {
                return 2;
            }
            if (param.IndexOf("q_") >= 0 || param.IndexOf("Q_") >= 0)
            {
                return 3;
            }
            if (param.IndexOf("a_") >= 0 || param.IndexOf("A_") >= 0)
            {
                return 4;
            }
            if (param.IndexOf("m_") >= 0 || param.IndexOf("M_") >= 0)
            {
                return 5;
            }
            return 0;
        }
        private void StartVoting(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (parameters.Count ==1)
            {
                if(parameters[0] == "help")
                {
                    client.LocalUser.SendNotice(source.Name, string.Format("The works as follows: !startvote t_[time] | d_[date] | q_[question] | a_[answer1,answer2,...] | m_[multiple answers possible y]"));
                    client.LocalUser.SendNotice(source.Name, string.Format("User Time or Date to specify the end of the Voting"));
                    client.LocalUser.SendNotice(source.Name, string.Format("time like [1d][1h][1m] / date[15.05.2015_22:00]"));
                    client.LocalUser.SendNotice(source.Name, string.Format("ignore multiple option if not wanted"));
                    return;
                }
            }
            string time = ""; string date = ""; string question = "";string answerposibilities ="";string multiple = "";
            
            string combinedParameters = combineParameters(parameters);
            string[] splitParamters = combinedParameters.Split('|');
            if (CheckVotingParam(splitParamters[0]) == 1 && CheckVotingParam(splitParamters[1]) == 2)
            {
                client.LocalUser.SendNotice(source.Name, string.Format("You cannot use time and date in the same voting. use !startvote help for information"));
                return;
            }
            foreach (string param in splitParamters)
            {
                switch (CheckVotingParam(param))
                {
                    case 1: time = param; break;
                    case 2: date = param; break;
                    case 3: question = param; break;
                    case 4: answerposibilities = param; break;
                    case 5: multiple = param; break;
                }
            }
            if((time != "" ||  date != "") && question != "" &&  answerposibilities != "")
            {
                string terminationdate ="";
                if(time != null && time != "")
                {
                    time = time.Replace("t_", "");
                    var timeString = time.Trim();
                    var timeRegex = new Regex(@"^(\d+d)?(\d+h)?(\d+m)?(\d+s)?$");
                    var timeMatch = timeRegex.Match(timeString);
                    if (!timeMatch.Success)
                    {
                        client.LocalUser.SendNotice(source.Name, "Time needs to be in the following format: [<num>d][<num>h][<num>m]");
                        return;
                    }
                    var span = new TimeSpan();
                    TimeSpan tmpSpan;
                    if (TimeSpan.TryParseExact(timeMatch.Groups[1].Value, "d'd'", null, out tmpSpan))
                        span += tmpSpan;
                    if (TimeSpan.TryParseExact(timeMatch.Groups[2].Value, "h'h'", null, out tmpSpan))
                        span += tmpSpan;
                    if (TimeSpan.TryParseExact(timeMatch.Groups[3].Value, "m'm'", null, out tmpSpan))
                        span += tmpSpan;
                    if (TimeSpan.TryParseExact(timeMatch.Groups[4].Value, "s's'", null, out tmpSpan))
                        span += tmpSpan;
                    var endTime = DateTime.Now + span;
                    terminationdate = endTime.ToString();
                }
                if (date != null && date != "")
                {
                    date = date.Replace("_", " ");
                    date = date.Replace("d_", " ");
                    terminationdate = date;
                }
                question = question.Replace("q_", "");
                answerposibilities = answerposibilities.Replace("a_", "");
                string[] answers = answerposibilities.Split(',');
                bool multi = false;
                if(multiple != "")
                {
                    multiple = multiple.Replace("m_", " ");
                    if(multiple.Trim() == "y"){multi = true;}
                    else { multi = false; };
                }
                int questionid = xmlprovider.startVote(DateTime.Parse(terminationdate), multi, answers, question);
                if (questionid != 0)
                {
                    client.LocalUser.SendMessage(Settings.Default.Channel, "The following vote has been started: " + question);
                    client.LocalUser.SendMessage(Settings.Default.Channel, "to vote use the Command !vote " + questionid + " [answer number],[optional answernumber] ");
                    client.LocalUser.SendMessage(Settings.Default.Channel, "Possible answers are:");
                    int i = 1;
                    foreach (var singleanswer in answers)
                    {
                        client.LocalUser.SendMessage(Settings.Default.Channel, i+": "+singleanswer.ToString());
                        i++;
                    }
                    client.LocalUser.SendMessage(Settings.Default.Channel, source.Name+ " has started this Vote");
                    if(VoteTimer.Enabled == false)
                    {
                        VoteTimer.Start();
                        VoteTimer.Enabled = true;
                    }
                }
                
            }
            else
            {
                client.LocalUser.SendNotice(source.Name, string.Format("Please use the help command '!startvote help' for information"));
            }
        }

        private void EndVoting(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (parameters.Count() == 1)
            {
                if(parameters[0] == "help")
                {
                    client.LocalUser.SendMessage(source.Name, "To end a vote use !endvotint [Vote_ID]");
                }
                else
                {
                    try
                    {
                        xmlprovider.EndVote(Int32.Parse(parameters[0]));
                        if (xmlprovider.VoteResult(Int32.Parse(parameters[0]), false).Count() == 0)
                        {
                            client.LocalUser.SendMessage(source.Name, "The vote id entered does not match any Votes in the Database");
                            return;
                        }
                        foreach (var result in xmlprovider.VoteResult(Int32.Parse(parameters[0]),false))
                        {
                            if (result == "There is no Question matching this ID")
                            {
                                client.LocalUser.SendMessage(source.Name, result.ToString());
                            }
                            client.LocalUser.SendMessage(Settings.Default.Channel, result.ToString());
                        }
                    }
                    catch(FormatException)
                    {
                        client.LocalUser.SendMessage(source.Name, "[Vote_ID] must be a number!");
                    }
                }
            }
            
        }
        private void Vote(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (parameters[0] != null)
            {
                if (parameters[0] == "help")
                {
                    client.LocalUser.SendNotice(source.Name, string.Format("The vote command works as follows: !vote [QuestionID] [AnswerID],[AnswerID]..."));
                    client.LocalUser.SendNotice(source.Name, string.Format("If multiple answers aren't allowed in the vote only the first will be used."));
                    return;
                }
            }
            if(parameters.Count() == 2)
            {
                try
                {
                    List<int> answers = new List<int>();
                    if (parameters[1].IndexOf(',') > 0)
                    {
                        foreach (var item in parameters[1].Split(','))
                        {
                            answers.Add(Int32.Parse(item));
                        }
                    }
                    else
                    {
                        answers.Add(Int32.Parse(parameters[1]));
                    }
                    switch(xmlprovider.vote(source.Name, Int32.Parse(parameters[0]), answers))
                    {
                        case 0: client.LocalUser.SendNotice(source.Name, string.Format("You have already voted on this Vote")); break;
                        case 1: client.LocalUser.SendNotice(source.Name, string.Format("Your Vote has been counted.")); break;
                        case 3: client.LocalUser.SendNotice(source.Name, string.Format("The Question ID does not match any Questions")); break;
                        default: client.LocalUser.SendNotice(source.Name, string.Format("There was an error in your vote please report to an admin")); break;
                    }
                }catch(FormatException)
                {
                    client.LocalUser.SendNotice(source.Name, string.Format("[QuestionID] and [AnswerID] must be whole numbers!"));
                }
                
            }
        }


        private void ListVotings(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            List<VoteObject> Votings = xmlprovider.runningVotes();
            if(Votings.Count() > 0)
            {
                thisclient.LocalUser.SendNotice(source.Name, "These are the votes currently running");
                foreach(VoteObject vote in Votings)
                {
                    string output = vote.Question +"("+vote.QuestionID+")"+ " with the answers: " ;
                    foreach(Answer answer in vote.Answers)
                    {
                        output+=answer.value+" ("+answer.id+"),";
                    }
                    thisclient.LocalUser.SendNotice(source.Name, output);
                }
            }
            else
            {
                thisclient.LocalUser.SendNotice(source.Name, "There are no Votes running");
            }
        }
        #endregion
        #region Voting EventListeners
        private void OnVoteTimerEvent(Object source, ElapsedEventArgs e)
        {
            List<int> Votes = xmlprovider.expiredVotes();
            foreach (int vote in Votes)
            {
                xmlprovider.EndVote(vote);
                List<string> Question = xmlprovider.VoteResult(vote,true);
                foreach(string output in Question)
                {
                    thisclient.LocalUser.SendMessage(Settings.Default.Channel, output);
                }
            }
            if (xmlprovider.runningVotes().Count() == 0)
            {
                VoteTimer.Enabled = false;
                VoteTimer.Stop();
            }
        }
      
        #endregion
        #endregion
        #region counter stuff
        private void CounterCommand(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (parameters.Count() < 1)
            {
                client.LocalUser.SendNotice(source.Name, "Error: count needs a counter name. '!counter [countername] [command(read/reset)]' [command] is optional.");
                return;
            }
            if(parameters[0] == "help")
            {
                client.LocalUser.SendNotice(source.Name, "Error: count needs a counter name. '!counter [countername] [command(read/reset)]' [command] is optional.");
                return;
            }
            if(parameters.Count() == 1)
            {
                client.LocalUser.SendMessage(Properties.Settings.Default.Channel, xmlprovider.Counter(parameters[0].ToString()));
            }
            else if(parameters.Count() >= 2)
            {
                if(parameters.Count() >2)
                {
                    client.LocalUser.SendNotice(source.Name, "Warning: Only 2 Parameters needed, ignoring parameters after 2nd.");
                }
                if(parameters[1] == "read")
                {
                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel, xmlprovider.Counter(parameters[0].ToString(), false, true));
                }
                else if(parameters[1] == "reset")
                {
                    client.LocalUser.SendNotice(source.Name, xmlprovider.Counter(parameters[0].ToString(), true));
                }
                else
                {
                    client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "command has to be either read or reset, if you just want to advance the count use '!counter [countername]'.");
                }
            }
        }
        #endregion
        private void ToggleUserLogging(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (xmlprovider == null) { xmlprovider = new XMLProvider(); }
            client.LocalUser.SendNotice(source.Name, xmlprovider.ToggleUserLogging(source.Name));
        }
        
        private void SetPassword(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            bool result = false;
            if (parameters[0] == "help")
            {
                client.LocalUser.SendMessage(source.Name, "Password Command: '!setpass [New Password] [old Pass]' old Pass is optional on the First time");
                return;
            }
            if(parameters.Count == 1)
            {
                result = xmlprovider.AddorUpdatePassword(source.Name, parameters[0]);
            }
            if(parameters.Count == 2)
            {
                result =xmlprovider.AddorUpdatePassword(source.Name, parameters[0],parameters[1]);
            }
            if(result)
            {
                client.LocalUser.SendMessage(source.Name, "new Password Confirmed");
            }
            else
            {
                client.LocalUser.SendMessage(source.Name, "Entered current Password is incorrect or not entered, type !SetPassword help for the command");
            }
        }
        private void ChangeSubscription(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            bool result = false;
            if (parameters[0] == "help")
            {
                client.LocalUser.SendMessage(source.Name, "To update Subscripions use following structure '!changesubscription [add/remove] [streamname] [password]");
                return;
            }
            if (parameters.Count() == 3)
            {
                if(parameters[0] == "remove")
                {
                    result = xmlprovider.AddorUpdateSuscription(source.Name, parameters[1], parameters[2], true);
                }
                if(parameters[0] == "add")
                {
                    result = xmlprovider.AddorUpdateSuscription(source.Name, parameters[1], parameters[2], false);
                }
            }
            if (result)
            {
                client.LocalUser.SendMessage(source.Name, "Subscription Changed");
            }
            else
            {
                client.LocalUser.SendMessage(source.Name, "Either Subscription not found or password is wrong");
            }
        }
        #endregion
        #region CTCP Client Event Handlers

        public void ctcpClient_PingResponseReceived(object sender, CtcpPingResponseReceivedEventArgs e)
        {
            if (e.User.NickName == thisclient.LocalUser.NickName)
                clientPingTime = e.PingTime;

            if (ctcpClientPingResponseReceivedEvent != null)
                ctcpClientPingResponseReceivedEvent.Set();
        }

        public void ctcpClient_VersionResponseReceived(object sender, CtcpVersionResponseReceivedEventArgs e)
        {
            if (e.User.NickName == thisclient.LocalUser.NickName)
                clientReceivedVersionInfo = e.VersionInfo;

            if (ctcpClientVersionResponseReceivedEvent != null)
                ctcpClientVersionResponseReceivedEvent.Set();
        }

        public void ctcpClient_TimeResponseReceived(object sender, CtcpTimeResponseReceivedEventArgs e)
        {
            if (e.User.NickName == thisclient.LocalUser.NickName)
                clientReceivedTimeInfo = e.DateTime;

            if (ctcpClientTimeResponseReceivedEvent != null)
                ctcpClientTimeResponseReceivedEvent.Set();
        }

        public void ctcpClient_ActionReceived(object sender, CtcpMessageEventArgs e)
        {
            if (e.Source.NickName == thisclient.LocalUser.NickName)
                clientReceivedActionText = e.Text;

            if (ctcpClientActionReceivedEvent != null)
                ctcpClientActionReceivedEvent.Set();
        }

        #endregion

    }
}
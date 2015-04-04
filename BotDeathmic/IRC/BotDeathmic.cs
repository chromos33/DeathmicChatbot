using System;
using System.Collections.Generic;
using System.Linq;
using IrcDotNet;
using IrcDotNet.Ctcp;
using System.Threading;
using System.Text.RegularExpressions;
//using DeathmicChatbot.Interfaces;
using DeathmicChatbot.Properties;
//using DeathmicChatbot.StreamInfo.Hitbox;
//using DeathmicChatbot.StreamInfo.Twitch;
//using RestSharp;
using DeathmicChatbot.StreamInfo;
using DeathmicChatbot.StreamInfo.Twitch;
using DeathmicChatbot.StreamInfo.Hitbox;

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
        private static bool automaticmessages = false;

        private static StreamProviderManager _streamProviderManager;
        private static VoteManager _voting;
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
        private static List<string> commandlist = new List<string>();
        public System.Timers.Timer reconnectimer;
        #endregion
        #region Constructor
        public BotDeathmic()
            : base()
        {
            _voting = new VoteManager();
            _voting.VotingStarted += VotingOnVotingStarted;
            _voting.VotingEnded += VotingOnVotingEnded;
            _voting.Voted += VotingOnVoted;
            _voting.VoteRemoved += VotingOnVoteRemoved;        
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
           
        }

        protected override void OnClientRegistered(IrcClient client)
        {
            
        }

        

        protected override void OnLocalUserJoinedChannel(IrcLocalUser localUser, IrcChannelEventArgs e)
        {
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

            reconnectimer = new System.Timers.Timer(5000);
            reconnectimer.Elapsed += OnReconnectTimer;
            reconnectimer.Enabled = true;
        }
        #region Reconnect Stuff
        public static bool ReconnectInbound = false;
        private void Reconnect()
        {
            Console.WriteLine("Reconnect");
            try
            {
                this.Connect(Settings.Default.Server, RegistrationInfo);

                if (Settings.Default.Server.Contains("quakenet"))
                {
                    string quakeservername = null;
                    foreach (var _client in this.Clients)
                    {
                        while (_client.ServerName == null)
                        {

                        }
                        if (_client.ServerName.Contains("quakenet"))
                        {
                            quakeservername = _client.ServerName;
                            this.thisclient = _client;
                            this.ctcpClient1 = new CtcpClient(_client);
                            this.ctcpClient1.ClientVersion = this.clientVersionInfo;
                            this.ctcpClient1.PingResponseReceived += this.ctcpClient_PingResponseReceived;
                            this.ctcpClient1.VersionResponseReceived += this.ctcpClient_VersionResponseReceived;
                            this.ctcpClient1.TimeResponseReceived += this.ctcpClient_TimeResponseReceived;
                            this.ctcpClient1.ActionReceived += this.ctcpClient_ActionReceived;
                        }

                    }
                    var quakeclient = this.GetClientFromServerNameMask(quakeservername);
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.Channel + " " + quakeservername);
                    quakeclient.Channels.Join(Properties.Settings.Default.Channel);
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void OnReconnectTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!ReconnectInbound)
            {
                ReconnectInbound = true;
                thisclient.LocalUser.SendMessage(Properties.Settings.Default.Name, "!reconnect");
            }
            else
            {
                Reconnect();
            }
        }
        private void ReconnectDisableRequester(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            Console.WriteLine("ReconnectDisableRequester");
            ReconnectInbound = false;
        }
        #endregion
        protected override void OnLocalUserLeftChannel(IrcLocalUser localUser, IrcChannelEventArgs e)
        {
            //
        }

        protected override void OnLocalUserNoticeReceived(IrcLocalUser localUser, IrcMessageEventArgs e)
        {
            //
        }

        protected override void OnLocalUserMessageReceived(IrcLocalUser localUser, IrcMessageEventArgs e)
        {
            //
        }

        protected override void OnChannelUserJoined(IrcChannel channel, IrcChannelUserEventArgs e)
        {
            #region whisperstatsonjoin
            string[] userdata = xmlprovider.UserInfo(e.ChannelUser.User.ToString()).Split(',');
            System.Diagnostics.Debug.WriteLine(userdata[1]);
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
            xmlprovider.AddorUpdateUser(e.ChannelUser.User.ToString());


            foreach (var msg in _streamProviderManager.GetStreamInfoArray())
                thisclient.LocalUser.SendNotice(e.ChannelUser.User.ToString(), msg);
            if (xmlprovider == null) { xmlprovider = new XMLProvider(); }
        }

        protected override void OnChannelUserLeft(IrcChannel channel, IrcChannelUserEventArgs e)
        {
            //thisclient.LocalUser.SendMessage(channel.Name, "bla");
        }

        protected override void OnChannelNoticeReceived(IrcChannel channel, IrcMessageEventArgs e)
        {
            //
        }

        protected override void OnChannelMessageReceived(IrcChannel channel, IrcMessageEventArgs e)
        {

        }
        #endregion
        #region commandinit
        protected override void InitializeChatCommandProcessors()
        {
            base.InitializeChatCommandProcessors();
            commandlist = new List<string>();
            this.ChatCommandProcessors.Add("addstream", AddStream);
            commandlist.Add("addstream");
            this.ChatCommandProcessors.Add("delstream", DelStream);
            commandlist.Add("delstream");
            this.ChatCommandProcessors.Add("streamcheck", StreamCheck);
            commandlist.Add("streamcheck");
            this.ChatCommandProcessors.Add("startvoting", StartVoting);
            commandlist.Add("startvoting");
            this.ChatCommandProcessors.Add("endvoting", EndVoting);
            commandlist.Add("endvoting");
            this.ChatCommandProcessors.Add("pickrandomuser", PickRandomUser);
            commandlist.Add("pickrandomuser");
            this.ChatCommandProcessors.Add("userpicklist", UserPickList);
            commandlist.Add("userpicklist");
            this.ChatCommandProcessors.Add("removeuserpicklist", RemoveUserPicklist);
            commandlist.Add("removeuserpicklist");
            this.ChatCommandProcessors.Add("roll", Roll);
            commandlist.Add("roll");
            this.ChatCommandProcessors.Add("counter", CounterCommand);
            commandlist.Add("counter");
            this.ChatCommandProcessors.Add("vote", Vote);
            commandlist.Add("vote");
            this.ChatCommandProcessors.Add("removevote", RemoveVote);
            commandlist.Add("removevote");
            this.ChatCommandProcessors.Add("listvotings", ListVotings);
            commandlist.Add("listvotings");
            this.ChatCommandProcessors.Add("toggleuserloggin", ToggleUserLogging);
            commandlist.Add("toggleuserloggin");
            this.ChatCommandProcessors.Add("sendmessage", SendMessage);
            commandlist.Add("sendmessage");
            // Don't add this to commandlist only bot should call it (doesn't matter if others call it but...)
            this.ChatCommandProcessors.Add("reconnect",ReconnectDisableRequester);

            this.ChatCommandProcessors.Add("listcommands",ListCommands);
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
            string message = xmlprovider.AddStream(parameters[0], source.Name);
            //_streamProviderManager.AddStream(commandArgs);
            if (message == (source.Name + " added Stream to the streamlist"))
            {
                client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), String.Format("{0} added {1} to the streamlist", source.Name, parameters[0]));
            }
            else if (message == (source.Name + " wanted to readd Stream to the streamlist."))
            {
                BotDeathmicMessageTarget target = new BotDeathmicMessageTarget();
                target.Name = Properties.Settings.Default.Channel.ToString();

                string textMessage = "slaps " + source.Name + " around for being an idiot.";
                ctcpClient1.SendAction(target, textMessage);
            }
            
        }
        private void DelStream(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            string message = xmlprovider.RemoveStream(parameters[0]);
            client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), String.Format(message, source.Name, parameters[0]));
        }

        private void StreamCheck(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            foreach (var stream in xmlprovider.OnlineStreamList())
            {
                string[] streamprovidersplit = stream.Split(',');
                //TODO add provider link completion
                client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), streamprovidersplit[0] + "is currently streaming at " + streamprovidersplit[1]);
            }
            if (xmlprovider.OnlineStreamList().Count() == 0)
            {
                client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), "No Stream is currently running.");
            }
        }

        private void OnStreamStopped(object sender, StreamEventArgs args)
        {
            if (xmlprovider == null) { xmlprovider = new XMLProvider(); }
            if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "starttime") != "" && Convert.ToBoolean(xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "running")))
            {
                xmlprovider.StreamStartUpdate(args.StreamData.Stream.Channel, true);
                string duration = DateTime.Now.Subtract(Convert.ToDateTime(xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "starttime"))).ToString("h':'mm':'ss");
                Console.WriteLine("{0}: Stream stopped: {1}",
                                  DateTime.Now,
                                  args.StreamData.Stream.Channel);
                thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel,String.Format(
                                                       "Stream stopped after {1}: {0}",
                                                       args.StreamData.Stream
                                                           .Channel,
                                                       duration));
            }


        }
        private void OnStreamGlobalNotification(object sender, StreamEventArgs args)
        {
            if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "running") == "true")
            {
                if(xmlprovider.GlobalAnnouncementDue(args.StreamData.Stream.Channel))
                {
                    Console.WriteLine("{0}: Stream running: {1}",
                          DateTime.Now,
                          args.StreamData.Stream.Channel);
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

                    thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, String.Format(
                                                           "Stream running: {0} ({1}) at {2}/{0}",
                                                           args.StreamData.Stream
                                                               .Channel,
                                                           game,
                                                           args.StreamData
                                                               .StreamProvider.GetLink()));
                }
            }
        }

        private void OnStreamStarted(object sender, StreamEventArgs args)
        {
            if (xmlprovider == null) { xmlprovider = new XMLProvider(); }
            Console.WriteLine(args.StreamData.StreamProvider.ToString());
            if (xmlprovider.isinStreamList(args.StreamData.Stream.Channel))
            {
                if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel,"running") == "false")
                {
                    Console.WriteLine("{0}: Stream started: {1}",
                              DateTime.Now,
                              args.StreamData.Stream.Channel);
                    
                    thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, String.Format(
                                                           "Stream started: {0} ({1}: {2}) at {3}/{0}",
                                                           args.StreamData.Stream
                                                               .Channel,
                                                           args.StreamData.Stream.Game,
                                                           args.StreamData.Stream
                                                               .Message,
                                                           args.StreamData
                                                               .StreamProvider.GetLink()));
                }
                
            }
            xmlprovider.GlobalAnnouncementDue(args.StreamData.Stream.Channel);
            xmlprovider.StreamStartUpdate(args.StreamData.Stream.Channel);
        }
        #endregion
        #region general stuff
        private void ListCommands(IrcClient client, IIrcMessageSource source, System.Collections.Generic.IList<IIrcMessageTarget> targets, string command, System.Collections.Generic.IList<string> parameters)
        {
            client.LocalUser.SendNotice(source.Name, combineParameters(commandlist));
        }
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
            client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), combineParameters(parameters));
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
            // TODO: Test this Shit
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
                                for (int i = 0; int.Parse(multiplevalue) > i; i++)
                                {
                                    int randcounter = 0;
                                    if(filteredTargets.Count() > 0)
                                    {
                                        do
                                        {
                                            randcounter++;
                                            pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                            if (xmlprovider.CheckforUserinPick(reasonvalue, pickeduser) == false)
                                            {
                                                xmlprovider.CreateUserPick(reasonvalue, pickeduser);
                                                pickeduseroutput.Add(pickeduser);
                                                break;
                                            }
                                            if (randcounter == 20)
                                            {
                                                break;
                                            }
                                        } while (xmlprovider.CheckforUserinPick(reasonvalue, pickeduser) == false);
                                    }
                                    else
                                    {
                                        client.LocalUser.SendMessage(Properties.Settings.Default.Channel, "No Users left after Ignorefilter");
                                        return;
                                    } 
                                }
                            }
                            else if (multiple)
                            {
                                if (filteredTargets.Count() > 0)
                                {
                                    for (int i = 0; int.Parse(multiplevalue) > i; i++)
                                    {
                                        int randcounter = 0;
                                        do
                                        {
                                            randcounter++;
                                            pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                            if (!pickeduseroutput.Contains(pickeduser))
                                            {
                                                pickeduseroutput.Add(pickeduser);
                                                break;
                                            }
                                            if (randcounter == 20)
                                            {
                                                break;
                                            }
                                        } while (!pickeduseroutput.Contains(pickeduser));

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
            if(parameters.Count() >0)
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
                if (parameters.Count() > 0 && parameters[0] != "")
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
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            
        }

       #endregion
        #region Voting Stuff
        #region Voting Commands
        private void StartVoting(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {

            string args = combineParameters(parameters);
            string[] singleparams = args.Split('|');
            if (singleparams.Count() < 3)
            {
                client.LocalUser.SendNotice(source.Name, string.Format("Please use the following format: !startvote <time> | <question> | <answer1,answer2,...>"));
            }
            var timeString = singleparams[0];
            var timeRegex = new Regex(@"^(\d+d)?(\d+h)?(\d+m)?(\d+s)?$");
            var timeMatch = timeRegex.Match(timeString);
            if (!timeMatch.Success)
            {
                client.LocalUser.SendNotice(source.Name, "Time needs to be in the following format: [<num>d][<num>h][<num>m][<num>s]");
                client.LocalUser.SendNotice(source.Name, "Examples: 10m30s or n5h or n1d or n1d6h");
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
            var question = singleparams[1];
            var answers = new List<string>(singleparams[2].Split(','));
            var endTime = DateTime.Now + span;
            try
            {
                _voting.StartVoting(source.Name, question, answers, endTime);
                //_log.WriteToLog("Information", String.Format("{0} started a voting: {1}. End Date is: {2}",source.Name,question,endTime));
            }
            catch (InvalidOperationException e)
            {

                client.LocalUser.SendNotice(source.Name, e.Message);
                //_log.WriteToLog("Error",String.Format("{0} tried starting a voting: {1}. But: {2}",source.Name,question,e.Message));
            }
        }

        private void EndVoting(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            int index;
            if (parameters.Count() > 0)
            {
                if (!int.TryParse(parameters[0], out index))
                {
                    client.LocalUser.SendNotice(source.Name, string.Format("The format for ending a vote is: !endvote <id>"));
                    return;
                }
                try
                {
                    _voting.EndVoting(source.Name, index - 1);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    client.LocalUser.SendNotice(source.Name, "There is no voting with the id " + index);
                }
                catch (InvalidOperationException e)
                {
                    client.LocalUser.SendNotice(source.Name, e.Message);
                }
            }
            else
            {
                client.LocalUser.SendNotice(source.Name, string.Format("The format for ending a vote is: !endvote <id>"));
            }

        }
        private void Vote(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (!(parameters.Count() == 0))
            {
                if (parameters.Count() < 2)
                {
                    client.LocalUser.SendNotice(source.Name, string.Format("Format: /msg {0} vote <id> <answer>", source.Name));
                    client.LocalUser.SendNotice(source.Name, string.Format("You can check the running votings with /msg {0} listvotings", source.Name));
                    return;
                }
                int index;
                var answer = parameters[1];
                if (!int.TryParse(parameters[0], out index))
                {
                    client.LocalUser.SendNotice(source.Name, "id must be a number");
                    return;
                }
                try
                {
                    _voting.Vote(source.Name, index - 1, answer);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    switch (e.ParamName)
                    {
                        case "id":
                            client.LocalUser.SendNotice(source.Name, string.Format("There is no voting with the id {0}", index));
                            break;
                        case "answer":
                            client.LocalUser.SendNotice(source.Name, string.Format("The voting {0} has no answer {1}", index, answer));
                            break;
                        default:
                            throw;
                    }
                }
            }
        }

        private void RemoveVote(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            int index;
            if (parameters == null || !int.TryParse(parameters[0], out index))
            {
                client.LocalUser.SendNotice(source.Name, string.Format("The format for removintg your vote is: /msg {0} removevote <id>", source.Name));
                return;
            }
            try
            {
                _voting.RemoveVote(source.Name, index - 1);
            }
            catch (ArgumentOutOfRangeException e)
            {
                if (e.ParamName == "id")
                {
                    client.LocalUser.SendNotice(source.Name, string.Format("There is no voting with the id {0}", index));
                }
                else

                    throw;
            }
        }

        private void ListVotings(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (_voting.Votings.Count == 0)
            {
                client.LocalUser.SendNotice(source.Name, "There are currently no votings running");
            }
            foreach (var voting in _voting.Votings.Values)
            {
                client.LocalUser.SendNotice(source.Name, string.Format("{0} - {1}", voting._iIndex + 1, voting._sQuestion));
                client.LocalUser.SendNotice(source.Name, "Answers:");
                foreach (var answer in voting._slAnswers)
                    client.LocalUser.SendNotice(source.Name, string.Format("    {0}", answer));
                client.LocalUser.SendNotice(source.Name, string.Format("Voting runs until {0}", voting._dtEndTime));
            }
        }
        #endregion
        #region Voting EventListeners
        private static void CheckAllVotingsThreaded()
        {
            //Checks if any votings running to stop loop to perserve memry and cpu
            while (_voting.anyVotings())
            {
                _voting.CheckVotings();
                Thread.Sleep(Settings.Default.StreamcheckIntervalSeconds * 1000);
            }
        }
        private void VotingOnVoteRemoved(object sender, VotingEventArgs args)
        {
            thisclient.LocalUser.SendNotice(args.User, String.Format("Your vote for '{0}' has been removed.", args.Voting._sQuestion));
        }

        private void VotingOnVoted(object sender, VotingEventArgs args)
        {
            thisclient.LocalUser.SendNotice(args.User, String.Format("Your vote for '{0}' has been counted.", args.Voting._sQuestion));
        }

        private void VotingOnVotingEnded(object sender, VotingEventArgs args)
        {
            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, String.Format("The voting '{0}' has ended with the following results:", args.Voting._sQuestion));
            var votes = new Dictionary<string, int>();
            foreach (var answer in args.Voting._slAnswers)
                votes[answer] = 0;
            foreach (var answer in args.Voting._votes.Values)
                ++votes[answer];
            args.Voting._votes.Clear();
            foreach (var vote in votes)
            {
                thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, String.Format("    {0}: {1} votes", vote.Key, vote.Value));
            }
        }

        private void VotingOnVotingStarted(object sender, VotingEventArgs args)
        {
            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, String.Format("{0} started a voting which runs until {1}.", args.User,args.Voting._dtEndTime));
            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, args.Voting._sQuestion +" Possible answers:");

            foreach (var answer in args.Voting._slAnswers)
                thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, string.Format("    {0}", answer));

            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, String.Format("Vote with /msg " + thisclient.LocalUser + " !vote {1} <answer>", args.User, args.Voting._iIndex + 1));
            isVoteRunning = true;
            CheckAllVotingsThreaded();
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

using System;
using System.Collections.Generic;
using System.Linq;
using IrcDotNet;
using DeathmicChatbot.Exceptions;
using IrcDotNet.Ctcp;
using System.Threading;
using System.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
//using DeathmicChatbot.Interfaces;
using DeathmicChatbot.Properties;
//using DeathmicChatbot.StreamInfo.Hitbox;
//using DeathmicChatbot.StreamInfo.Twitch;
//using RestSharp;

namespace DeathmicChatbot.IRC
{
    public class BotDeathmicMessageTarget : IrcDotNet.IIrcMessageTarget // Summary:
    //     Represents the target of a message or notice sent by an IRC client.
    {
        // Summary:
        //     Gets the name of the source, as understood by the IRC protocol.
        public string Name { get; set; }
    }
    public class BotDeathmic : BasicIrcBot
    {
        private static VoteManager _voting;
        public string clientVersionInfo = "IRC.NET Community Bot";
        XMLProvider xmlprovider = new XMLProvider();
        String[] IgnoreTheseUsers = new String[] {"AUTH","Global","py-ctcp","peer"};
        public IrcClient thisclient;
        public CtcpClient ctcpClient1;

        private AutoResetEvent ctcpClientPingResponseReceivedEvent;
        private AutoResetEvent ctcpClientVersionResponseReceivedEvent;
        private AutoResetEvent ctcpClientTimeResponseReceivedEvent;
        private AutoResetEvent ctcpClientActionReceivedEvent;
        private static TimeSpan clientPingTime;
        private static string clientReceivedTimeInfo;
        private static string clientReceivedVersionInfo;
        private static string clientReceivedActionText;

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

        protected override void OnClientConnect(IrcClient client)
        {

        }

        protected override void OnClientDisconnect(IrcClient client)
        {
            //
        }

        protected override void OnClientRegistered(IrcClient client)
        {
            //
        }

        protected override void OnLocalUserJoinedChannel(IrcLocalUser localUser, IrcChannelEventArgs e)
        {
        }

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
        }

        protected override void OnChannelUserLeft(IrcChannel channel, IrcChannelUserEventArgs e)
        {
            //
        }

        protected override void OnChannelNoticeReceived(IrcChannel channel, IrcMessageEventArgs e)
        {
            //
        }

        protected override void OnChannelMessageReceived(IrcChannel channel, IrcMessageEventArgs e)
        {

        }

        protected override void InitializeChatCommandProcessors()
        {
            base.InitializeChatCommandProcessors();

            this.ChatCommandProcessors.Add("test", Test);
            this.ChatCommandProcessors.Add("addstream", AddStream);
            this.ChatCommandProcessors.Add("delstream", DelStream);
            this.ChatCommandProcessors.Add("streamcheck", StreamCheck);
            this.ChatCommandProcessors.Add("starvoting", StartVoting);
            this.ChatCommandProcessors.Add("endvoting", EndVoting);
            this.ChatCommandProcessors.Add("pickrandomuser", PickRandomUser);
            this.ChatCommandProcessors.Add("roll", Roll);
            this.ChatCommandProcessors.Add("countercount", CounterCount);
            this.ChatCommandProcessors.Add("counterreset", CounterReset);
            this.ChatCommandProcessors.Add("counterstats", CounterStats);
            this.ChatCommandProcessors.Add("vote", Vote);
            this.ChatCommandProcessors.Add("removevote", RemoveVote);
            this.ChatCommandProcessors.Add("listvotings", ListVotings);
            this.ChatCommandProcessors.Add("toggleuserloggin", ToggleUserLogging);
            this.ChatCommandProcessors.Add("sendmessage", SendMessage);

            

        }
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
        #region Chattcommands
        private void SendMessage(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), combineParameters(parameters));
        }

        private void AddStream(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
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

                string textMessage = "slaps " + source.Name +" around for being an idiot.";
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
            foreach(var stream in xmlprovider.OnlineStreamList())
            {
                string[] streamprovidersplit = stream.Split(',');
                //TODO add provider link completion
                client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), streamprovidersplit[0] +"is currently streaming at "+streamprovidersplit[1]);
            }
        }

        private void StartVoting(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (parameters.Count() < 3)
            {
                client.LocalUser.SendNotice(source.Name,string.Format("Please use the following format: !startvote <time> <question> <answer1,answer2,...>"));
                return;
            }
            var timeString = parameters[0];
            var timeRegex = new Regex(@"^(\d+d)?(\d+h)?(\d+m)?(\d+s)?$");
            var timeMatch = timeRegex.Match(timeString);
            if (!timeMatch.Success)
            {
                client.LocalUser.SendNotice(source.Name, "Time needs to be in the following format: [<num>d][<num>h][<num>m][<num>s]");
                client.LocalUser.SendNotice(source.Name, "Examples: 10m30s\n5h\n1d\n1d6h");
                return;
            }
            var span = new TimeSpan();
            TimeSpan tmpSpan;
            if (TimeSpan.TryParseExact(timeMatch.Groups[1].Value,
            "d'd'",
            null,
            out tmpSpan))
                span += tmpSpan;
            if (TimeSpan.TryParseExact(timeMatch.Groups[2].Value,
            "h'h'",
            null,
            out tmpSpan))
                span += tmpSpan;
            if (TimeSpan.TryParseExact(timeMatch.Groups[3].Value,
            "m'm'",
            null,
            out tmpSpan))
                span += tmpSpan;
            if (TimeSpan.TryParseExact(timeMatch.Groups[4].Value,
            "s's'",
            null,
            out tmpSpan))
                span += tmpSpan;
            var question = parameters[1];
            var answers = new List<string>(parameters[2].Split(','));
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
            throw new NotImplementedException();
        }

        private void PickRandomUser(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            throw new NotImplementedException();
        }

        private void Roll(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            throw new NotImplementedException();
        }

        private void CounterCount(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            throw new NotImplementedException();
        }

        private void CounterReset(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            throw new NotImplementedException();
        }

        private void CounterStats(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            throw new NotImplementedException();
        }

        private void Vote(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            throw new NotImplementedException();
        }

        private void RemoveVote(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            throw new NotImplementedException();
        }

        private void ListVotings(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            throw new NotImplementedException();
        }

        private void ToggleUserLogging(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            throw new NotImplementedException();
        }
        private void Test(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var sourceUser = (IrcUser)source;

            if (parameters.Count != 0)
                throw new InvalidCommandParametersException(1);

            // List all currently logged-in twitter users.
            var replyTargets = GetDefaultReplyTarget(client, sourceUser, targets);

            string test = "";
            foreach (var target in client.Users)
            {
                
                if (!IgnoreTheseUsers.Contains(target.NickName))
                {
                    test += target.NickName + ", ";
                }
            }
            client.LocalUser.SendMessage(targets, "These Users are currently in this channel: " + test);
            
            
            //client.LocalUser.SendMessage(replyTargets., replyTargets.Count().ToString(), Encoding.UTF8);
            

        }
        #endregion
        #region CTCP Client 1 Event Handlers

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
        #region Vote Events
        private void VotingOnVoteRemoved(object sender, VotingEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void VotingOnVoted(object sender, VotingEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void VotingOnVotingEnded(object sender, VotingEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void VotingOnVotingStarted(object sender, VotingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
        #endregion  

    }
}

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
            this.ChatCommandProcessors.Add("startvoting", StartVoting);
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
            if(xmlprovider.OnlineStreamList().Count() == 0)
            {
                client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(),"No Stream is currently running.");
            }
        }

        private void StartVoting(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {

            string args = combineParameters(parameters);
            string[] singleparams = args.Split('|');
            if (singleparams.Count() < 3)
            {
                client.LocalUser.SendNotice(source.Name,string.Format("Please use the following format: !startvote <time> | <question> | <answer1,answer2,...>"));
            }
            var timeString = singleparams[0];
            var timeRegex = new Regex(@"^(\d+d)?(\d+h)?(\d+m)?(\d+s)?$");
            Console.WriteLine(timeString.Trim());
            Console.WriteLine(timeRegex);
            var timeMatch = timeRegex.Match(timeString);
            if (!timeMatch.Success)
            {
                client.LocalUser.SendNotice(source.Name, "Time needs to be in the following format: [<num>d][<num>h][<num>m][<num>s]");
                client.LocalUser.SendNotice(source.Name, "Examples: 10m30s or n5h or n1d or n1d6h");
            }
            var span = new TimeSpan();
            TimeSpan tmpSpan;
            if (TimeSpan.TryParseExact(timeMatch.Groups[1].Value,"d'd'",null,out tmpSpan))
                span += tmpSpan;
            if (TimeSpan.TryParseExact(timeMatch.Groups[2].Value,"h'h'",null,out tmpSpan))
                span += tmpSpan;
            if (TimeSpan.TryParseExact(timeMatch.Groups[3].Value,"m'm'",null,out tmpSpan))
                span += tmpSpan;
            if (TimeSpan.TryParseExact(timeMatch.Groups[4].Value,"s's'",null,out tmpSpan))
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
                    Console.WriteLine(index);
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
            if(!(parameters.Count() == 0))
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
                    client.LocalUser.SendNotice(source.Name,"id must be a number");
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
                            client.LocalUser.SendNotice(source.Name,string.Format("The voting {0} has no answer {1}",index,answer));
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
            Console.WriteLine("test");
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
            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, String.Format("{0} started a voting.", args.User));
            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, args.Voting._sQuestion);
            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, "Possible answers:");
            Console.WriteLine(args.Voting._dtEndTime);

            foreach (var answer in args.Voting._slAnswers)
                thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, string.Format("    {0}", answer));

            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, String.Format("Vote with /msg "+thisclient.LocalUser+" !vote {1} <answer>", args.User, args.Voting._iIndex + 1));
            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, string.Format("Voting runs until {0}", args.Voting._dtEndTime));
        }
        #endregion  

    }
}

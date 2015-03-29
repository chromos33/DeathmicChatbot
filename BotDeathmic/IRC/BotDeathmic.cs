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
using DeathmicChatbot.Properties;


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
        #region Streamcommands stuff
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
        #endregion
        #region general stuff
        private void SendMessage(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            client.LocalUser.SendMessage(Properties.Settings.Default.Channel.ToString(), combineParameters(parameters));
        }
        private void PickRandomUser(IrcClient client, IIrcMessageSource source, IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            // TODO: Test this Shit
            if(parameters[0].ToString() == "help")
            {
                client.LocalUser.SendNotice(source.Name, "The command to for PickRandomUser looks like this: '!PickRandomUser [Reason] [Number of Picks] | [Ignored User 1],[Ignored User 2]...'.");
                client.LocalUser.SendNotice(source.Name, "All parameter are optional [Reason] is used fore multi picks with filtering previous picked Users.");
            }else
            {
                // PickUpUser for Occasion
                bool choosemultiple;
                bool hasreason;
                int numberofrolls = 0;
                string reason = "";
                string[] splitcombinedparameters;
                string[] firstparametershalf;
                string[] secondparametershalf;
                string combinedparameters = combineParameters(parameters);
                if(combinedparameters.IndexOf('|') >=0)
                {
                    choosemultiple = false;
                    hasreason = false;
                    splitcombinedparameters = combinedparameters.Split('|');
                    firstparametershalf = splitcombinedparameters[0].Trim().Split(' ');
                    secondparametershalf = splitcombinedparameters[1].Trim().Split(' ');
                    if(firstparametershalf.Count() == 1)
                    {
                        
                        choosemultiple = true;
                        if(!int.TryParse(firstparametershalf[0],out numberofrolls))
                        {
                            client.LocalUser.SendNotice(source.Name, "Please enter a number for 'Number of Rolls', and not a word,special sign or whatever.");
                        }
                    }
                    else
                    {
                        if(firstparametershalf.Count() == 2)
                        {
                            hasreason = true;
                            reason = firstparametershalf[1];
                        }
                        else{
                            client.LocalUser.SendNotice(source.Name, "The command to for PickRandomUser looks like this: '!PickRandomUser [Number of Picks] [Reason] | [Ignored User 1],[Ignored User 2]...'.");
                            client.LocalUser.SendNotice(source.Name, "All parameter are semi optional you have to take every parameter up to the one you want.");
                            client.LocalUser.SendNotice(source.Name, "[Reason] is used for picks with filtering previous picked Users.");
                            
                            return;
                        }
                    }
                    List<string> filteredTargets = new List<string>();

                    foreach (var target in client.Users)
                    {
                        if (!IgnoreTheseUsers.Contains(target.NickName) || !secondparametershalf.Contains(target.NickName))
                        {
                            filteredTargets.Add(target.NickName);
                        }
                    }
                    if(choosemultiple)
                    {
                        String result = "";
                        if(hasreason)
                        {
                            for (int i = 0; i < numberofrolls; i++)
                            {
                                if (xmlprovider.CheckforUserinPick(reason, filteredTargets[Rnd.Next(filteredTargets.Count() - 1)]))
                                {
                                    if(i == numberofrolls-1)
                                    {
                                        result += filteredTargets[Rnd.Next(filteredTargets.Count() - 1)] + ",";
                                    }
                                    else
                                    {
                                        result += filteredTargets[Rnd.Next(filteredTargets.Count() - 1)];
                                    }

                                    
                                }
                            }
                        }
                        else
                        {
                            for(int i = 0; i < numberofrolls;i++)
                            {
                                if (i == numberofrolls - 1)
                                {
                                    result += filteredTargets[Rnd.Next(filteredTargets.Count() - 1)] + ",";
                                }
                                else
                                {
                                    result += filteredTargets[Rnd.Next(filteredTargets.Count() - 1)];
                                }
                            }
                        }
                        client.LocalUser.SendMessage(Properties.Settings.Default.Channel, result);
                        
                    }
                    else
                    {
                        client.LocalUser.SendMessage(Properties.Settings.Default.Channel, filteredTargets[Rnd.Next(filteredTargets.Count() - 1)]);
                    }
                    

                }
                else
                {
                    List<string> filteredTargets = new List<string>();

                    foreach (var target in client.Users)
                    {
                        if (!IgnoreTheseUsers.Contains(target.NickName))
                        {
                            filteredTargets.Add(target.NickName);
                        }
                    }
                }
                

                
            }
            /*
            var index = Rnd.Next(nameList.Count);
            var chosen = nameList[index];
            _messageQueue.PublicMessageEnqueue(channel, chosen);
            ChosenUsers.TryAdd(chosen, chosen);
            SaveChosenUsers();*/
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
            Console.WriteLine(args.Voting._dtEndTime);

            foreach (var answer in args.Voting._slAnswers)
                thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, string.Format("    {0}", answer));

            thisclient.LocalUser.SendMessage(Properties.Settings.Default.Channel, String.Format("Vote with /msg " + thisclient.LocalUser + " !vote {1} <answer>", args.User, args.Voting._iIndex + 1));
            isVoteRunning = true;
            CheckAllVotingsThreaded();
        }
        #endregion
        #endregion



        

        

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

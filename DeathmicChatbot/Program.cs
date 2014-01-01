#region Using

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
using DeathmicChatbot.Interfaces;
using DeathmicChatbot.Properties;
using DeathmicChatbot.StreamInfo.Hitbox;
using DeathmicChatbot.StreamInfo.Twitch;
using RestSharp;
using Sharkbite.Irc;

#endregion


namespace DeathmicChatbot
{
    internal static class Program
    {
        private const string CHOSEN_USERS_FILE = "chosenusers.txt";
        private const int USER_UPDATE_INTERVAL = 60;
        private static ConnectionArgs _cona;
        private static Connection _con;
        private static YotubeManager _youtube;
        private static LogManager _log;
        private static WebsiteManager _website;
        private static StreamProviderManager _streamProviderManager;
        private static CommandManager _commands;
        private static VoteManager _voting;
        private static readonly String Channel = Settings.Default.Channel;
        private static readonly String Nick = Settings.Default.Name;
        private static readonly String Server = Settings.Default.Server;
        private static readonly String Logfile = Settings.Default.Logfile;
        private static bool _restarted;
        private static readonly Random Rnd = new Random();
        private static MessageQueue _messageQueue;
        private static readonly ICounter Counter = new Counter();
        private static IModel _model;

        private static readonly ConcurrentDictionary<string, string> ChosenUsers
            = new ConcurrentDictionary<string, string>();

        private static readonly ConcurrentDictionary<string, string>
            CurrentUsers = new ConcurrentDictionary<string, string>();

        private static bool _debugMode;

        private static void Main(string[] args)
        {
            _debugMode = args.Length > 0 && args.Contains("debug");

            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;

            LoadChosenUsers();
            Connect();

            _model = new Model(new SqliteDatabaseProvider());
        }

        private static void Connect()
        {
            _cona = new ConnectionArgs(Nick, Server);
            _con = new Connection(Encoding.UTF8, _cona, false, false);
            _con.Listener.OnRegistered += OnRegistered;
            _con.Listener.OnPublic += OnPublic;
            _con.Listener.OnPrivate += OnPrivate;
            _con.Listener.OnJoin += OnJoin;
            _con.Listener.OnPart += OnPart;
            _con.Listener.OnNames += OnNames;
            _con.Listener.OnNick += OnNick;
            _con.Listener.OnDisconnected += OnDisconnect;
            while (!IsConnectionPossible(_cona))
                Console.WriteLine("OFFLINE");
            _con.Connect();
            _messageQueue = new MessageQueue(_con);
        }

        private static bool IsConnectionPossible(ConnectionArgs cona)
        {
            try
            {
                var s = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);
                s.Connect(cona.Hostname, cona.Port);
                s.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static void OnDisconnect()
        {
            while (!IsConnectionPossible(_cona))
                Console.WriteLine("OFFLINE");
            if (!_restarted)
                Connect();
            _restarted = true;
        }

        private static void AddStream(UserInfo user,
                                      string channel,
                                      string text,
                                      string commandArgs)
        {
            if (_streamProviderManager.AddStream(commandArgs))
            {
                _log.WriteToLog("Information",
                                String.Format(
                                    "{0} added {1} to the streamlist",
                                    user.Nick,
                                    commandArgs));
                _messageQueue.PublicMessageEnqueue(channel,
                                                   String.Format(
                                                       "{0} added {1} to the streamlist",
                                                       user.Nick,
                                                       commandArgs));
            }
            else
            {
                _log.WriteToLog("Information",
                                String.Format(
                                    "{0} wanted to readd {1} to the streamlist",
                                    user.Nick,
                                    commandArgs));
                _con.Sender.Action(channel,
                                   String.Format(
                                       "slaps {0} around for being an idiot",
                                       user.Nick));
            }
        }

        private static void DelStream(UserInfo user,
                                      string channel,
                                      string text,
                                      string commandArgs)
        {
            _log.WriteToLog("Information",
                            String.Format(
                                "{0} removed {1} from the streamlist",
                                user.Nick,
                                commandArgs));
            _messageQueue.PublicMessageEnqueue(channel,
                                               String.Format(
                                                   "{0} removed {1} from the streamlist",
                                                   user.Nick,
                                                   commandArgs));
            _streamProviderManager.RemoveStream(commandArgs);
        }

        private static void StreamCheck(UserInfo user,
                                        string channel,
                                        string text,
                                        string commandArgs)
        {
            if (!_streamProviderManager.GetStreamInfoArray().Any())
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   "There are currently no streams running :(");
                return;
            }
            foreach (var stream in _streamProviderManager.GetStreamInfoArray())
                _messageQueue.PrivateNoticeEnqueue(user.Nick, stream);
        }

        private static void OnStreamStopped(object sender, StreamEventArgs args)
        {
            Console.WriteLine("{0}: Stream stopped: {1}",
                              DateTime.Now,
                              args.StreamData.Stream.Channel);
            _messageQueue.PublicMessageEnqueue(Channel,
                                               String.Format(
                                                   "Stream stopped after {1}: {0}",
                                                   args.StreamData.Stream
                                                       .Channel,
                                                   args.StreamData
                                                       .TimeSinceStart.ToString(
                                                           "h':'mm':'ss")));
        }

        private static void OnStreamStarted(object sender, StreamEventArgs args)
        {
            Console.WriteLine("{0}: Stream started: {1}",
                              DateTime.Now,
                              args.StreamData.Stream.Channel);
            _messageQueue.PublicMessageEnqueue(Channel,
                                               String.Format(
                                                   "Stream started: {0} ({1}: {2}) at {3}/{0}",
                                                   args.StreamData.Stream
                                                       .Channel,
                                                   args.StreamData.Stream.Game,
                                                   args.StreamData.Stream
                                                       .Message,
                                                   args.StreamData
                                                       .StreamProvider.GetLink()));
        }

        private static void VotingOnVotingStarted(object sender,
                                                  VotingEventArgs args)
        {
            _messageQueue.PublicMessageEnqueue(Channel,
                                               String.Format(
                                                   "{0} started a voting.",
                                                   args.User.Nick));
            _messageQueue.PublicMessageEnqueue(Channel, args.Voting._sQuestion);
            _messageQueue.PublicMessageEnqueue(Channel, "Possible answers:");
            foreach (var answer in args.Voting._slAnswers)
                _messageQueue.PublicMessageEnqueue(Channel,
                                                   string.Format("    {0}",
                                                                 answer));
            _messageQueue.PublicMessageEnqueue(Channel,
                                               String.Format(
                                                   "Vote with /msg {0} vote {1} <answer>",
                                                   Nick,
                                                   args.Voting._iIndex + 1));
            _messageQueue.PublicMessageEnqueue(Channel,
                                               string.Format(
                                                   "Voting runs until {0}",
                                                   args.Voting._dtEndTime));
        }

        private static void VotingOnVotingEnded(object sender,
                                                VotingEventArgs args)
        {
            _messageQueue.PublicMessageEnqueue(Channel,
                                               String.Format(
                                                   "The voting '{0}' has ended with the following results:",
                                                   args.Voting._sQuestion));
            var votes = new Dictionary<string, int>();
            foreach (var answer in args.Voting._slAnswers)
                votes[answer] = 0;
            foreach (var answer in args.Voting._votes.Values)
                ++votes[answer];
            args.Voting._votes.Clear();
            foreach (var vote in votes)
            {
                _messageQueue.PublicMessageEnqueue(Channel,
                                                   String.Format(
                                                       "    {0}: {1} votes",
                                                       vote.Key,
                                                       vote.Value));
            }
        }

        private static void VotingOnVoted(object sender, VotingEventArgs args)
        {
            _messageQueue.PrivateNoticeEnqueue(args.User.Nick,
                                               String.Format(
                                                   "Your vote for '{0}' has been counted.",
                                                   args.Voting._sQuestion));
        }

        private static void VotingOnVoteRemoved(object sender,
                                                VotingEventArgs args)
        {
            _messageQueue.PrivateNoticeEnqueue(args.User.Nick,
                                               String.Format(
                                                   "Your vote for '{0}' has been removed.",
                                                   args.Voting._sQuestion));
        }

        private static void StartVoting(UserInfo user,
                                        string channel,
                                        string text,
                                        string commandArgs)
        {
            var args = commandArgs != null
                           ? commandArgs.Split('|')
                           : new string[0];
            if (args.Length < 3)
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   string.Format(
                                                       "Please use the following format: {0}startvote <time>|<question>|<answer1,answer2,...>",
                                                       CommandManager.ACTIVATOR));
                return;
            }
            var timeString = args[0];
            var timeRegex = new Regex(@"^(\d+d)?(\d+h)?(\d+m)?(\d+s)?$");
            var timeMatch = timeRegex.Match(timeString);
            if (!timeMatch.Success)
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   "Time needs to be in the following format: [<num>d][<num>h][<num>m][<num>s]");
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   "Examples: 10m30s\n5h\n1d\n1d6h");
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

            var question = args[1];
            var answers = new List<string>(args[2].Split(','));

            var endTime = DateTime.Now + span;
            try
            {
                _voting.StartVoting(user, question, answers, endTime);
                _log.WriteToLog("Information",
                                String.Format(
                                    "{0} started a voting: {1}. End Date is: {2}",
                                    user.Nick,
                                    question,
                                    endTime));
            }
            catch (InvalidOperationException e)
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick, e.Message);
                _log.WriteToLog("Error",
                                String.Format(
                                    "{0} tried starting a voting: {1}. But: {2}",
                                    user.Nick,
                                    question,
                                    e.Message));
            }
        }

        private static void EndVoting(UserInfo user,
                                      string channel,
                                      string text,
                                      string commandArgs)
        {
            int index;
            if (commandArgs == null || !int.TryParse(commandArgs, out index))
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   string.Format(
                                                       "The format for ending a vote is: {0}endvote <id>",
                                                       CommandManager.ACTIVATOR));
                return;
            }
            try
            {
                _voting.EndVoting(user, index - 1);
            }
            catch (ArgumentOutOfRangeException e)
            {
                if (e.ParamName == "id")
                {
                    _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                       string.Format(
                                                           "There is no voting with the id {0}",
                                                           index));
                }
                else

                    throw;
            }
            catch (InvalidOperationException e)
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick, e.Message);
            }
        }

        private static void Vote(UserInfo user, string text, string commandArgs)
        {
            var args = commandArgs != null
                           ? commandArgs.Split(' ')
                           : new string[0];
            if (args.Length < 2)
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   string.Format(
                                                       "Format: /msg {0} vote <id> <answer>",
                                                       Nick));
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   string.Format(
                                                       "You can check the running votings with /msg {0} listvotings",
                                                       Nick));
                return;
            }
            int index;
            var answer = args[1];
            if (!int.TryParse(args[0], out index))
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   "id must be a number");
                return;
            }
            try
            {
                _voting.Vote(user, index - 1, answer);
            }
            catch (ArgumentOutOfRangeException e)
            {
                switch (e.ParamName)
                {
                    case "id":
                        _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                           string.Format(
                                                               "There is no voting with the id {0}",
                                                               index));
                        break;
                    case "answer":
                        _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                           string.Format(
                                                               "The voting {0} has no answer {1}",
                                                               index,
                                                               answer));
                        break;
                    default:
                        throw;
                }
            }
        }

        private static void RemoveVote(UserInfo user,
                                       string text,
                                       string commandArgs)
        {
            int index;
            if (commandArgs == null || !int.TryParse(commandArgs, out index))
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   string.Format(
                                                       "The format for removintg your vote is: /msg {0} removevote <id>",
                                                       Nick));
                return;
            }
            try
            {
                _voting.RemoveVote(user, index - 1);
            }
            catch (ArgumentOutOfRangeException e)
            {
                if (e.ParamName == "id")
                {
                    _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                       string.Format(
                                                           "There is no voting with the id {0}",
                                                           index));
                }
                else

                    throw;
            }
        }

        private static void ListVotings(UserInfo user,
                                        string text,
                                        string commandArgs)
        {
            if (_voting.Votings.Count == 0)
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   "There are currently no votings running");
            }
            foreach (var voting in _voting.Votings.Values)
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   string.Format("{0} - {1}",
                                                                 voting._iIndex +
                                                                 1,
                                                                 voting
                                                                     ._sQuestion));
                _messageQueue.PrivateNoticeEnqueue(user.Nick, "Answers:");
                foreach (var answer in voting._slAnswers)
                    _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                       string.Format("    {0}",
                                                                     answer));
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   string.Format(
                                                       "Voting runs until {0}",
                                                       voting._dtEndTime));
            }
        }

        private static void CheckAllVotingsThreaded()
        {
            while (true)
            {
                _voting.CheckVotings();
                Thread.Sleep(Settings.Default.StreamcheckIntervalSeconds * 1000);
            }
        }

        private static void OnNames(string channel, string[] nicks, bool last)
        {
            foreach (var nick in nicks.Where(nick => nick.Trim() != ""))
                CurrentUsers.TryAdd(nick, nick);
        }

        private static void PickRandomUser(UserInfo user,
                                           string channel,
                                           string text,
                                           string commandArgs)
        {
            var nameList = new List<string>(CurrentUsers.Keys);
            RemoveIgnoredUsers(ref nameList);
            if (nameList.Count == 0)
            {
                nameList = new List<string>(CurrentUsers.Keys);
                ChosenUsers.Clear();
                RemoveIgnoredUsers(ref nameList);
            }
            var index = Rnd.Next(nameList.Count);
            var chosen = nameList[index];
            _messageQueue.PublicMessageEnqueue(channel, chosen);
            ChosenUsers.TryAdd(chosen, chosen);
            SaveChosenUsers();
        }

        private static void RemoveIgnoredUsers(ref List<string> nameList)
        {
            var ignores = new List<string>(ChosenUsers.Keys);
            ignores.AddRange(Settings.Default.pickIgnores.Split(';'));
            foreach (var nick in
                new List<string>(nameList).Where(
                    nick => ignores.Contains(nick) || nick == Nick))
                nameList.Remove(nick);
        }

        private static void OnError(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = ((Exception) e.ExceptionObject);
            var st = new StackTrace(ex, true);
            _log.WriteToLog("Error", ex.Message, st);
        }

        private static void OnJoin(UserInfo user, string channel)
        {
            foreach (var msg in _streamProviderManager.GetStreamInfoArray())
                _messageQueue.PrivateNoticeEnqueue(user.Nick, msg);
            CurrentUsers.TryAdd(user.Nick, user.Nick);
            JoinLogger.LogJoin(user.Nick, _messageQueue);
        }

        private static void OnPart(UserInfo user, string channel, string reason)
        {
            string tmpout;
            CurrentUsers.TryRemove(user.Nick, out tmpout);
        }

        private static void OnNick(UserInfo user, string newnick)
        {
            string tmpout;
            if (ChosenUsers.ContainsKey(user.Nick))
            {
                ChosenUsers.TryRemove(user.Nick, out tmpout);
                ChosenUsers.TryAdd(newnick, newnick);
            }
            CurrentUsers.TryRemove(user.Nick, out tmpout);
            CurrentUsers.TryAdd(newnick, newnick);
        }

        private static void LoadChosenUsers()
        {
            if (!File.Exists(CHOSEN_USERS_FILE))
                File.Create(CHOSEN_USERS_FILE).Close();
            var reader = new StreamReader(CHOSEN_USERS_FILE);
            while (!reader.EndOfStream)
            {
                var nick = reader.ReadLine();
                if (nick != null)
                    ChosenUsers.TryAdd(nick, nick);
            }
            reader.Close();
        }

        private static void SaveChosenUsers()
        {
            var writer = new StreamWriter(CHOSEN_USERS_FILE, false);
            foreach (var user in ChosenUsers.Keys)
                writer.WriteLine(user);
            writer.Close();
        }

        private static void UpdateUsers() { _con.Sender.Names(Channel); }

        private static void SaveChosenUsersThreaded()
        {
            while (true)
            {
                SaveChosenUsers();
                Thread.Sleep(USER_UPDATE_INTERVAL * 1000);
            }
        }

        private static void SendMessage(UserInfo user,
                                        string text,
                                        string commandArgs) { _messageQueue.PublicMessageEnqueue(Channel, commandArgs); }

        private static void Roll(UserInfo user,
                                 string channel,
                                 string text,
                                 string commandArgs)
        {
            var regex = new Regex(@"(^\d+)[wWdD](\d+$)");
            if (!regex.IsMatch(commandArgs))
            {
                _messageQueue.PublicMessageEnqueue(channel,
                                                   String.Format(
                                                       "Error: Invalid roll request: {0}.",
                                                       commandArgs));
            }
            else
            {
                var match = regex.Match(commandArgs);

                UInt64 numberOfDice;
                UInt64 sidesOfDice;

                try
                {
                    sidesOfDice = Convert.ToUInt64(match.Groups[2].Value);
                    numberOfDice = Convert.ToUInt64(match.Groups[1].Value);
                }
                catch (OverflowException)
                {
                    _messageQueue.PublicMessageEnqueue(channel,
                                                       "Error: Result could make the server explode. Get real, you maniac.");
                    return;
                }

                if (numberOfDice == 0 || sidesOfDice == 0)
                {
                    _messageQueue.PublicMessageEnqueue(channel,
                                                       string.Format(
                                                           "Error: Can't roll 0 dice, or dice with 0 sides."));
                    return;
                }

                if (sidesOfDice > Int32.MaxValue)
                {
                    _messageQueue.PublicMessageEnqueue(channel,
                                                       string.Format(
                                                           "Error: Due to submolecular limitations, a die can't have more than {0} sides.",
                                                           Int32.MaxValue));
                    return;
                }

                UInt64 sum = 0;

                var random = new Random();

                var max = numberOfDice * sidesOfDice;
                if (max / numberOfDice != sidesOfDice)
                {
                    _messageQueue.PublicMessageEnqueue(channel,
                                                       "Error: Result could make the server explode. Get real, you maniac.");
                    return;
                }

                if (numberOfDice > 100000000)
                {
                    _messageQueue.PublicMessageEnqueue(channel,
                                                       "Seriously? ... I'll try. But don't expect the result too soon. It's gonna take me a while.");
                }

                for (UInt64 i = 0; i < numberOfDice; i++)
                    sum += (ulong) random.Next(1, Convert.ToInt32(sidesOfDice));

                _messageQueue.PublicMessageEnqueue(channel,
                                                   String.Format("{0}: {1}",
                                                                 commandArgs,
                                                                 sum));
            }
        }

        private static void OnRegistered()
        {
            CurrentUsers.Clear();
            _con.Sender.Join(Channel);
            UpdateUsers();
            _restarted = false;
            _log = new LogManager(Logfile);
            AppDomain.CurrentDomain.UnhandledException += OnError;
            _youtube = new YotubeManager();
            _website = new WebsiteManager(_log);
            _streamProviderManager = new StreamProviderManager();
            _streamProviderManager.AddStreamProvider(new TwitchProvider(_log,
                                                                        _debugMode));
            _streamProviderManager.AddStreamProvider(
                new HitboxProvider(
                    new RestClientProvider(new RestClient("http://api.hitbox.tv")),
                    new LogManagerProvider(_log),
                    new TextFile(HitboxProvider.STREAMS_FILE),
                    _debugMode));
            _voting = new VoteManager();
            _streamProviderManager.StreamStarted += OnStreamStarted;
            _streamProviderManager.StreamStopped += OnStreamStopped;
            _voting.VotingStarted += VotingOnVotingStarted;
            _voting.VotingEnded += VotingOnVotingEnded;
            _voting.Voted += VotingOnVoted;
            _voting.VoteRemoved += VotingOnVoteRemoved;
            _commands = new CommandManager();
            CommandManager.PublicCommand addstream = AddStream;
            CommandManager.PublicCommand delstream = DelStream;
            CommandManager.PublicCommand streamcheck = StreamCheck;
            CommandManager.PublicCommand startvote = StartVoting;
            CommandManager.PublicCommand endvote = EndVoting;
            CommandManager.PublicCommand pickuser = PickRandomUser;
            CommandManager.PublicCommand roll = Roll;
            CommandManager.PublicCommand count = CounterCount;
            CommandManager.PublicCommand counterReset = CounterReset;
            CommandManager.PublicCommand counterStats = CounterStats;
            CommandManager.PrivateCommand vote = Vote;
            CommandManager.PrivateCommand removevote = RemoveVote;
            CommandManager.PrivateCommand listvotings = ListVotings;
            CommandManager.PrivateCommand sendmessage = SendMessage;
            CommandManager.PrivateCommand mergeusers = MergeUsers;

            _commands.SetCommand("addstream", addstream);
            _commands.SetCommand("streamadd", addstream);
            _commands.SetCommand("delstream", delstream);
            _commands.SetCommand("streamdel", delstream);
            _commands.SetCommand("streamwegschreinen", delstream);
            _commands.SetCommand("streamcheck", streamcheck);
            _commands.SetCommand("checkstream", streamcheck);
            _commands.SetCommand("startvote", startvote);
            _commands.SetCommand("endvote", endvote);
            _commands.SetCommand("stopvote", endvote);
            _commands.SetCommand("votestop", endvote);
            _commands.SetCommand("vote", vote);
            _commands.SetCommand("listvotings", listvotings);
            _commands.SetCommand("removevote", removevote);
            _commands.SetCommand("pickuser", pickuser);
            _commands.SetCommand("say", sendmessage);
            _commands.SetCommand("roll", roll);
            _commands.SetCommand("mergeusers", mergeusers);
            _commands.SetCommand("count", count);
            _commands.SetCommand("counterReset", counterReset);
            _commands.SetCommand("counterStats", counterStats);

            Counter.CountRequested += CounterOnCountRequested;
            Counter.StatRequested += CounterOnStatRequested;
            Counter.ResetRequested += CounterOnResetRequested;

            var votingCheckThread = new Thread(CheckAllVotingsThreaded);
            var saveChosenUsersThread = new Thread(SaveChosenUsersThreaded);

            votingCheckThread.Start();
            saveChosenUsersThread.Start();
        }

        private static void CounterOnResetRequested(object sender,
                                                    CounterEventArgs
                                                        counterEventArgs) { _messageQueue.PublicMessageEnqueue(Channel, counterEventArgs.Message); }

        private static void CounterOnStatRequested(object sender,
                                                   CounterEventArgs
                                                       counterEventArgs) { _messageQueue.PublicMessageEnqueue(Channel, counterEventArgs.Message); }

        private static void CounterOnCountRequested(object sender,
                                                    CounterEventArgs
                                                        counterEventArgs) { _messageQueue.PublicMessageEnqueue(Channel, counterEventArgs.Message); }

        private static void CounterStats(UserInfo user,
                                         string channel,
                                         string text,
                                         string commandargs)
        {
            var split = commandargs.Split(new[] {' '});
            if (split.Length < 1)
            {
                _messageQueue.PublicMessageEnqueue(channel,
                                                   "Error: counterStats needs a counter name. '!counterStats <name>'");
                return;
            }

            var sName = split[0];

            Counter.CounterStats(sName);
        }

        private static void CounterReset(UserInfo user,
                                         string channel,
                                         string text,
                                         string commandargs)
        {
            var split = commandargs.Split(new[] {' '});
            if (split.Length < 1)
            {
                _messageQueue.PublicMessageEnqueue(channel,
                                                   "Error: counterReset needs a counter name. '!counterReset <name>'");
                return;
            }

            var sName = split[0];

            Counter.CounterReset(sName);
        }

        private static void CounterCount(UserInfo user,
                                         string channel,
                                         string text,
                                         string commandargs)
        {
            var split = commandargs.Split(new[] {' '});
            if (split.Length < 1)
            {
                _messageQueue.PublicMessageEnqueue(channel,
                                                   "Error: count needs a counter name. '!count <name>'");
                return;
            }

            var sName = split[0];

            Counter.Count(sName);
        }

        private static void MergeUsers(UserInfo user,
                                       string text,
                                       string commandargs)
        {
            var split = commandargs.Split(new[] {' '});

            if (split.Length < 2)
            {
                _messageQueue.PrivateNoticeEnqueue(user.Nick,
                                                   "MergeUsers: Incorrect usage (no 2 arguments detected).");
                return;
            }

            var userToMergeAway = split[0];
            var userToMergeInto = split[1];

            UserMerger.MergeUsers(_messageQueue,
                                  user.Nick,
                                  userToMergeAway,
                                  userToMergeInto);
        }

        private static void OnPublic(UserInfo user,
                                     string channel,
                                     string message)
        {
            if (_commands.CheckCommand(user, channel, message))
                return;

            var link = _youtube.IsYtLink(message);

            if (link != null)
            {
                var vid = _youtube.GetVideoInfo(link);
                _messageQueue.PublicMessageEnqueue(channel,
                                                   YotubeManager.GetInfoString(
                                                       vid));
                return;
            }

            var urls = _website.ContainsLinks(message);

            foreach (var title in
                urls.Select(url => _website.GetPageTitle(url).Trim())
                    .Where(title => !string.IsNullOrEmpty(title)))
                _messageQueue.PublicMessageEnqueue(channel, title);
        }

        private static void OnPrivate(UserInfo user, string message) { _commands.CheckCommand(user, Channel, message, true); }
    }
}
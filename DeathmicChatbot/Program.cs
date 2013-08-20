using System;
using System.Linq;
using System.Text;
using System.Threading;
using DeathmicChatbot.Properties;
using Sharkbite.Irc;
using Google.YouTube;
using System.Diagnostics;
using System.Collections.Generic;
using DeathmicChatbot.StreamInfo;
using System.Text.RegularExpressions;

namespace DeathmicChatbot
{
    internal class Program
    {
        private static Connection _con;
        private static YotubeManager _youtube;
        private static LogManager _log;
        private static WebsiteManager _website;
        private static TwitchManager _twitch;
        private static CommandManager _commands;
        private static VoteManager _voting;
        private static readonly String Channel = Settings.Default.Channel;
        private static readonly String Nick = Settings.Default.Name;
        private static readonly String Server = Settings.Default.Server;
        private static readonly String Logfile = Settings.Default.Logfile;
        private static bool listenForStreams = true;
        private static bool checkVotings = true;

        private static void Main(string[] args)
        {
            ConnectionArgs cona = new ConnectionArgs(Nick, Server);
            _con = new Connection(Encoding.UTF8, cona, false, false);
            _con.Listener.OnRegistered += OnRegistered;
            _con.Listener.OnPublic += OnPublic;
            _con.Listener.OnPrivate += OnPrivate;
            _con.Listener.OnJoin += OnJoin;
            _con.Connect();
            while (true)
            {
                if (!_con.Connected)
                    _con.Connect();
            }
        }

        private static void AddStream(UserInfo user, string channel, string text, string commandArgs)
        {
            if (_twitch.AddStream(commandArgs))
            {
                _log.WriteToLog("Information", String.Format("{0} added {1} to the streamlist", user.Nick, commandArgs));
                _con.Sender.PublicMessage(
                    channel, String.Format("{0} added {1} to the streamlist", user.Nick, commandArgs));
            }
            else
            {
                _log.WriteToLog(
                    "Information", String.Format("{0} wanted to readd {1} to the streamlist", user.Nick, commandArgs));
                _con.Sender.Action(
                    channel, String.Format("slaps {0} around for being an idiot", user.Nick));
            }
        }

        private static void DelStream(UserInfo user, string channel, string text, string commandArgs)
        {
            _log.WriteToLog("Information", String.Format("{0} removed {1} from the streamlist", user.Nick, commandArgs));
            _con.Sender.PublicMessage(
                channel, String.Format("{0} removed {1} from the streamlist", user.Nick, commandArgs));
            _twitch.RemoveStream(commandArgs);
        }

        private static void StreamCheck(UserInfo user, string channel, string text, string commandArgs)
        {
            foreach (StreamData stream in _twitch._streamData.Values)
            {
                _con.Sender.PrivateNotice(
                    user.Nick, String.Format("{0} is streaming at http://www.twitch.tv/{0}", stream.Stream.Channel.Name));
            }
        }

        private static void TwitchOnStreamStopped(object sender, StreamEventArgs args)
        {
            Console.WriteLine("{0}: Stream stopped: {1}", DateTime.Now, args.StreamData.Stream.Channel.Name);
            _con.Sender.PublicMessage(
                Channel,
                String.Format(
                    "Stream stopped after {1}: {0}",
                    args.StreamData.Stream.Channel.Name,
                    string.Format(
                        "{0}:{1}",
                        Math.Floor(args.StreamData.TimeSinceStart.TotalHours),
                        Math.Floor(args.StreamData.TimeSinceStart.TotalMinutes))));
        }

        private static void TwitchOnStreamStarted(object sender, StreamEventArgs args)
        {
            Console.WriteLine("{0}: Stream started: {1}", DateTime.Now, args.StreamData.Stream.Channel.Name);
            _con.Sender.PublicMessage(
                Channel,
                String.Format(
                "Stream started: {0} ({1}: {2}) at http://www.twitch.tv/{0}",
                args.StreamData.Stream.Channel.Name,
                args.StreamData.Stream.Channel.Game,
                args.StreamData.Stream.Channel.Status));
        }

        private static void CheckAllStreamsThreaded()
        {
            while (listenForStreams)
            {
                _twitch.CheckStreams();
                Thread.Sleep(Settings.Default.StreamcheckIntervalSeconds * 1000);
            }
        }

        private static void VotingOnVotingStarted(object sender, VotingEventArgs args)
        {
            _con.Sender.PublicMessage(
                Channel,
                String.Format("{0} started a voting.", args.user.Nick));
            _con.Sender.PublicMessage(Channel, args.voting.question);
            _con.Sender.PublicMessage(Channel, "Possible answers:");
            foreach (string answer in args.voting.answers)
            {
                _con.Sender.PublicMessage(Channel, string.Format("    {0}", answer));
            }
            _con.Sender.PublicMessage(
                Channel,
                String.Format(
                "Vote with /msg {0} vote {1} <answer>",
                Nick,
                args.voting.index + 1));
            _con.Sender.PublicMessage(
                Channel,
                string.Format("Voting runs until {0}", args.voting.endTime.ToString()));

        }

        private static void VotingOnVotingEnded(object sender, VotingEventArgs args)
        {
            _con.Sender.PublicMessage(
                Channel,
                String.Format(
                "The voting '{0}' has ended with the following results:",
                args.voting.question));
            Dictionary<string, int> votes = new Dictionary<string, int>();
            foreach (string answer in args.voting.answers)
                votes[answer] = 0;
            foreach (string answer in args.voting.votes.Values)
            {
                ++votes[answer];
            }
            args.voting.votes.Clear();
            foreach (KeyValuePair<string, int>vote in votes)
            {
                _con.Sender.PublicMessage(
                    Channel,
                    String.Format(
                    "    {0}: {1} votes",
                    vote.Key, vote.Value));
            }
        }

        private static void VotingOnVoted(object sender, VotingEventArgs args)
        {
            _con.Sender.PrivateNotice(
                args.user.Nick,
                String.Format("Your vote for '{0}' has been counted.",
                              args.voting.question));
        }

        private static void VotingOnVoteRemoved(object sender, VotingEventArgs args)
        {
            _con.Sender.PrivateNotice(
                args.user.Nick,
                String.Format("Your vote for '{0}' has been removed.",
                          args.voting.question));
        }

        private static void StartVoting(UserInfo user, string channel, string text, string commandArgs)
        {
            string[] args = commandArgs.Split('|');
            if (args.Length < 3)
            {
                _con.Sender.PrivateNotice(
                    user.Nick,
                    string.Format(
                    "Please use the following format: {0}startvote <time>|<question>|<answer1,answer2,...>",
                    CommandManager.ACTIVATOR));
                return;
            }
            string timeString = args[0];
            Regex timeRegex = new Regex(@"^(\d+d)?(\d+h)?(\d+m)?(\d+s)?$");
            Match timeMatch = timeRegex.Match(timeString);
            if (!timeMatch.Success)
            {
                _con.Sender.PrivateNotice(
                    user.Nick,
                    "Time needs to be in the following format: [<num>d][<num>h][<num>m][<num>s]");
                _con.Sender.PrivateNotice(
                    user.Nick,
                    "Examples: 10m30s\n5h\n1d\n1d6h");
                return;
            }
            TimeSpan span = new TimeSpan();
            TimeSpan tmpSpan = new TimeSpan();
            if (TimeSpan.TryParseExact(
                timeMatch.Groups[1].Value,
                "d'd'",
                null,
                out tmpSpan))
            {
                span += tmpSpan;
            }
            if (TimeSpan.TryParseExact(
                    timeMatch.Groups[2].Value,
                    "h'h'",
                    null,
                    out tmpSpan))
            {
                span += tmpSpan;
            }

            if (TimeSpan.TryParseExact(
                    timeMatch.Groups[3].Value,
                    "m'm'",
                    null,
                    out tmpSpan))
            {
                span += tmpSpan;
            }
            if (TimeSpan.TryParseExact(
                    timeMatch.Groups[4].Value,
                    "s's'",
                    null,
                    out tmpSpan))
            {
                span += tmpSpan;
            }

            string question = args[1];
            List<string> answers = new List<string>(args[2].Split(','));

            DateTime endTime = DateTime.Now + span;
            try
            {
                _voting.StartVoting(user, question, answers, endTime);
                _log.WriteToLog(
                    "Information",
                    String.Format("{0} started a voting: {1}. End Date is: {2}",
                                  user.Nick, question, endTime.ToString()));
            }
            catch (InvalidOperationException e)
            {
                _con.Sender.PrivateNotice(user.Nick, e.Message);
                _log.WriteToLog(
                    "Error",
                    String.Format("{0} tried starting a voting: {1}. But: {2}",
                                  user.Nick, question, e.Message
                )
                );
            }
        }

        private static void EndVoting(UserInfo user, string channel, string text, string commandArgs)
        {
            int index;
            if (!int.TryParse(commandArgs, out index))
            {
                _con.Sender.PrivateNotice(
                    user.Nick,
                    string.Format("The format for ending a vote is: {0}endvote <id>",
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
                    _con.Sender.PrivateNotice(
                        user.Nick,
                        string.Format("There is no voting with the id {0}", index));
                }
                else
                {
                    throw e;
                }
            }
            catch (InvalidOperationException e)
            {
                _con.Sender.PrivateNotice(user.Nick, e.Message);
            }
        }

        private static void Vote(UserInfo user, string text, string commandArgs)
        {
            string[] args = commandArgs.Split(' ');
            if (args.Length < 2)
            {
                _con.Sender.PrivateNotice(
                    user.Nick,
                    string.Format("Format: /msg {0} vote <id> <answer>", Nick));
                _con.Sender.PrivateNotice(
                    user.Nick,
                    string.Format("You can check the running votings with /msg {0} listvotings", Nick));
                return;
            }
            int index;
            string answer = args[1];
            if (!int.TryParse(args[0], out index))
            {
                _con.Sender.PrivateNotice(
                    user.Nick,
                    "id must be a number");
                return;
            }
            try
            {
                _voting.Vote(user, index - 1, answer);
            }
            catch (ArgumentOutOfRangeException e)
            {
                if (e.ParamName == "id")
                {
                    _con.Sender.PrivateNotice(
                        user.Nick,
                        string.Format("There is no voting with the id {0}", index));
                }
                else if (e.ParamName == "answer")
                {
                    _con.Sender.PrivateNotice(
                        user.Nick,
                        string.Format("The voting {0} has no answer {1}", index, answer));
                }
                else
                {
                    throw e;
                }                       
           }
        }

        private static void RemoveVote(UserInfo user, string text, string commandArgs)
        {
            int index;
            if (!int.TryParse(commandArgs, out index))
            {
                _con.Sender.PrivateNotice(
                    user.Nick,
                    string.Format("The format for removintg your vote is: /msg {0} removevote <id>",
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
                    _con.Sender.PrivateNotice(
                        user.Nick,
                        string.Format("There is no voting with the id {0}", index));
                }
                else
                {
                    throw e;
                }   
            }
        }

        private static void ListVotings(UserInfo user, string text, string command_args)
        {
            if (_voting.Votings.Count == 0)
            {
                _con.Sender.PrivateNotice(user.Nick, "There are currently no votings running");
            }
            foreach (Voting voting in _voting.Votings.Values)
            {
                _con.Sender.PrivateNotice(
                    user.Nick,
                    string.Format("{0} - {1}", voting.index + 1, voting.question));
                _con.Sender.PrivateNotice(user.Nick, "Answers:");
                foreach (string answer in voting.answers)
                {
                    _con.Sender.PrivateNotice(
                        user.Nick,
                        string.Format("    {0}", answer)
                    );
                }
                _con.Sender.PrivateNotice(
                    user.Nick, 
                    string.Format("Voting runs until {0}", voting.endTime.ToString()));
            }
        }

        private static void CheckAllVotngsThreaded()
        {
            while (checkVotings)
            {
                _voting.CheckVotings();
                Thread.Sleep(Settings.Default.StreamcheckIntervalSeconds * 1000);
            }
        }

        public static void OnError(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = ((Exception)e.ExceptionObject);
            StackTrace st = new StackTrace(ex, true);
            _log.WriteToLog("Error", ex.Message, st);
        }

        public static void OnJoin(UserInfo user, string channel)
        {
            foreach (StreamData stream in _twitch._streamData.Values)
            {
                _con.Sender.PrivateNotice(
                    user.Nick,
                    String.Format(
                        "{0} is streaming! ===== Game: {1} ===== Message: {2} ===== Started: {3} o'clock ({4} ago) ===== Link: http://www.twitch.tv/{0}",
                        stream.Stream.Channel.Name,
                        stream.Stream.Channel.Game,
                        stream.Stream.Channel.Status,
                        stream.Started.ToString("t"),
                        string.Format(
                            "{0}:{1}",
                            Math.Floor(stream.TimeSinceStart.TotalHours),
                            Math.Floor(stream.TimeSinceStart.TotalMinutes))));
            }
        }

        public static void OnRegistered()
        {
            _con.Sender.Join(Channel);
            _log = new LogManager(Logfile);
            AppDomain.CurrentDomain.UnhandledException += OnError;
            _youtube = new YotubeManager();
            _website = new WebsiteManager(_log);
            _twitch = new TwitchManager();
            _voting = new VoteManager();
            _twitch.StreamStarted += TwitchOnStreamStarted;
            _twitch.StreamStopped += TwitchOnStreamStopped;
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
            CommandManager.PrivateCommand vote = Vote;
            CommandManager.PrivateCommand removevote = RemoveVote;
            CommandManager.PrivateCommand listvotings = ListVotings;
            _commands.SetCommand("addstream", addstream);
            _commands.SetCommand("delstream", delstream);
            _commands.SetCommand("streamwegschreinen", delstream);
            _commands.SetCommand("streamcheck", streamcheck);
            _commands.SetCommand("startvote", startvote);
            _commands.SetCommand("endvote", endvote);
            _commands.SetCommand("vote", vote);
            _commands.SetCommand("listvotings", listvotings);
            _commands.SetCommand("removevote", removevote);
            Thread streamCheckThread = new Thread(CheckAllStreamsThreaded);
            Thread votingCheckThread = new Thread(CheckAllVotngsThreaded);
            streamCheckThread.Start();
            votingCheckThread.Start();
        }

        public static void OnPublic(UserInfo user, string channel, string message)
        {
            if (_commands.CheckCommand(user, channel, message)) return;

            string link = _youtube.IsYtLink(message);

            if (link != null)
            {
                Video vid = _youtube.GetVideoInfo(link);
                _con.Sender.PublicMessage(channel, _youtube.GetInfoString(vid));
                return;
            }

            List<string> urls = _website.ContainsLinks(message);

            foreach (
                string title in
                urls.Select(url => _website.GetPageTitle(url).Trim()).Where(title => !string.IsNullOrEmpty(title)))
            {
                _con.Sender.PublicMessage(channel, title);
            }
        }

        public static void OnPrivate(UserInfo user, string message)
        {
            _commands.CheckCommand(user, Channel, message, true);
        }
    }
}
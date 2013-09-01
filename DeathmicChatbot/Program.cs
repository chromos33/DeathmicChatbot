using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using DeathmicChatbot.Properties;
using DeathmicChatbot.StreamInfo;
using Google.YouTube;
using Sharkbite.Irc;

namespace DeathmicChatbot
{
	internal class Program
	{
		private static ConnectionArgs _cona;
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
		private static bool restarted = false;
		private static Random rnd = new Random();
		private static HashSet<string> chosenUsers = new HashSet<string>();
		private static HashSet<string> currentUsers = new HashSet<string>();
		private static string chosenUsersFile = "chosenusers.txt";
		private static int userUpdateInterval = 60;
		
		private static void Main(string[] args)
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
			loadChosenUsers();
			while (!CheckConnection(_cona))
			{
				Console.WriteLine("OFFLINE");
			}
			_con.Connect();
		}
		
		private static bool CheckConnection(ConnectionArgs cona)
		{
			try {
				Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				s.Connect(cona.Hostname, cona.Port);
				s.Close();
			} catch (Exception) {
				return false;
			}
			return true;
		}

		private static void OnDisconnect()
		{
			while (!CheckConnection(_cona))
			{
				Console.WriteLine("OFFLINE");
			}
			if (!restarted) _con.Connect(); restarted = true;
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
			if (_twitch._streamData.Count == 0)
			{
				_con.Sender.PrivateNotice(user.Nick, "There are currently no streams running :(");
				return;
			}
			foreach (String stream in _twitch.GetStreamInfoArray())
			{
				_con.Sender.PrivateNotice(user.Nick, stream);
			}
		}

		private static void TwitchOnStreamStopped(object sender, StreamEventArgs args)
		{
			Console.WriteLine("{0}: Stream stopped: {1}", DateTime.Now, args.StreamData.Stream.Channel.Name);
			_con.Sender.PublicMessage(
				Channel,
				String.Format(
					"Stream stopped after {1:HH}:{1:mm}: {0}",
					args.StreamData.Stream.Channel.Name,
					new DateTime(args.StreamData.TimeSinceStart.Ticks)
				));
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
			string[] args = commandArgs != null ? commandArgs.Split(' ') : new string[0];
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
			if (commandArgs == null || !int.TryParse(commandArgs, out index))
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
			string[] args = commandArgs != null ? commandArgs.Split(' ') : new string[0];
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
			if (commandArgs == null || !int.TryParse(commandArgs, out index))
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

		private static void CheckConnectionThreaded()
		{
			while (true)
			{
				_con.Sender.RequestTopic(Channel);
				Thread.Sleep(2000);
			}
		}

		private static void OnNames(string channel, string[] nicks, bool last)
		{
			foreach(string nick in nicks)
			{
				if(nick.Trim() != "")
					currentUsers.Add(nick);
			}
		}
		
		private static void pickRandomUser(UserInfo user, string channel, string text, string commandArgs)
		{
			List<string> nameList = new List<string>(currentUsers);
			removeIgnoredUsers(ref nameList);
			if (nameList.Count == 0)
			{
				nameList = new List<string>(currentUsers);
				chosenUsers.Clear();
				removeIgnoredUsers(ref nameList);
			}
			int index = rnd.Next(nameList.Count);
			string chosen = nameList[index];
			_con.Sender.PublicMessage(channel, chosen);
			chosenUsers.Add(chosen);
			saveChosenUsers();
		}

		static void removeIgnoredUsers(ref List<string> nameList)
		{
			List<string> ignores = new List<string>(chosenUsers);
			ignores.AddRange(Settings.Default.pickIgnores.Split(';'));
			foreach (string nick in new List<string>(nameList))
			{
				if (ignores.Contains(nick) || nick == Nick) 
				{
					nameList.Remove(nick);
				}
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
			foreach(String msg in _twitch.GetStreamInfoArray())
			{
				_con.Sender.PrivateNotice(user.Nick, msg);
			}
			currentUsers.Add(user.Nick);
		}

		public static void OnPart(UserInfo user, string channel, string reason)
		{
			currentUsers.Remove(user.Nick);
		}
		
		public static void OnNick(UserInfo user, string newnick)
		{
			if(chosenUsers.Contains(user.Nick))
			{
				chosenUsers.Remove(user.Nick);
				chosenUsers.Add(newnick);
			}
			currentUsers.Remove(user.Nick);
			currentUsers.Add(newnick);
		}

		public static void loadChosenUsers()
		{
			if (!File.Exists(chosenUsersFile)) File.Create(chosenUsersFile).Close();
			StreamReader reader = new StreamReader(chosenUsersFile);
			while (!reader.EndOfStream)
			{
				string nick = reader.ReadLine();
				chosenUsers.Add(nick);
			}
			reader.Close();
		}
		
		public static void saveChosenUsers()
		{
			StreamWriter writer = new StreamWriter(chosenUsersFile, false);
			foreach(string user in chosenUsers)
			{
				writer.WriteLine(user);
			}
			writer.Close();
		}
		
		public static void updateUsersThreaded()
		{
			while (true)
			{
				saveChosenUsers();
				currentUsers.Clear();
				_con.Sender.Names(Channel);
				Thread.Sleep(userUpdateInterval * 1000);
			}
		}
		
		public static void OnRegistered()
		{
			_con.Sender.Join(Channel);
			restarted = false;
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
			CommandManager.PublicCommand pickuser = pickRandomUser;
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
			_commands.SetCommand("pickuser", pickuser);
			Thread streamCheckThread = new Thread(CheckAllStreamsThreaded);
			Thread votingCheckThread = new Thread(CheckAllVotngsThreaded);
			Thread connectionCheckThread = new Thread(CheckConnectionThreaded);
			Thread updateUsersThread = new Thread(updateUsersThreaded);
			streamCheckThread.Start();
			votingCheckThread.Start();
			connectionCheckThread.Start();
			updateUsersThread.Start();
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
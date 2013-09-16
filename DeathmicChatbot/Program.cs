using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using DeathmicChatbot.Properties;
using Sharkbite.Irc;

namespace DeathmicChatbot
{
	internal static class Program
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
		private static bool _restarted;
		private static readonly Random Rnd = new Random();

		private static readonly Queue<KeyValuePair<string, string>> PublicMessageQueue =
			new Queue<KeyValuePair<string, string>>();

		private static readonly Queue<KeyValuePair<string, string>> PrivateNoticeQueue =
			new Queue<KeyValuePair<string, string>>();

		private static readonly System.Timers.Timer MessageTimer = new System.Timers.Timer();
		private const int MESSAGE_QUEUE_INTERVAL_MILLISECONDS = 1000;

		private static readonly ConcurrentDictionary<string, string> ChosenUsers =
			new ConcurrentDictionary<string, string>();

		private static readonly ConcurrentDictionary<string, string> CurrentUsers =
			new ConcurrentDictionary<string, string>();

		private const string CHOSEN_USERS_FILE = "chosenusers.txt";
		private const int USER_UPDATE_INTERVAL = 60;

		private static void Main()
		{
			MessageTimer.Interval = MESSAGE_QUEUE_INTERVAL_MILLISECONDS;
			MessageTimer.Elapsed += MessageTimerOnElapsed;
			MessageTimer.Start();
			LoadChosenUsers();
			Connect();
		}

		static void Connect()
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
			while (!CheckConnection(_cona)) {
				Console.WriteLine("OFFLINE");
			}
			_con.Connect();
		}

		private static void MessageTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (PublicMessageQueue.Count > 0)
			{
				var msg = PublicMessageQueue.Dequeue();
				_con.Sender.PublicMessage(msg.Key, msg.Value);
				return;
			}

			if (PrivateNoticeQueue.Count > 0)
			{
				var msg = PrivateNoticeQueue.Dequeue();
				_con.Sender.PrivateNotice(msg.Key, msg.Value);
				return;
			}
		}

		private static void PublicMessageEnqueue(string sChannel, string sMessage)
		{
			PublicMessageQueue.Enqueue(new KeyValuePair<string, string>(sChannel, sMessage));
		}

		private static void PrivateNoticeEnqueue(string sNick, string sMessage)
		{
			PrivateNoticeQueue.Enqueue(new KeyValuePair<string, string>(sNick, sMessage));
		}

		private static bool CheckConnection(ConnectionArgs cona)
		{
			try
			{
				var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
			while (!CheckConnection(_cona))
			{
				Console.WriteLine("OFFLINE");
			}
			if (!_restarted) Connect();
			_restarted = true;
		}

		private static void AddStream(UserInfo user, string channel, string text, string commandArgs)
		{
			if (_twitch.AddStream(commandArgs))
			{
				_log.WriteToLog("Information", String.Format("{0} added {1} to the streamlist", user.Nick, commandArgs));
				PublicMessageEnqueue(
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
			PublicMessageEnqueue(
				channel, String.Format("{0} removed {1} from the streamlist", user.Nick, commandArgs));
			_twitch.RemoveStream(commandArgs);
		}

		private static void StreamCheck(UserInfo user, string channel, string text, string commandArgs)
		{
			if (_twitch.StreamData.Count == 0)
			{
				PrivateNoticeEnqueue(user.Nick, "There are currently no streams running :(");
				return;
			}
			foreach (var stream in _twitch.GetStreamInfoArray())
			{
				PrivateNoticeEnqueue(user.Nick, stream);
			}
		}

		private static void TwitchOnStreamStopped(object sender, StreamEventArgs args)
		{
			Console.WriteLine("{0}: Stream stopped: {1}", DateTime.Now, args.StreamData.Stream.Channel.Name);
			PublicMessageEnqueue(
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
			PublicMessageEnqueue(
				Channel,
				String.Format(
					"Stream started: {0} ({1}: {2}) at http://www.twitch.tv/{0}",
					args.StreamData.Stream.Channel.Name,
					args.StreamData.Stream.Channel.Game,
					args.StreamData.Stream.Channel.Status));
		}

		private static void CheckAllStreamsThreaded()
		{
			while (true)
			{
				_twitch.CheckStreams();
				Thread.Sleep(Settings.Default.StreamcheckIntervalSeconds*1000);
			}
		}

		private static void VotingOnVotingStarted(object sender, VotingEventArgs args)
		{
			PublicMessageEnqueue(
				Channel,
				String.Format("{0} started a voting.", args.User.Nick));
			PublicMessageEnqueue(Channel, args.Voting.Question);
			PublicMessageEnqueue(Channel, "Possible answers:");
			foreach (var answer in args.Voting.Answers)
			{
				PublicMessageEnqueue(Channel, string.Format("    {0}", answer));
			}
			PublicMessageEnqueue(
				Channel,
				String.Format(
					"Vote with /msg {0} vote {1} <answer>",
					Nick,
					args.Voting.Index + 1));
			PublicMessageEnqueue(
				Channel,
				string.Format("Voting runs until {0}", args.Voting.EndTime));
		}

		private static void VotingOnVotingEnded(object sender, VotingEventArgs args)
		{
			PublicMessageEnqueue(
				Channel,
				String.Format(
					"The voting '{0}' has ended with the following results:",
					args.Voting.Question));
			var votes = new Dictionary<string, int>();
			foreach (var answer in args.Voting.Answers)
				votes[answer] = 0;
			foreach (var answer in args.Voting.Votes.Values)
			{
				++votes[answer];
			}
			args.Voting.Votes.Clear();
			foreach (var vote in votes)
			{
				PublicMessageEnqueue(
					Channel,
					String.Format(
						"    {0}: {1} votes",
						vote.Key, vote.Value));
			}
		}

		private static void VotingOnVoted(object sender, VotingEventArgs args)
		{
			PrivateNoticeEnqueue(
				args.User.Nick,
				String.Format("Your vote for '{0}' has been counted.",
				              args.Voting.Question));
		}

		private static void VotingOnVoteRemoved(object sender, VotingEventArgs args)
		{
			PrivateNoticeEnqueue(
				args.User.Nick,
				String.Format("Your vote for '{0}' has been removed.",
				              args.Voting.Question));
		}

		private static void StartVoting(UserInfo user, string channel, string text, string commandArgs)
		{
			var args = commandArgs != null ? commandArgs.Split('|') : new string[0];
			if (args.Length < 3)
			{
				PrivateNoticeEnqueue(
					user.Nick,
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
				PrivateNoticeEnqueue(
					user.Nick,
					"Time needs to be in the following format: [<num>d][<num>h][<num>m][<num>s]");
				PrivateNoticeEnqueue(
					user.Nick,
					"Examples: 10m30s\n5h\n1d\n1d6h");
				return;
			}
			var span = new TimeSpan();
			TimeSpan tmpSpan;
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

			var question = args[1];
			var answers = new List<string>(args[2].Split(','));

			var endTime = DateTime.Now + span;
			try
			{
				_voting.StartVoting(user, question, answers, endTime);
				_log.WriteToLog(
					"Information",
					String.Format("{0} started a voting: {1}. End Date is: {2}",
					              user.Nick, question, endTime));
			}
			catch (InvalidOperationException e)
			{
				PrivateNoticeEnqueue(user.Nick, e.Message);
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
				PrivateNoticeEnqueue(
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
					PrivateNoticeEnqueue(
						user.Nick,
						string.Format("There is no voting with the id {0}", index));
				}
				else

					throw;
			}
			catch (InvalidOperationException e)
			{
				PrivateNoticeEnqueue(user.Nick, e.Message);
			}
		}

		private static void Vote(UserInfo user, string text, string commandArgs)
		{
			var args = commandArgs != null ? commandArgs.Split(' ') : new string[0];
			if (args.Length < 2)
			{
				PrivateNoticeEnqueue(
					user.Nick,
					string.Format("Format: /msg {0} vote <id> <answer>", Nick));
				PrivateNoticeEnqueue(
					user.Nick,
					string.Format("You can check the running votings with /msg {0} listvotings", Nick));
				return;
			}
			int index;
			var answer = args[1];
			if (!int.TryParse(args[0], out index))
			{
				PrivateNoticeEnqueue(
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
				switch (e.ParamName)
				{
					case "id":
						PrivateNoticeEnqueue(
							user.Nick,
							string.Format("There is no voting with the id {0}", index));
						break;
					case "answer":
						PrivateNoticeEnqueue(
							user.Nick,
							string.Format("The voting {0} has no answer {1}", index, answer));
						break;
					default:
						throw;
				}
			}
		}

		private static void RemoveVote(UserInfo user, string text, string commandArgs)
		{
			int index;
			if (commandArgs == null || !int.TryParse(commandArgs, out index))
			{
				PrivateNoticeEnqueue(
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
					PrivateNoticeEnqueue(
						user.Nick,
						string.Format("There is no voting with the id {0}", index));
				}
				else

					throw;
			}
		}

		private static void ListVotings(UserInfo user, string text, string commandArgs)
		{
			if (_voting.Votings.Count == 0)
			{
				PrivateNoticeEnqueue(user.Nick, "There are currently no votings running");
			}
			foreach (var voting in _voting.Votings.Values)
			{
				PrivateNoticeEnqueue(
					user.Nick,
					string.Format("{0} - {1}", voting.Index + 1, voting.Question));
				PrivateNoticeEnqueue(user.Nick, "Answers:");
				foreach (var answer in voting.Answers)
				{
					PrivateNoticeEnqueue(
						user.Nick,
						string.Format("    {0}", answer)
					);
				}
				PrivateNoticeEnqueue(
					user.Nick,
					string.Format("Voting runs until {0}", voting.EndTime));
			}
		}

		private static void CheckAllVotngsThreaded()
		{
			while (true)
			{
				_voting.CheckVotings();
				Thread.Sleep(Settings.Default.StreamcheckIntervalSeconds*1000);
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
			foreach (var nick in nicks.Where(nick => nick.Trim() != ""))
			{
				CurrentUsers.TryAdd(nick, nick);
			}
		}

		private static void PickRandomUser(UserInfo user, string channel, string text, string commandArgs)
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
			PublicMessageEnqueue(channel, chosen);
			ChosenUsers.TryAdd(chosen, chosen);
			SaveChosenUsers();
		}

		private static void RemoveIgnoredUsers(ref List<string> nameList)
		{
			var ignores = new List<string>(ChosenUsers.Keys);
			ignores.AddRange(Settings.Default.pickIgnores.Split(';'));
			foreach (var nick in new List<string>(nameList).Where(nick => ignores.Contains(nick) || nick == Nick))
			{
				nameList.Remove(nick);
			}
		}

		private static void OnError(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = ((Exception) e.ExceptionObject);
			var st = new StackTrace(ex, true);
			_log.WriteToLog("Error", ex.Message, st);
		}

		private static void OnJoin(UserInfo user, string channel)
		{
			foreach (var msg in _twitch.GetStreamInfoArray())
			{
				PrivateNoticeEnqueue(user.Nick, msg);
			}
			CurrentUsers.TryAdd(user.Nick, user.Nick);
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
			if (!File.Exists(CHOSEN_USERS_FILE)) File.Create(CHOSEN_USERS_FILE).Close();
			var reader = new StreamReader(CHOSEN_USERS_FILE);
			while (!reader.EndOfStream)
			{
				var nick = reader.ReadLine();
				if (nick != null) ChosenUsers.TryAdd(nick, nick);
			}
			reader.Close();
		}

		private static void SaveChosenUsers()
		{
			var writer = new StreamWriter(CHOSEN_USERS_FILE, false);
			foreach (var user in ChosenUsers.Keys)
			{
				writer.WriteLine(user);
			}
			writer.Close();
		}

		private static void UpdateUsers()
		{
			_con.Sender.Names(Channel);
		}

		private static void SaveChosenUsersThreaded()
		{
			while (true)
			{
				SaveChosenUsers();
				Thread.Sleep(USER_UPDATE_INTERVAL*1000);
			}
		}

		private static void SendMessage(UserInfo user, string text, string commandArgs)
		{
			PublicMessageEnqueue(Channel, commandArgs);
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
			CommandManager.PublicCommand pickuser = PickRandomUser;
			CommandManager.PrivateCommand vote = Vote;
			CommandManager.PrivateCommand removevote = RemoveVote;
			CommandManager.PrivateCommand listvotings = ListVotings;
			CommandManager.PrivateCommand sendmessage = SendMessage;
			_commands.SetCommand("addstream", addstream);
			_commands.SetCommand("delstream", delstream);
			_commands.SetCommand("streamwegschreinen", delstream);
			_commands.SetCommand("streamcheck", streamcheck);
			_commands.SetCommand("startvote", startvote);
			_commands.SetCommand("endvote", endvote);
			_commands.SetCommand("stopvote", endvote);
			_commands.SetCommand("votestop", endvote);
			_commands.SetCommand("vote", vote);
			_commands.SetCommand("listvotings", listvotings);
			_commands.SetCommand("removevote", removevote);
			_commands.SetCommand("pickuser", pickuser);
			_commands.SetCommand("say", sendmessage);
			var streamCheckThread = new Thread(CheckAllStreamsThreaded);
			var votingCheckThread = new Thread(CheckAllVotngsThreaded);
			var connectionCheckThread = new Thread(CheckConnectionThreaded);
			var saveChosenUsersThread = new Thread(SaveChosenUsersThreaded);
			streamCheckThread.Start();
			votingCheckThread.Start();
			connectionCheckThread.Start();
			saveChosenUsersThread.Start();
		}

		private static void OnPublic(UserInfo user, string channel, string message)
		{
			if (_commands.CheckCommand(user, channel, message)) return;

			var link = _youtube.IsYtLink(message);

			if (link != null)
			{
				var vid = _youtube.GetVideoInfo(link);
				PublicMessageEnqueue(channel, _youtube.GetInfoString(vid));
				return;
			}

			var urls = _website.ContainsLinks(message);

			foreach (
				var title in
				urls.Select(url => _website.GetPageTitle(url).Trim()).Where(title => !string.IsNullOrEmpty(title)))
			{
				PublicMessageEnqueue(channel, title);
			}
		}

		private static void OnPrivate(UserInfo user, string message)
		{
			_commands.CheckCommand(user, Channel, message, true);
		}
	}
}
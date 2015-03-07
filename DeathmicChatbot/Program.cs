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
		private static readonly String Channel = Settings.Default.Channel;
		private static readonly String Nick = Settings.Default.Name;
		private static readonly String Server = Settings.Default.Server;
		private static readonly String Logfile = Settings.Default.Logfile;
		private static LogManager _log = new LogManager(Logfile);
		private static StreamProviderManager _streamProviderManager;
		private static CommandManager _commands;
		private static VoteManager _voting;
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
		public static XMLProvider xmlprovider;

		private static List<IURLHandler> handlers = new List<IURLHandler>() {
			new Handlers.YoutubeHandler(),
			new Handlers.Imgur(_log),
			new Handlers.WebsiteHandler(_log)
		};
		private static URLExtractor urlExtractor = new URLExtractor();

		private static void Main(string[] args)
		{
			_debugMode = args.Length > 0 && args.Contains("debug");

			ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;

			//Test for XML Implementation
			xmlprovider = new XMLProvider();
			LoadChosenUsers();
			Connect();

			//_model = new Model(new SqliteDatabaseProvider());

            
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
			System.Diagnostics.Debug.WriteLine(_con.Connected);
			do {
				_con.Connect();
			} while (!_con.Connected);

			_messageQueue = new MessageQueue(_con);
		}

		private static bool IsConnectionPossible(ConnectionArgs cona)
		{
			try {
				var s = new Socket(AddressFamily.InterNetwork,
					                    SocketType.Stream,
					                    ProtocolType.Tcp);
				s.Connect(cona.Hostname, cona.Port);
				s.Close();
			} catch (Exception) {
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

		private static void AddStream(MessageContext ctx,
		                              string text,
		                              string commandArgs)
		{
			string nick = ctx.getSenderNick();
			string message = xmlprovider.AddStream(commandArgs, nick);
			_streamProviderManager.AddStream(commandArgs);
			if (message == (nick + " added Stream to the streamlist")) {
				ctx.reply(String.Format(
					"{0} added {1} to the streamlist",
					ctx.getSenderNick(),
					commandArgs));
			} else if (message == (nick + " wanted to readd Stream to the streamlist.")) {
				_con.Sender.Action(ctx.getChannel(),
					String.Format("slaps {0} around for being an idiot", nick));
			}
			_log.WriteToLog("Information", message);
		}

		private static void DelStream(MessageContext ctx,
		                              string text,
		                              string commandArgs)
		{
			string message = xmlprovider.RemoveStream(commandArgs);
			ctx.reply(String.Format(message, ctx.getSenderNick(), commandArgs));
			_log.WriteToLog("Information", String.Format(message, ctx.getSenderNick(), commandArgs));
		}

		private static void StreamCheck(MessageContext ctx,
		                                string text,
		                                string commandArgs)
		{
			if (!_streamProviderManager.GetStreamInfoArray().Any()) {
				ctx.reply("There are currently no streams running :(");
				return;
			}
			foreach (var stream in _streamProviderManager.GetStreamInfoArray())
				ctx.reply(stream);
		}

		private static void OnStreamStopped(object sender, StreamEventArgs args)
		{
			if (xmlprovider == null) {
				xmlprovider = new XMLProvider();
			}
			if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "starttime") != "" && Convert.ToBoolean(xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "running"))) {
				xmlprovider.StreamStartUpdate(args.StreamData.Stream.Channel, true);
				string duration = DateTime.Now.Subtract(Convert.ToDateTime(xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "starttime"))).ToString("h':'mm':'ss");
				Console.WriteLine("{0}: Stream stopped: {1}",
					DateTime.Now,
					args.StreamData.Stream.Channel);
				_messageQueue.PublicMessageEnqueue(Channel,
					String.Format(
						"Stream stopped after {1}: {0}",
						args.StreamData.Stream
                                                           .Channel,
						duration));
			}
            
            
		}

		private static void OnStreamStarted(object sender, StreamEventArgs args)
		{
			if (xmlprovider == null) {
				xmlprovider = new XMLProvider();
			}
			xmlprovider.StreamStartUpdate(args.StreamData.Stream.Channel);
			Console.WriteLine(xmlprovider.isinStreamList("RocketBeansTV"));
			if (xmlprovider.isinStreamList(args.StreamData.Stream.Channel)) {
				Console.WriteLine("{0}: Stream started: {1}",
					DateTime.Now,
					args.StreamData.Stream.Channel);
				_messageQueue.PublicMessageEnqueue(Channel,
					String.Format(
						"Stream started: {0} ({1}: {2}) at {3}/{0}",
						args.StreamData.Stream.Channel,
						args.StreamData.Stream.Game,
						args.StreamData.Stream.Message,
						args.StreamData.StreamProvider.GetLink()));
			}
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
			foreach (var vote in votes) {
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

		private static void CheckAllVotingsThreaded()
		{
			while (true) {
				_voting.CheckVotings();
				Thread.Sleep(Settings.Default.StreamcheckIntervalSeconds * 1000);
			}
		}

		private static void OnNames(string channel, string[] nicks, bool last)
		{
			foreach (var nick in nicks.Where(nick => nick.Trim() != ""))
				CurrentUsers.TryAdd(nick, nick);
		}

		private static void PickRandomUser(MessageContext ctx,
		                                   string text,
		                                   string commandArgs)
		{
			var nameList = new List<string>(CurrentUsers.Keys);
			RemoveIgnoredUsers(ref nameList);
			if (nameList.Count == 0) {
				nameList = new List<string>(CurrentUsers.Keys);
				ChosenUsers.Clear();
				RemoveIgnoredUsers(ref nameList);
			}
			var index = Rnd.Next(nameList.Count);
			var chosen = nameList[index];
			ctx.reply(chosen);
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
			var ex = ((Exception)e.ExceptionObject);
			var st = new StackTrace(ex, true);
			_log.WriteToLog("Error", ex.Message, st);
		}

		private static void OnJoin(UserInfo user, string channel)
		{
			foreach (var msg in _streamProviderManager.GetStreamInfoArray())
				_messageQueue.PrivateNoticeEnqueue(user.Nick, msg);
			CurrentUsers.TryAdd(user.Nick, user.Nick);
			JoinLogger.LogJoin(user.Nick, _messageQueue);
			//Needs testing if AddorUpdateUser is called before LogJoin sent message if so data is incorrect
			if (xmlprovider == null) {
				xmlprovider = new XMLProvider();
			}
			xmlprovider.AddorUpdateUser(user.Nick);
		}

		private static void OnPart(UserInfo user, string channel, string reason)
		{
			if (xmlprovider == null) {
				xmlprovider = new XMLProvider();
			}
			xmlprovider.AddorUpdateUser(user.Nick, true);
			string tmpout;
			CurrentUsers.TryRemove(user.Nick, out tmpout);
		}

		private static void OnNick(UserInfo user, string newnick)
		{
			// Correct this currently would not add Alias to nick because when command is fired nick is alias
			_messageQueue.PrivateNoticeEnqueue(newnick, "Would you like to add this new Nick as an Alias to your User?");
			_messageQueue.PrivateNoticeEnqueue(newnick, "If so enter this '/msg BotDeathmic !addalias " + newnick + "," + user.Nick);
			string tmpout;
			if (ChosenUsers.ContainsKey(user.Nick)) {
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
			while (!reader.EndOfStream) {
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

		private static void UpdateUsers()
		{
			_con.Sender.Names(Channel);
		}

		private static void SaveChosenUsersThreaded()
		{
			while (true) {
				SaveChosenUsers();
				Thread.Sleep(USER_UPDATE_INTERVAL * 1000);
			}
		}

		private static void SendMessage(MessageContext ctx,
		                                string text,
		                                string commandArgs)
		{
			_messageQueue.PublicMessageEnqueue(Channel, commandArgs);
		}

        

		private static void OnRegistered()
		{
			CurrentUsers.Clear();
			_con.Sender.Join(Channel);
			UpdateUsers();
			_restarted = false;
			AppDomain.CurrentDomain.UnhandledException += OnError;
			_streamProviderManager = new StreamProviderManager();
			_streamProviderManager.AddStreamProvider(new TwitchProvider(_log,
				_debugMode));
			_streamProviderManager.AddStreamProvider(
				new HitboxProvider(
					new RestClientProvider(new RestClient("http://api.hitbox.tv")),
					new LogManagerProvider(_log),
					new TextFile(HitboxProvider.STREAMS_FILE),
					_debugMode));
			_streamProviderManager.StreamStarted += OnStreamStarted;
			_streamProviderManager.StreamStopped += OnStreamStopped;
			_commands = new CommandManager();

			_commands.setPublicCommand("addstream", AddStream);
			_commands.setPublicCommand("streamadd", AddStream);
			_commands.setPublicCommand("delstream", DelStream);
			_commands.setPublicCommand("streamdel", DelStream);
			_commands.setPublicCommand("streamwegschreinen", DelStream);
			_commands.setPublicCommand("streamcheck", StreamCheck);
			_commands.setPublicCommand("checkstream", StreamCheck);
			_commands.setPublicCommand("pickuser", PickRandomUser);
			_commands.setPrivateCommand("say", SendMessage);
			_commands.setPublicCommand("count", CounterCount);
			_commands.setPublicCommand("counterReset", CounterReset);
			_commands.setPublicCommand("counterStats", CounterStats);
			_commands.setPublicCommand("toggleuserlogging", ToggleUserLogging);
			_commands.setPublicCommand("addalias", AddAlias);

			new Dice(_commands);
			_voting = new VoteManager(_commands, _log, Nick);

			_voting.VotingStarted += VotingOnVotingStarted;
			_voting.VotingEnded += VotingOnVotingEnded;
			_voting.Voted += VotingOnVoted;
			_voting.VoteRemoved += VotingOnVoteRemoved;

			Counter.CountRequested += CounterOnCountRequested;
			Counter.StatRequested += CounterOnStatRequested;
			Counter.ResetRequested += CounterOnResetRequested;

			var votingCheckThread = new Thread(CheckAllVotingsThreaded);
			var saveChosenUsersThread = new Thread(SaveChosenUsersThreaded);

			votingCheckThread.Start();
			saveChosenUsersThread.Start();
		}

		private static void ToggleUserLogging(UserInfo user, string text, string commandArgs)
		{
			if (xmlprovider == null) {
				xmlprovider = new XMLProvider();
			}
			_messageQueue.PrivateNoticeEnqueue(user.Nick, xmlprovider.ToggleUserLogging(user.Nick));
		}


		private static void AddAlias(MessageContext ctx, string text, string commandArgs)
		{
			if (xmlprovider == null) {
				xmlprovider = new XMLProvider();
			}
			if (commandArgs.IndexOf(',') >= 0) {
				System.Diagnostics.Debug.WriteLine(commandArgs.IndexOf(','));
				string[] commandArgssplit = commandArgs.Split(',');
				// [0] = alias , [1] = Nick
				ctx.reply(xmlprovider.AddAlias(commandArgssplit[1], commandArgssplit[0]));
			} else {
				ctx.reply(xmlprovider.AddAlias(ctx.getSenderNick(), commandArgs));
			}
            
		}

		private static void CounterOnResetRequested(object sender,
		                                                  CounterEventArgs
                                                        counterEventArgs)
		{
			_messageQueue.PublicMessageEnqueue(Channel, counterEventArgs.Message);
		}

		private static void CounterOnStatRequested(object sender,
		                                                 CounterEventArgs
                                                       counterEventArgs)
		{
			_messageQueue.PublicMessageEnqueue(Channel, counterEventArgs.Message);
		}

		private static void CounterOnCountRequested(object sender,
		                                                  CounterEventArgs
                                                        counterEventArgs)
		{
			_messageQueue.PublicMessageEnqueue(Channel, counterEventArgs.Message);
		}

		private static void CounterStats(MessageContext ctx,
		                                 string text,
		                                 string commandargs)
		{
			var split = commandargs.Split(new[] { ' ' });
			if (String.IsNullOrEmpty(split[0])) {
				Counter.CounterStats(split[0]);
			} else {
				ctx.reply("Error: counterStats needs a counter name. '!counterStats <name>'");
				return;
			}
		}

		private static void CounterReset(MessageContext ctx,
		                                 string text,
		                                 string commandargs)
		{
			var split = commandargs.Split(new[] { ' ' });
			if (split.Length < 1) {
				ctx.reply("Error: counterReset needs a counter name. '!counterReset <name>'");
				return;
			}

			var sName = split[0];

			Counter.CounterReset(sName);
		}

		private static void CounterCount(MessageContext ctx,
		                                 string text,
		                                 string commandargs)
		{
			var split = commandargs.Split(new[] { ' ' });
			if (split.Length < 1) {
				ctx.reply("Error: count needs a counter name. '!count <name>'");
				return;
			}

			var sName = split[0];

			Counter.Count(sName);
		}

		private static void OnPublic(UserInfo user,
		                                   string channel,
		                                   string message)
		{
			MessageContext ctx = new MessageContext(channel, _messageQueue, user, false);
			if (_commands.CheckCommand(ctx, message))
				return;
			IEnumerable<string> urls = urlExtractor.extractURLs(message);

			if (urls.Count() > 0) {
				foreach (var url in urls)
					_log.WriteToLog("Information", "URL found: " + url);
			}

			foreach (var url in urls) {
				foreach (var handler in handlers) {
					if (handler.handleURL(url, ctx))
						break;
				}
			}
		}

		private static void OnPrivate(UserInfo user, string message)
		{
			_commands.CheckCommand(new MessageContext(Channel, _messageQueue, user, true), message);
		}
	}
}

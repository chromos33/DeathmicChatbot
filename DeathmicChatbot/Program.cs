using System;
using System.Text;
using System.Threading;
using DeathmicChatbot.Properties;
using Sharkbite.Irc;
using Google.YouTube;
using System.Diagnostics;
using System.Collections.Generic;

namespace DeathmicChatbot
{
    internal class Program
    {
        private static Connection _con;
        private static YotubeManager _youtube;
        private static LogManager _log;
        private static WebsiteManager _website;
        private static TwitchManager _twitch;
        private static readonly String Channel = Settings.Default.Channel;
        private static readonly String Nick = Settings.Default.Name;
        private static readonly String Server = Settings.Default.Server;
        private static readonly String Logfile = Settings.Default.Logfile;

        private static bool listenForStreams = true;

        private static void Main(string[] args)
        {
            _log = new LogManager(Logfile);
            AppDomain.CurrentDomain.UnhandledException += OnError;
            _youtube = new YotubeManager();
            _website = new WebsiteManager(_log);
            _twitch = new TwitchManager();
            _twitch.StreamStarted += TwitchOnStreamStarted;
            _twitch.StreamStopped += TwitchOnStreamStopped;

            ConnectionArgs cona = new ConnectionArgs(Nick, Server);
            _con = new Connection(Encoding.UTF8, cona, false, false);
            _con.Listener.OnRegistered += OnRegistered;
            _con.Listener.OnPublic += OnPublic;
            _con.Listener.OnPrivate += OnPrivate;
            //_con.Connect();

            Thread streamCheckThread = new Thread(CheckAllStreamsThreaded);
            streamCheckThread.Start();
        }

        private static void TwitchOnStreamStopped(object sender, StreamEventArgs args)
        {
            Console.WriteLine("{0}: Stream stopped: {1}", DateTime.Now, args.StreamData.Stream.Channel.Name);
        }

        private static void TwitchOnStreamStarted(object sender, StreamEventArgs args)
        {
            Console.WriteLine("{0}: Stream started: {1}", DateTime.Now, args.StreamData.Stream.Channel.Name);
        }

        private static void CheckAllStreamsThreaded()
        {
            while (listenForStreams)
            {
                _twitch.CheckStreams();
                Thread.Sleep(Settings.Default.StreamcheckIntervalSeconds*1000);
            }
        }

        public static void OnError(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = ((Exception) e.ExceptionObject);
            StackTrace st = new StackTrace(ex, true);
            _log.WriteToLog("Error", ex.Message, st);
        }

        public static void OnRegistered()
        {
            _con.Sender.Join(Channel);
        }


        public static void OnPublic(UserInfo user, string channel, string message)
        {
            string link = _youtube.IsYtLink(message);
            if (link != null)
            {
                Video vid = _youtube.GetVideoInfo(link);
                _con.Sender.PublicMessage(channel, _youtube.GetInfoString(vid));
                return;
            }
			List<string> urls = _website.ContainsLinks(message);
			foreach (string url in urls)
			{
				string title = _website.GetPageTitle(url).Trim();
				if (!string.IsNullOrEmpty(title)) _con.Sender.PublicMessage(channel, title);
			}
			if (urls.Count > 0)
				return;
        }

        public static void OnPrivate(UserInfo user, string message)
        {
            Console.WriteLine(user.Nick + ": " + message);
        }
    }
}
using System;
using System.Text;
using System.Threading;
using DeathmicChatbot.Properties;
using Sharkbite.Irc;
using Google.YouTube;
using System.Diagnostics;

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
            _commands = new CommandManager();
            CommandManager.Command addstream = AddStream;
            CommandManager.Command delstream = DelStream;
            _commands.SetCommand("addstream", addstream);
            _commands.SetCommand("delstream", delstream);
            _commands.SetCommand("streamwegschreinen", delstream);
            ConnectionArgs cona = new ConnectionArgs(Nick, Server);
            _con = new Connection(Encoding.UTF8, cona, false, false);
            _con.Listener.OnRegistered += OnRegistered;
            _con.Listener.OnPublic += OnPublic;
            _con.Listener.OnPrivate += OnPrivate;
            _con.Connect();

            Thread streamCheckThread = new Thread(CheckAllStreamsThreaded);
            streamCheckThread.Start();
        }

        private static void AddStream(UserInfo user, string channel, string text, string commandArgs)
        {
            _twitch.AddStream(commandArgs);
        }

        private static void DelStream(UserInfo user, string channel, string text, string commandArgs)
        {
            _twitch.RemoveStream(commandArgs);
        }

        private static void TwitchOnStreamStopped(object sender, StreamEventArgs args)
        {
            Console.WriteLine("{0}: Stream stopped: {1}", DateTime.Now, args.StreamData.Stream.Channel.Name);
            _con.Sender.PublicNotice(Channel, String.Format("Stream stopped: {0}", args.StreamData.Stream.Channel.Name));
        }

        private static void TwitchOnStreamStarted(object sender, StreamEventArgs args)
        {
            Console.WriteLine("{0}: Stream started: {1}", DateTime.Now, args.StreamData.Stream.Channel.Name);
            _con.Sender.PublicNotice(Channel, String.Format("Stream started: {0} at http://www.twitch.tv/{1}", args.StreamData.Stream.Channel.Name, args.StreamData.Stream.Channel.Name));
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
            if (_commands.CheckCommand(user, channel, message)) return;
            string link = _youtube.IsYtLink(message);
            if (link != null)
            {
                Video vid = _youtube.GetVideoInfo(link);
                _con.Sender.PublicMessage(channel, _youtube.GetInfoString(vid));
                return;
            }
            link = _website.IsWebpage(message);
            if (link == "") return;
            string title = _website.GetPageTitle(link).Trim();
            if (!string.IsNullOrEmpty(title)) _con.Sender.PublicMessage(channel, title);
        }

        public static void OnPrivate(UserInfo user, string message)
        {
            Console.WriteLine(user.Nick + ": " + message);
        }
    }
}
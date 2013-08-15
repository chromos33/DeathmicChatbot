using System;
using System.Text;
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
        private static readonly String Channel = Settings.Default.Channel;
        private static readonly String Nick = Settings.Default.Name;
        private static readonly String Server = Settings.Default.Server;
        private static readonly String Logfile = Settings.Default.Logfile;

        private static void Main(string[] args)
        {
            _log = new LogManager(Logfile);
            AppDomain.CurrentDomain.UnhandledException += OnError;
            _youtube = new YotubeManager();
            _website = new WebsiteManager(_log);

            ConnectionArgs cona = new ConnectionArgs(Nick, Server);
            _con = new Connection(Encoding.UTF8, cona, false, false);
            _con.Listener.OnRegistered += OnRegistered;
            _con.Listener.OnPublic += OnPublic;
            _con.Listener.OnPrivate += OnPrivate;
            _con.Connect();
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
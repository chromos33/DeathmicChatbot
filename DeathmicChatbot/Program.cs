using System;
using System.Text;
using DeathmicChatbot.Properties;
using Sharkbite.Irc;
using Google.YouTube;
using System.Diagnostics;

namespace DeathmicChatbot
{
    class Program
    {
        private static Connection con;
        private static YotubeManager youtube;
        private static LogManager log;
        private static WebsiteManager website;
        private static String channel = Settings.Default.Channel;
        private static String nick = Settings.Default.Name;
        private static String server = Settings.Default.Server;
        private static String logfile = Settings.Default.Logfile;

        static void Main(string[] args)
        {
            log = new LogManager(logfile);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnError);
            youtube = new YotubeManager();
            website = new WebsiteManager(log);

            ConnectionArgs cona = new ConnectionArgs(nick, server);
            con = new Connection(Encoding.UTF8, cona, false, false);
            con.Listener.OnRegistered += new RegisteredEventHandler(OnRegistered);
            con.Listener.OnPublic += new PublicMessageEventHandler(OnPublic);
            con.Listener.OnPrivate += new PrivateMessageEventHandler(OnPrivate);
            con.Connect();

        }

        public static void OnError(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = ((Exception)e.ExceptionObject);
            StackTrace st = new StackTrace(ex, true);
            log.WriteToLog("Error", ex.Message, st);
        }

        public static void OnRegistered()
        {
            con.Sender.Join(channel);
        }


        public static void OnPublic(UserInfo user, string channel, string message)
        {
            string link = youtube.isYtLink(message);
            if (link != null)
            {
                Video vid = youtube.getVideoInfo(link);
                con.Sender.PublicMessage(channel, youtube.getInfoString(vid));
                return;
            }
            link = website.isWebpage(message);
            if (link != "")
            {
            	string title = website.getPageTitle(link).Trim();
		    	if (!string.IsNullOrEmpty(title)) con.Sender.PublicMessage(channel, title);
                return;
            }
        }

        public static void OnPrivate(UserInfo user, string message)
        {
            Console.WriteLine(user.Nick + ": " + message);
        }

    }
}

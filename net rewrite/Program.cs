using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharkbite.Irc;
using net_rewrite.StreamInfo;
using Google.YouTube;
using net_rewrite.Properties;

namespace net_rewrite
{
    class Program
    {
        private static Connection con;
        private static YotubeManager youtube;
        private static WebsiteManager website;
        private static String channel = Settings.Default.Channel;
        private static String nick = Settings.Default.Name;
        private static String server = Settings.Default.Server;

        static void Main(string[] args)
        {
            youtube = new YotubeManager();
            website = new WebsiteManager();
            
            ConnectionArgs cona = new ConnectionArgs(nick, server);
            con = new Connection(Encoding.UTF8, cona, false, false);
            con.Listener.OnRegistered += new RegisteredEventHandler(OnRegistered);
            con.Listener.OnPublic += new PublicMessageEventHandler(OnPublic);
            con.Listener.OnPrivate += new PrivateMessageEventHandler(OnPrivate);
            con.Connect();
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
            if (link != null)
            {
            	con.Sender.PublicMessage(channel, website.getPageTitle(link));
                return;
            }
        }

        public static void OnPrivate(UserInfo user, string message)
        {
            Console.WriteLine(user.Nick + ": " + message);
        }

    }
}

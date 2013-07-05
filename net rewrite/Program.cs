using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharkbite.Irc;
using net_rewrite.StreamInfo;
using Google.YouTube;

namespace net_rewrite
{
    class Program
    {
        private static Connection con;
        private static YotubeManager youtube;
        private static String channel = "#deathmic";

        static void Main(string[] args)
        {
            youtube = new YotubeManager();

            ConnectionArgs cona = new ConnectionArgs("YotubeBot", "irc.quakenet.org");
            con = new Connection(cona, false, false);
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
            }
        }

        public static void OnPrivate(UserInfo user, string message)
        {
            Console.WriteLine(user.Nick + ": " + message);
        }

    }
}

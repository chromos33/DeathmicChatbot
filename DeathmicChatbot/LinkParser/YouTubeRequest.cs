using System;
using System.Text.RegularExpressions;
using DeathmicChatbot.Interfaces;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace DeathmicChatbot.LinkParser
{
    //TODO: Handle Playlist Requests
    //Request should look like this 
    //https://www.googleapis.com/youtube/v3/playlists?id=id&key=key&part=snippet
    internal class YoutubeHandler : IURLHandler
    {
        private readonly Regex _reg;

        public YoutubeHandler()
        {

            _reg =
                new Regex(
                    "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:youtu\\.be\\/|youtube\\.com\\/(?:embed\\/|v\\/|watch\\?v\\=))([\\w-]{10,12})(?:[\\&\\?\\#].*?)*?(?:[\\&\\?\\#]t=([\\dhm]+s))?$");
        }

        public string IsYtLink(string txt)
        {
            var match = _reg.Match(txt);
            return match.Success ? match.Groups[1].Value : null;
        }

        public bool handleURL(string URL, IrcDotNet.IrcClient ctx)
        {
            string answer ="";
            var match = IsYtLink(URL);
            if(match != null)
            {
                string url = "https://www.googleapis.com/youtube/v3/videos";
                // video ID
                url += "?id=";
                url += match;
                // API Token
                url += "&key=";
                url += "AIzaSyBQwWTl6Md5oOm858tKi4xIBGH3ELSaa_A";
                // Fields
                url += "&fields=";
                url += "items%28id,snippet%28channelId,title,categoryId%29,statistics%29&part=snippet,statistics";
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream receivedstream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader readStream = new StreamReader(receivedstream, encode);
                Char[] read = new Char[1000000];
                // setting max Chars for read (1M suffice aprox. 4700 Tickets)
                int count = readStream.Read(read, 0, 1000000);
                String str = "";
                while (count > 0)
                {
                    str = new String(read, 0, count);
                    count = readStream.Read(read, 0, 1000000);
                }
                JObject obj = JObject.Parse(str);
                JArray jarr = (JArray)obj["items"];

                foreach (var item in jarr)
                {
                    answer = item["snippet"].SelectToken("title").ToString();

                }
                ctx.LocalUser.SendMessage(Properties.Settings.Default.Channel, answer);
                return true;
            }
            return false;
        }
    }
}

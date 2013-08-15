using System;
using Google.YouTube;
using System.Text.RegularExpressions;

namespace DeathmicChatbot
{
    public class YotubeManager
    {
        private YouTubeRequest request;
        private readonly Regex reg;

        public YotubeManager() 
        {
            YouTubeRequestSettings settings = new YouTubeRequestSettings("Youtube Title Bot", "AI39si6DFGChi5M0rnrX6p5dasT6STlFELYpJdbxdVXR3L1-Cj5RzNUU2nsm2LPmshlVGHuYmeaZ30zGJgqdhSSNoWQgJmEEDA");
            this.request = new YouTubeRequest(settings);
            this.reg = new Regex("^(?:https?\\:\\/\\/)?(?:www\\.)?(?:youtu\\.be\\/|youtube\\.com\\/(?:embed\\/|v\\/|watch\\?v\\=))([\\w-]{10,12})(?:[\\&\\?\\#].*?)*?(?:[\\&\\?\\#]t=([\\dhm]+s))?$");
        }

        public Video getVideoInfo(string addr)
        {
            Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + addr);
            return this.request.Retrieve<Video>(videoEntryUrl);
        }

        public string isYtLink(string txt)
        {
            Match mt = this.reg.Match(txt);
            if (mt.Success)
            {
                return mt.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        public string getInfoString(Video vi)
        {
            return vi.Title + " " + Math.Round(vi.RatingAverage, 2) + "Ø";
        }
    }
}

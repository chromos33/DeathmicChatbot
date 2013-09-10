using System;
using Google.YouTube;
using System.Text.RegularExpressions;

namespace DeathmicChatbot
{
    public class YotubeManager
    {
        private readonly YouTubeRequest _request;
        private readonly Regex _reg;

        public YotubeManager()
        {
            var settings = new YouTubeRequestSettings(
                "Youtube Title Bot",
                "AI39si6DFGChi5M0rnrX6p5dasT6STlFELYpJdbxdVXR3L1-Cj5RzNUU2nsm2LPmshlVGHuYmeaZ30zGJgqdhSSNoWQgJmEEDA");
            _request = new YouTubeRequest(settings);
            _reg =
                new Regex(
                    "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:youtu\\.be\\/|youtube\\.com\\/(?:embed\\/|v\\/|watch\\?v\\=))([\\w-]{10,12})(?:[\\&\\?\\#].*?)*?(?:[\\&\\?\\#]t=([\\dhm]+s))?$");
        }

        public Video GetVideoInfo(string addr)
        {
            var videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + addr);
            return _request.Retrieve<Video>(videoEntryUrl);
        }

        public string IsYtLink(string txt)
        {
            var mt = _reg.Match(txt);
            return mt.Success ? mt.Groups[1].Value : null;
        }

        public string GetInfoString(Video vi)
        {
            return vi.Title + " " + Math.Round(vi.RatingAverage, 2) + "Ø";
        }
    }
}
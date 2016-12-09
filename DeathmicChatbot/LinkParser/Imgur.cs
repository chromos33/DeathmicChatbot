using DeathmicChatbot.Interfaces;
using IrcDotNet;
using IrcDotNet.Ctcp;
using System.Text.RegularExpressions;

namespace DeathmicChatbot.LinkParser
{
    internal class Imgur : IURLHandler
    {
        private readonly Regex _imgurreg;
        private readonly WebsiteHandler website;

        public Imgur()
        {
            _imgurreg = new Regex(@"(https?:\/\/)?i.imgur.com\/(\w+).\w+");
            website = new WebsiteHandler();
        }

        public bool handleURL(string url, IrcDotNet.IrcClient ctx, CtcpClient ctpcclient = null, IIrcMessageSource source = null)
        {
            var match = _imgurreg.Match(url);

            if (match.Success)
            {
                var id = match.Groups[2].Value;
                url = "http://imgur.com/gallery/" + id;
                website.handleURL(url, ctx, ctpcclient, source);
                return true;
            }
            return false;
        }
    }
}

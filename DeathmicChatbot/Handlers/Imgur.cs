using System;
using System.Text.RegularExpressions;

namespace DeathmicChatbot.Handlers
{
	internal class Imgur : IURLHandler
	{
		private readonly Regex _imgurreg;
		private readonly WebsiteHandler website;

		public Imgur (LogManager log_)
		{
			_imgurreg = new Regex(@"(https?:\/\/)?i.imgur.com\/(\w+).\w+");
			website = new WebsiteHandler(log_);
		}

		public bool handleURL(string url, MessageContext ctx)
		{
			var match = _imgurreg.Match(url);

			if (match.Success)
			{
				var id = match.Groups[2].Value;
				url = "http://imgur.com/gallery/" + id;
				website.handleURL(url, ctx);
				return true;
			}
			return false;
		}
	}
}


#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Google.GData.Client;
using HtmlAgilityPack;

#endregion


namespace DeathmicChatbot
{
	internal class WebsiteManager : IURLHandler
    {
        private readonly Regex _imgurreg;
        private readonly LogManager _log;

        public WebsiteManager(LogManager log)
        {
            _imgurreg = new Regex(@"(https?:\/\/)?i.imgur.com\/(\w+).\w+");
            _log = log;
        }

        public string GetPageTitle(string url)
        {
            try
            {
                string pageTitle;
                if (GetPageTitleForUrl(url, out pageTitle))
                    return pageTitle;
            }
            catch (WebException ex)
            {
                var stackTrace = new StackTrace(ex, true);
                _log.WriteToLog("Error", ex.Message, stackTrace);
            }

            return "";
        }

        private bool GetPageTitleForUrl(string url, out string pageTitle)
        {
            url = TransformIfImgurLink(url);

            var htmlDocument = TryLoadingDocument(url, new HtmlWeb());

            if (htmlDocument != null)
            {
                if (PageTitleFromMetaTags(url, htmlDocument, out pageTitle))
                    return true;
            }

            pageTitle = "";

            return false;
        }

        private bool PageTitleFromMetaTags(string url,
                                           HtmlDocument htmlDocument,
                                           out string pageTitle)
        {
            var metaTags = htmlDocument.DocumentNode.SelectNodes("//title");
            if (metaTags != null)
            {
                pageTitle = GetPageTitleFromMetaTags(url, metaTags);
                return true;
            }
            pageTitle = "";
            return false;
        }

        private string GetPageTitleFromMetaTags(string url,
                                                IList<HtmlNode> metaTags)
        {
            var title =
                HttpUtility.HtmlDecode(metaTags[0].InnerText)
                           .Replace("\r\n", "");
            _log.WriteToLog("Information", url + " title: " + title);
            return title;
        }

        private HtmlDocument TryLoadingDocument(string url, HtmlWeb webGet)
        {
            HtmlDocument doc;
            try
            {
                doc = webGet.Load(url);
            }
            catch (UriFormatException)
            {
                doc = TryLoadingWithPrefixHttp(url, webGet);
            }
            return doc;
        }

        private HtmlDocument TryLoadingWithPrefixHttp(string url, HtmlWeb webGet)
        {
            HtmlDocument doc;
            try
            {
                doc = webGet.Load("http://" + url);
            }
            catch (UriFormatException ex)
            {
                var st = new StackTrace(ex, true);
                _log.WriteToLog("Error", ex.Message, st);
                doc = null;
            }
            return doc;
        }

        private string TransformIfImgurLink(string url)
        {
            var match = _imgurreg.Match(url);

            if (match.Success)
            {
                var id = match.Groups[2].Value;
                url = "http://imgur.com/gallery/" + id;
            }

            return url;
        }

		public bool handleURL(string url, MessageContext ctx) {
			var title = GetPageTitle(url).Trim ();
			if (!string.IsNullOrWhiteSpace(title))
				ctx.reply(title);
			return true;
		}
    }
}
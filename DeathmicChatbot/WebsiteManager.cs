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
    internal class WebsiteManager
    {
        private readonly Regex _imgurreg;
        private readonly LogManager _log;
        private readonly Regex _reg;

        public WebsiteManager(LogManager log)
        {
            //Regex by Diego Perini: https://gist.github.com/dperini/729294
            //Modifiziert um urls auch ohne Protokoll zuzulassen
            _reg =
                new Regex(
                    @"(?:(?:https?|ftp):\/\/)?(?:\S+(?::\S*)?@)?(?:(?!10(?:\.\d{1,3}){3})(?!127(?:\.\d{1,3}){3})(?!169\.254(?:\.\d{1,3}){2})(?!192\.168(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:\/[^\s]*)?");
            _imgurreg = new Regex(@"(https?:\/\/)?i.imgur.com\/(\w+).\w+");
            _log = log;
        }

        public IEnumerable<string> ContainsLinks(string txt)
        {
            var match = _reg.Match(txt);
            var urls = new List<string>();

            while (match.Success)
            {
                urls.Add(match.Value);
                match = match.NextMatch();
            }

            if (urls.Count > 0)
            {
                foreach (var url in urls)
                    _log.WriteToLog("Information", "URL found: " + url);
            }

            return urls;
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
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Google.GData.Client;
using HtmlAgilityPack;
using DeathmicChatbot.Interfaces;
using IrcDotNet;

namespace DeathmicChatbot.LinkParser
{
    internal class WebsiteHandler : IURLHandler
    {

        public WebsiteHandler()
        {
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
            }

            return "";
        }

        private bool GetPageTitleForUrl(string url, out string pageTitle)
        {
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
                doc = null;
            }
            return doc;
        }

        public bool handleURL(string url, IrcClient ctx)
        {
            var title = GetPageTitle(url).Trim();
            if (!string.IsNullOrWhiteSpace(title))
                ctx.LocalUser.SendMessage(Properties.Settings.Default.Channel,title);
            return true;
        }
    }
}

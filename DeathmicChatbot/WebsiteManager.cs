using System.Text.RegularExpressions;
using Google.GData.Client;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Collections.Generic;

namespace DeathmicChatbot
{
    /// <summary>
    /// Description of WebsiteManager.
    /// </summary>
    internal class WebsiteManager
    {
        private readonly Regex _reg;
		private readonly Regex _imgurreg;
        private readonly LogManager _log;

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

		public List<string> ContainsLinks(string txt)
        {
            Match mt = _reg.Match(txt);
			List<string> urls = new List<string>();
            while (mt.Success)
            {
				urls.Add(mt.Value);
                mt = mt.NextMatch();
            }
			if (urls.Count > 0) 
            {
				foreach (string url in urls)
				{
					_log.WriteToLog ("Information", "URL found: " + url);					                     
				}
            }
            else
            {
                _log.WriteToLog("Information", "No URL found.");
            }
			return urls;
        }

        public string GetPageTitle(string url)
        {
            try
            {
				Match mt = _imgurreg.Match (url);
				if (mt.Success)
				{
					string id = mt.Groups [2].Value;
					url = "http://imgur.com/gallery/" + id;
				}
				var webGet = new HtmlWeb();
                HtmlDocument doc;
                try
                {
                    doc = webGet.Load(url);
                }
                catch (System.UriFormatException)
                {
                    try
                    {
                        doc = webGet.Load("http://" + url);
                    }
                    catch (System.UriFormatException ex)
                    {
                        StackTrace st = new StackTrace(ex, true);
                        _log.WriteToLog("Error", ex.Message, st);
                        doc = null;
                    }
                }
                if (doc != null)
                {
                    HtmlNodeCollection metaTags = doc.DocumentNode.SelectNodes("//title");
                    if (metaTags != null)
                    {
                        string title = HttpUtility.HtmlDecode(metaTags[0].InnerText).Replace("\r\n", "");
                        _log.WriteToLog("Information", url + " title: " + title);
                        return title;
                    }
                }
            }
            catch (System.Net.WebException ex)
            {
                StackTrace st = new StackTrace(ex, true);
                _log.WriteToLog("Error", ex.Message, st);
                return "";
            }
            return "";
        }
    }
}
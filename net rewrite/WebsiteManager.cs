using System;
using System.Text;
using System.Text.RegularExpressions;

using Google.GData.Client;
using HtmlAgilityPack;

namespace net_rewrite
{
	/// <summary>
	/// Description of WebsiteManager.
	/// </summary>
	public class WebsiteManager
	{
		
		private readonly Regex reg;
		
		public WebsiteManager()
		{
			//Regex von hier: http://regexlib.com/REDetails.aspx?regexp_id=1374
			this.reg = new Regex(@"(([\w]+:)?//)?(([\d\w]|%[a-fA-f\d]{2,2})+(:([\d\w]|%[a-fA-f\d]{2,2})+)?@)?([\d\w][-\d\w]{0,253}[\d\w]\.)+[\w]{2,4}(:[\d]+)?(/([-+_~.\d\w]|%[a-fA-f\d]{2,2})*)*(\?(&?([-+_~.\d\w]|%[a-fA-f\d]{2,2})=?)*)?(#([-+_~.\d\w]|%[a-fA-f\d]{2,2})*)?");
		}
		
		public string isWebpage(string txt)
		{
            Match mt = this.reg.Match(txt);
            string url=null;
            while (mt.Success)
            {
            	url += mt.Value;
            	mt = mt.NextMatch();
            }
            return url;
		}
		
		public string getPageTitle(string url)
		{
			var webGet = new HtmlWeb();
			HtmlDocument doc;
			try
			{
				doc = webGet.Load(url);
			}
			catch (System.UriFormatException)
			{
				doc = webGet.Load("http://" + url);						
			}
			if (doc != null)
			{
				HtmlNodeCollection metaTags = doc.DocumentNode.SelectNodes("//title");
			    if (metaTags != null)
			    {
			    	return HttpUtility.HtmlDecode(metaTags[0].InnerText).Replace("\r\n", "");
			    }			
			}
			return null;			
		}
	}
}

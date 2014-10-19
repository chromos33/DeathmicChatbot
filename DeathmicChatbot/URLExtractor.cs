using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DeathmicChatbot
{
	public class URLExtractor
	{
		private readonly Regex regex;

		public URLExtractor ()
		{
			regex = new Regex(@"\b(?:https?:\/\/)?(?:[a-zA-Z\u00a1-\uffff0-9\-]*[a-zA-Z\u00a1-\uffff][a-zA-Z\u00a1-\uffff0-9\-]*)(?:\.(?:[a-zA-Z\u00a1-\uffff0-9]+-?)*[a-zA-Z\u00a1-\uffff0-9]+)+(?::\d{2,5})?(\/[^\b\/]*)*\b");
		}

		public IEnumerable<string> extractURLs(string text) {
			var match = regex.Match (text.ToLower());
			var urls = new List<string> ();

			while (match.Success) {
				urls.Add (match.Value.Trim());
				match = match.NextMatch ();
			}

			return urls;
		}
	}
}


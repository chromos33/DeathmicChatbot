using System;

namespace DeathmicChatbot
{
	internal interface IURLHandler
	{
		/*! handles a URL
		 * \return true if appropriate action could be taken for this URL and handling with other handlers should be stopped
		 */
		bool handleURL(string url, MessageContext ctx);
	}
}


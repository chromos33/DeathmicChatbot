using System;

namespace DeathmicChatbot
{
	internal class MessageContext
	{
		private readonly string channel;
		private readonly MessageQueue mqueue;
		private readonly string sender;
		private readonly bool priv;

		public MessageContext(string channel_, MessageQueue mq, string sender_, bool private_)
		{
			channel = channel_;
			mqueue = mq;
			sender = sender_;
			priv = private_;
		}

		public void reply(string text) {
			if (!priv) {
				mqueue.PrivateNoticeEnqueue (sender, text);
			} else {
				mqueue.PublicMessageEnqueue (channel, text);
			}
		}
	}
}


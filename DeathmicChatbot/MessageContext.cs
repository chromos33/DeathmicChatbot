using System;
using Sharkbite.Irc;

namespace DeathmicChatbot
{
	public class MessageContext
	{
		private readonly string channel;
		private readonly MessageQueue mqueue;
		private readonly UserInfo sender;
		private readonly bool priv;

		public MessageContext(string channel_, MessageQueue mq, UserInfo sender_, bool private_)
		{
			channel = channel_;
			mqueue = mq;
			sender = sender_;
			priv = private_;
		}

		public void reply(string text) {
			if (priv) {
				mqueue.PrivateNoticeEnqueue (sender.Nick, text);
			} else {
				mqueue.PublicMessageEnqueue (channel, text);
			}
		}

		public void replyPrivate(string text) {
			mqueue.PrivateNoticeEnqueue(sender.Nick, text);
		}

		public bool isPrivate() {
			return priv;
		}

		public string getSenderNick() {
			return sender.Nick;
		}

		public UserInfo getSenderInfo() {
			return sender;
		}

		public string getChannel() {
			return channel;
		}
	}
}


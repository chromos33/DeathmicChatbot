#region Using

using System.Collections.Generic;
using System.Timers;
using Sharkbite.Irc;

#endregion


namespace DeathmicChatbot
{
    internal class MessageQueue
    {
        private const int MESSAGE_QUEUE_INTERVAL_MILLISECONDS = 1000;

        private static readonly Timer MessageTimer = new Timer();
        private readonly Connection _con;
        private readonly Queue<KeyValuePair<string, string>> _privateNotices =
            new Queue<KeyValuePair<string, string>>();
        private readonly Queue<KeyValuePair<string, string>> _publicMessages =
            new Queue<KeyValuePair<string, string>>();

        public MessageQueue(Connection con)
        {
            _con = con;

            MessageTimer.Interval = MESSAGE_QUEUE_INTERVAL_MILLISECONDS;
            MessageTimer.Elapsed += MessageTimerOnElapsed;
            MessageTimer.Start();
        }

        public void PublicMessageEnqueue(string sChannel, string sMessage)
        {
            _publicMessages.Enqueue(new KeyValuePair<string, string>(sChannel,
                                                                     sMessage));
        }

        public void PrivateNoticeEnqueue(string sNick, string sMessage)
        {
            _privateNotices.Enqueue(new KeyValuePair<string, string>(sNick,
                                                                     sMessage));
        }

        private KeyValuePair<string, string> PrivateNoticeDequeue() { return _privateNotices.Dequeue(); }

        private void MessageTimerOnElapsed(object sender,
                                           ElapsedEventArgs elapsedEventArgs)
        {
            KeyValuePair<string, string> msg;

            if (_publicMessages.Count > 0)
            {
                msg = _publicMessages.Dequeue();
                _con.Sender.PublicMessage(msg.Key, msg.Value);
                return;
            }

            if (_privateNotices.Count <= 0)
                return;
            msg = PrivateNoticeDequeue();
            _con.Sender.PrivateNotice(msg.Key, msg.Value);
        }
    }
}
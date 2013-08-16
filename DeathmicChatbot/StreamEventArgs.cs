using System;

namespace DeathmicChatbot
{
    public class StreamEventArgs : EventArgs
    {
        public StreamData StreamData { get; set; }

        public StreamEventArgs(StreamData streamData)
        {
            StreamData = streamData;
        }
    }
}
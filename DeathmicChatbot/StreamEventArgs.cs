using System;

namespace DeathmicChatbot
{
    public class StreamEventArgs : EventArgs
    {
        public StreamData StreamData { get; private set; }

        public StreamEventArgs(StreamData streamData)
        {
            StreamData = streamData;
        }
    }
}
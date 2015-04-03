using System;

namespace DeathmicChatbot.StreamInfo
{
    public class StreamEventArgs : EventArgs
    {
        public StreamEventArgs(StreamData streamData) { StreamData = streamData; }

        public StreamData StreamData { get; private set; }
    }
}

#region Using

using System;

#endregion


namespace DeathmicChatbot
{
    public class StreamEventArgs : EventArgs
    {
        public StreamEventArgs(StreamData streamData) { StreamData = streamData; }

        public StreamData StreamData { get; private set; }
    }
}
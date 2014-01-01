#region Using

using System;
using DeathmicChatbot.Interfaces;

#endregion


namespace DeathmicChatbot
{
    public class StreamData
    {
        public Stream Stream { get; set; }

        public IStreamProvider StreamProvider { get; set; }

        public DateTime Started { private get; set; }

        public TimeSpan TimeSinceStart { get { return DateTime.Now - Started; } }
    }
}
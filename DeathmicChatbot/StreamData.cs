using System;

namespace DeathmicChatbot
{
    public class StreamData
    {
        public Stream Stream { get; set; }

        public DateTime Started { get; set; }

        public TimeSpan TimeSinceStart
        {
            get { return DateTime.Now - Started; }
        }
    }
}
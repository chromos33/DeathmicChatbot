#region Using

using System;

#endregion


namespace DeathmicChatbot.StreamInfo.Twitch
{
    public class TwitchStreamData
    {
        public Stream Stream { get; set; }

        public DateTime Started { get; set; }

        public TimeSpan TimeSinceStart { get { return DateTime.Now - Started; } }
    }
}
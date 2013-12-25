#region Using

using System;

#endregion


namespace DeathmicChatbot.StreamInfo.Hitbox
{
    public class HitboxStreamData
    {
        public Livestream Stream { get; set; }

        public DateTime Started { get; set; }

        public TimeSpan TimeSinceStart { get { return DateTime.Now - Started; } }
    }
}
using System.Collections.Generic;

namespace DeathmicChatbot.StreamInfo.Hitbox
{
    internal class HitboxRootObject
    {
        public Request request { get; set; }
        public string media_type { get; set; }
        public List<Livestream> livestream { get; set; }
    }
}

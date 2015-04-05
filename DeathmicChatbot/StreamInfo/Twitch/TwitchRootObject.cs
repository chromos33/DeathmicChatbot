using System.Collections.Generic;
namespace DeathmicChatbot.StreamInfo.Twitch
{
    public class TwitchRootObject
    {
        public List<Stream> Streams { get; set; }
        public Links3 Links { get; set; }
    }
}

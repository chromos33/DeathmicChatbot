namespace DeathmicChatbot.StreamInfo.Twitch
{
    public class Stream
    {
        public int Viewers { get; set; }
        public long ID { get; set; }
        public Channel Channel { get; set; }
        public string Game { get; set; }
        public Preview Preview { get; set; }
        public Links2 Links { get; set; }
    }
}

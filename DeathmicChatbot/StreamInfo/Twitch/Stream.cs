namespace DeathmicChatbot.StreamInfo.Twitch
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable InconsistentNaming

    public class Stream
    {
        public int Viewers { get; set; }
        public long ID { get; set; }
        public Channel Channel { get; set; }
        public string Game { get; set; }
        public Preview Preview { get; set; }
        public Links2 Links { get; set; }
    }

    // ReSharper restore InconsistentNaming
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore UnusedMember.Global
}
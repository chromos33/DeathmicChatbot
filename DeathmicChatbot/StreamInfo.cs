using System.Collections.Generic;

namespace DeathmicChatbot
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable InconsistentNaming

    public class Links
    {
        public string Videos { get; set; }
        public string Subscriptions { get; set; }
        public string Features { get; set; }
        public string Commercial { get; set; }
        public string Follows { get; set; }
        public string Chat { get; set; }
        public string Editors { get; set; }
        public string Teams { get; set; }
        public string Self { get; set; }
        public string Stream_Key { get; set; }
    }

    public class Channel
    {
        public string Profile_Banner { get; set; }
        public string Created_At { get; set; }
        public string Updated_At { get; set; }
        public string Status { get; set; }
        public bool Mature { get; set; }
        public int ID { get; set; }
        public string Logo { get; set; }
        public string Banner { get; set; }
        public string Url { get; set; }
        public string Game { get; set; }
        public string Background { get; set; }
        public string Name { get; set; }
        public int Delay { get; set; }
        public string Display_Name { get; set; }
        public string Video_Banner { get; set; }
        public Links Links { get; set; }
    }

    public class Preview
    {
        public string Medium { get; set; }
        public string Large { get; set; }
        public string Small { get; set; }
        public string Template { get; set; }
    }

    public class Links2
    {
        public string Self { get; set; }
    }

    public class Stream
    {
        public int Viewers { get; set; }
        public long ID { get; set; }
        public Channel Channel { get; set; }
        public string Game { get; set; }
        public Preview Preview { get; set; }
        public Links2 Links { get; set; }
    }

    public class Links3
    {
        public string Featured { get; set; }
        public string Next { get; set; }
        public string Followed { get; set; }
        public string Self { get; set; }
        public string Summary { get; set; }
    }

    public class RootObject

    {
        public List<Stream> Streams { get; set; }
        public Links3 Links { get; set; }
    }

    // ReSharper restore InconsistentNaming
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore UnusedMember.Global
}
using System.Collections.Generic;

namespace DeathmicChatbot.StreamInfo
{
    public class Links
    {
        public string videos { get; set; }
        public string subscriptions { get; set; }
        public string features { get; set; }
        public string commercial { get; set; }
        public string follows { get; set; }
        public string chat { get; set; }
        public string editors { get; set; }
        public string teams { get; set; }
        public string self { get; set; }
        public string stream_key { get; set; }
    }

    public class Channel
    {
        public object profile_banner { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string status { get; set; }
        public bool mature { get; set; }
        public int _id { get; set; }
        public string logo { get; set; }
        public object banner { get; set; }
        public string url { get; set; }
        public string game { get; set; }
        public string background { get; set; }
        public string name { get; set; }
        public int delay { get; set; }
        public string display_name { get; set; }
        public string video_banner { get; set; }
        public Links _links { get; set; }
    }

    public class Preview
    {
        public string medium { get; set; }
        public string large { get; set; }
        public string small { get; set; }
        public string template { get; set; }
    }

    public class Links2
    {
        public string self { get; set; }
    }

    public class Stream
    {
        public int viewers { get; set; }
        public long _id { get; set; }
        public Channel channel { get; set; }
        public string game { get; set; }
        public Preview preview { get; set; }
        public Links2 _links { get; set; }
    }

    public class Links3
    {
        public string featured { get; set; }
        public string next { get; set; }
        public string followed { get; set; }
        public string self { get; set; }
        public string summary { get; set; }
    }

    public class RootObject
    {
        public List<Stream> streams { get; set; }
        public Links3 _links { get; set; }
    }
}

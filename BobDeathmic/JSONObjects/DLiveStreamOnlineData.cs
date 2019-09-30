using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.JSONObjects.DLive
{
    public class DLiveStreamOnlineData
    {
        public data data { get; set; }
    }
    public class data
    {
        public userByDisplayName userByDisplayName { get; set; }
    }
    public class userByDisplayName
    {
        public livestream livestream { get; set; }
    }
    public class livestream
    {
        public string id { get; set; }
        public string title { get; set; }

        public category category { get; set; }
    }
    public class category
    {
        public string title { get; set; }
    }

}

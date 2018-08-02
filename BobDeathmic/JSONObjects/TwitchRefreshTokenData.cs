using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.JSONObjects
{
    public class TwitchRefreshTokenData
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string[] scope { get; set; }
        public string error { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
}

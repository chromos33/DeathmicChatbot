using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Args
{
    public class DiscordMessageArgs
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public Models.Enum.StreamProviderTypes StreamType { get; set; }
    }
}

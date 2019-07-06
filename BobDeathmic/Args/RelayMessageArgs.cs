using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Args
{
    public class RelayMessageArgs: MessageArgs
    {
        public string SourceChannel { get; set; }
        public string TargetChannel { get; set; }
        public Models.Enum.StreamProviderTypes StreamType { get; set; }
    }
}

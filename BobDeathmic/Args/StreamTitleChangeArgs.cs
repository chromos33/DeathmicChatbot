using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Args
{
    public class StreamTitleChangeArgs
    {
        public Models.Enum.StreamProviderTypes Type { get; set; }
        public string StreamName { get; set; }
        public string Message { get; set; }
    }
}

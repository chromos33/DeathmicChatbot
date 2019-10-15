using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Args
{
    public class StrawPollRequestEventArgs
    {
        public string StreamName { get; set; }
        public string Message { get; set; }
        public Models.Enum.StreamProviderTypes Type { get; set; }
    }
}

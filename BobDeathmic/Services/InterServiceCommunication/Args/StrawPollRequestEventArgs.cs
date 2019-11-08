using BobDeathmic.Data.Enums.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Args
{
    public class StrawPollRequestEventArgs
    {
        public string StreamName { get; set; }
        public string Question { get; set; }
        public string[] Answers { get; set; }
        public bool multiple { get; set; }
        public StreamProviderTypes Type { get; set; }
    }
}

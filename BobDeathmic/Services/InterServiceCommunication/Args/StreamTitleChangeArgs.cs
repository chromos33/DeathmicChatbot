using BobDeathmic.Data.Enums.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Args
{
    public class StreamTitleChangeArgs
    {
        public StreamProviderTypes Type { get; set; }
        public string StreamName { get; set; }
        public string Message { get; set; }
        public string Game { get; set; }
        public string Title { get; set; }
    }
}

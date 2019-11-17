using BobDeathmic.Data.Enums.Stream;
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
        public StreamProviderTypes StreamType { get; set; }

        public RelayMessageArgs()
        {}
        public RelayMessageArgs(string SourceChannel,string TargetChannel,StreamProviderTypes type,string Message)
        {
            this.SourceChannel = SourceChannel;
            this.TargetChannel = TargetChannel;
            this.StreamType = type;
            this.Message = Message;
        }
    }
}

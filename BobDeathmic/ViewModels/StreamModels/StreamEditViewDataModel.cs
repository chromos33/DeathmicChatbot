using BobDeathmic.Data.DBModels.Relay;
using BobDeathmic.Data.DBModels.StreamModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.StreamModels
{
    public class StreamEditViewDataModel
    {
        public Stream stream { get; set; }
        public List<string> StreamTypes { get; set; }
        public List<RelayChannels> RelayChannels { get; set; }
        public string SelectedRelayChannel { get; set; }
        public string StatusMessage { get; set; }
    }
}

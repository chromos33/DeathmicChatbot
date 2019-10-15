using BobDeathmic.Models.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.StreamModels
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

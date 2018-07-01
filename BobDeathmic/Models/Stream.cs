using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Models.Enum;

namespace BobDeathmic.Models
{
    public class Stream
    {
        public int ID { get; set; }
        public string StreamName { get; set; }
        public string Game { get; set; }
        public DateTime Started { get; set; }
        public DateTime Stopped { get; set; }
        public RelayState RelayState { get; set; }
        public StreamState StreamState { get; set; }
        public string DiscordRelayChannel { get; set; }
        public List<StreamProvider> StreamProvider { get; set; }
    }
}

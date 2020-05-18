using BobDeathmic.Data.DBModels.StreamModels;
using Jurassic.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Other
{
    public class StreamEditData
    {
        public string StreamName { get; set; }
        public string Type { get; set; }
        public int UpTime { get; set; }
        public int Quote { get; set; }
        public string RelayChannel { get; set; }
        public string[] RelayChannels { get; set; }
        public StreamEditData(Stream stream,List<string> channels)
        {
            StreamName = stream.StreamName;
            Type = stream.Type.ToString();
            UpTime = stream.UpTimeInterval;
            Quote = stream.QuoteInterval;
            List<string> tmpchannels = new List<string>();
            tmpchannels.Add("Aus");
            tmpchannels.Add("An");
            tmpchannels.AddRange(channels);
            RelayChannel = stream.DiscordRelayChannel;
            
            RelayChannels = tmpchannels.ToArray();
        }   
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

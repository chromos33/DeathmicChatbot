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
        
        public string UserID { get; set; }
        public string Url { get; set; }
        public StreamProviderTypes Type { get; set; }
        public string AccessToken { get; set; }
        public string Secret { get; set; }
        public string ClientID { get; set; }

        public DateTime Started { get; set; }
        public DateTime Stopped { get; set; }
        public RelayState RelayState { get; set; }
        public StreamState StreamState { get; set; }
        public string DiscordRelayChannel { get; set; }
        public List<StreamSubscription> StreamSubscriptions { get; set; }
        public ChatUserModel Owner { get; set; }
        public List<string> EnumStreamTypes()
        {
            List<string> EnumTypes = new List<string>();

            EnumTypes.Add("Twitch");
            EnumTypes.Add("Mixer");

            return EnumTypes;
        }
        public string StreamStartedMessage()
        {
            string message = "";

            message = $"{StreamName} hat angefangen {Game} auf {Url} zu streamen";

            return message;
        }
    }
}

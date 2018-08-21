using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Models.Enum;
using TwitchLib.Api.Models.Helix.Streams.GetStreams;

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
        public string RefreshToken { get; set; }

        public DateTime Started { get; set; }
        public DateTime Stopped { get; set; }
        
        public StreamState StreamState { get; set; }
        public string DiscordRelayChannel { get; set; }
        public List<StreamSubscription> StreamSubscriptions { get; set; }
        public ChatUserModel Owner { get; set; }
        public int UpTimeInterval { get; set; }
        public DateTime LastUpTime { get; set; }
        public List<StreamCommand> Commands { get; set; }

        public List<string> EnumStreamTypes()
        {
            List<string> EnumTypes = new List<string>();

            EnumTypes.Add("Twitch");
            EnumTypes.Add("Mixer");

            return EnumTypes;
        }
        public static List<string> StaticEnumStreamTypes()
        {
            List<string> EnumTypes = new List<string>();

            EnumTypes.Add("Twitch");
            EnumTypes.Add("Mixer");

            return EnumTypes;
        }
        public string StreamStartedMessage(TwitchLib.Api.Models.Helix.Streams.GetStreams.Stream data)
        {
            string message = "";

            message = $"{StreamName} hat angefangen {data.Title} auf {Url} zu streamen.";
            if(Type == StreamProviderTypes.Twitch && DiscordRelayChannel != null && DiscordRelayChannel != "" && DiscordRelayChannel != "An" && DiscordRelayChannel != "Aus")
            {
                message += $" Sein Relay befindet sich in Channel {DiscordRelayChannel}";
            }

            return message;
        }
        public RelayState RelayState()
        {
            if(DiscordRelayChannel != "Aus" && DiscordRelayChannel != "")
            {
                return Enum.RelayState.Activated;
            }
            return Enum.RelayState.NotActivated;
        }
    }
}

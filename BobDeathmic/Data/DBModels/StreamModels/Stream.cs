using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Data.Enums.Relay;
using BobDeathmic.Data.Enums.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Data.DBModels.StreamModels
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
        public DateTime LastNotice { get; set; }

        public StreamState StreamState { get; set; }
        public string DiscordRelayChannel { get; set; }
        public List<StreamSubscription> StreamSubscriptions { get; set; }
        public ChatUserModel Owner { get; set; }
        public int UpTimeInterval { get; set; }
        public DateTime LastUpTime { get; set; }
        public int QuoteInterval { get; set; }
        public DateTime LastRandomQuote { get; set; }
        public List<StreamCommand> Commands { get; set; }

        public Stream()
        {

        }
        public Stream(string name)
        {
            StreamName = name;
        }


        public String[] GetActiveSubscribers()
        {
            //If you read this you probably forgot to "Include" either StreamSubscriptions or User in your EF Query
            return StreamSubscriptions.Where(x => x.Subscribed == SubscriptionState.Subscribed).Select(x => x.User.ChatUserName).ToArray();
        }
        public List<string> EnumStreamTypes()
        {
            List<string> EnumTypes = new List<string>();

            EnumTypes.Add("Twitch");
            EnumTypes.Add("Mixer");
            EnumTypes.Add("DLive");

            return EnumTypes;
        }
        public static List<string> StaticEnumStreamTypes()
        {
            List<string> EnumTypes = new List<string>();

            EnumTypes.Add("Twitch");
            EnumTypes.Add("Mixer");
            EnumTypes.Add("DLive");

            return EnumTypes;
        }
        public string UptimeMessage()
        {
            LastUpTime = DateTime.Now;
            return $"Stream läuft seit {DateTime.Now.Subtract(Started).Hours} Stunden und {DateTime.Now.Subtract(Started).Minutes} Minuten";
        }
        public bool UpTimeQueued()
        {
            return UpTimeInterval != 0 && LastUpTime.Add(new TimeSpan(0, UpTimeInterval, 0)) < DateTime.Now;
        }
        public string StreamStartedMessage(string title,string url = "")
        {
            string message = "";

            message = $"{StreamName} hat angefangen {title} auf {Url} zu streamen.";
            if (Type == StreamProviderTypes.Twitch && DiscordRelayChannel != null && DiscordRelayChannel != "" && DiscordRelayChannel != "An" && DiscordRelayChannel != "Aus")
            {
                message += $" Relay befindet sich in Channel {DiscordRelayChannel}";
            }
            return message;
        }
        public RelayState RelayState()
        {
            if (DiscordRelayChannel != "Aus" && DiscordRelayChannel != "")
            {
                return Enums.Relay.RelayState.Activated;
            }
            return Enums.Relay.RelayState.NotActivated;
        }
    }
}

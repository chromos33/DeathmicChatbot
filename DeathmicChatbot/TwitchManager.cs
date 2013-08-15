using DeathmicChatbot.StreamInfo;
using RestSharp;
using RestSharp.Deserializers;
using System.Collections.Generic;

namespace DeathmicChatbot
{
    public class TwitchManager
    {
        private List<string> streams;
        private RestClient client;

        public TwitchManager()
	    {
            this.client = new RestClient("https://api.twitch.tv");
            this.streams = new List<string>();
	    }

        public RootObject getOnlineStreams()
        {
            RestRequest req = new RestRequest("/kraken/streams", Method.GET);
            req.AddParameter("channel", arrayToString(streams));

            IRestResponse response = client.Execute(req);

            JsonDeserializer des = new JsonDeserializer();
            RootObject data = des.Deserialize<RootObject>(response);
            return data;
        }

        public void addStream(string stream)
        {
            this.streams.Add(stream);
        }

        public void removeStream(string stream)
        {
            this.streams.Remove(stream);
        }

        private string arrayToString(List<string> arr)
        {
            return string.Join(",", arr);
        }
    }
}
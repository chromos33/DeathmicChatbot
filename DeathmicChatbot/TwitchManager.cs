using DeathmicChatbot.StreamInfo;
using RestSharp;
using RestSharp.Deserializers;
using System.Collections.Generic;

namespace DeathmicChatbot
{
    public class TwitchManager
    {
        private readonly List<string> _streams;
        private readonly RestClient _client;

        public TwitchManager()
        {
            _client = new RestClient("https://api.twitch.tv");
            _streams = new List<string>();
        }

        public RootObject GetOnlineStreams()
        {
            RestRequest req = new RestRequest("/kraken/streams", Method.GET);
            req.AddParameter("channel", ArrayToString(_streams));

            IRestResponse response = _client.Execute(req);

            JsonDeserializer des = new JsonDeserializer();
            RootObject data = des.Deserialize<RootObject>(response);
            return data;
        }

        public void AddStream(string stream)
        {
            _streams.Add(stream);
        }

        public void RemoveStream(string stream)
        {
            _streams.Remove(stream);
        }

        private static string ArrayToString(IEnumerable<string> arr)
        {
            return string.Join(",", arr);
        }
    }
}
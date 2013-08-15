using System;
using System.IO;
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

        private const string STREAMS_FILE = "streams.txt";

        public TwitchManager()
        {
            _client = new RestClient("https://api.twitch.tv");
            _streams = new List<string>();

            LoadStreams();
        }

        private void LoadStreams()
        {
            if (!File.Exists(STREAMS_FILE)) File.Create(STREAMS_FILE).Close();
            StreamReader reader = new StreamReader(STREAMS_FILE);
            while (!reader.EndOfStream)
            {
                string sLine = reader.ReadLine();
                if (!_streams.Contains(sLine)) _streams.Add(sLine);
            }
            reader.Close();
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
            if (!_streams.Contains((stream))) _streams.Add(stream);
            WriteStreamsToFile();
        }

        public void RemoveStream(string stream)
        {
            _streams.Remove(stream);
            WriteStreamsToFile();
        }

        private void WriteStreamsToFile()
        {
            StreamWriter writer = new StreamWriter(STREAMS_FILE, false);
            foreach (string stream in _streams)
            {
                writer.WriteLine(stream);
            }
            writer.Close();
        }

        private static string ArrayToString(IEnumerable<string> arr)
        {
            return string.Join(",", arr);
        }
    }
}
using System;
using System.IO;
using System.Linq;
using DeathmicChatbot.StreamInfo;
using RestSharp;
using RestSharp.Deserializers;
using System.Collections.Generic;
using RestSharp.Serializers;
using Stream = DeathmicChatbot.StreamInfo.Stream;

namespace DeathmicChatbot
{
    public class TwitchManager
    {
        private readonly List<string> _streams;
        private readonly RestClient _client;
        public Dictionary<string, StreamData> _streamData = new Dictionary<string, StreamData>();

        private const string STREAMS_FILE = "streams.txt";
        private const string STREAMDATA_FILE = "streamdata.txt";

        public event EventHandler<StreamEventArgs> StreamStarted;
        public event EventHandler<StreamEventArgs> StreamStopped;

        public TwitchManager()
        {
            _client = new RestClient("https://api.twitch.tv");
            _streams = new List<string>();

            LoadStreams();
            LoadStreamData();
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

        private void LoadStreamData()
        {
            if (!File.Exists(STREAMDATA_FILE)) File.Create(STREAMDATA_FILE).Close();
            StreamReader reader = new StreamReader(STREAMDATA_FILE);

            JsonDeserializer deserializer = new JsonDeserializer();

            while (!reader.EndOfStream)
            {
                string sLine = reader.ReadLine();

                RestResponse response = new RestResponse {Content = sLine};

                StreamData streamData = deserializer.Deserialize<StreamData>(response);

                if (!_streamData.ContainsKey(streamData.Stream.Channel.Name))
                    _streamData.Add(streamData.Stream.Channel.Name, streamData);
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

        public bool AddStream(string stream)
        {
            if (!_streams.Contains(stream))
            {
                _streams.Add(stream);
                WriteStreamsToFile();
                return true;
            }
            return false;
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

        public void CheckStreams()
        {
            RootObject obj = GetOnlineStreams();

            Dictionary<string, StreamData> runningStreams = new Dictionary<string, StreamData>();

            foreach (Stream stream in obj.Streams)
            {
                runningStreams.Add(stream.Channel.Name, new StreamData {Stream = stream, Started = DateTime.Now});
                if (!_streamData.ContainsKey(stream.Channel.Name) && StreamStarted != null)
                    StreamStarted(this, new StreamEventArgs(new StreamData {Stream = stream, Started = DateTime.Now}));
            }

            foreach (
                KeyValuePair<string, StreamData> pair in
                    _streamData.Where(pair => !runningStreams.ContainsKey(pair.Key) && StreamStopped != null))
            {
                StreamStopped(this, new StreamEventArgs(pair.Value));
            }

            _streamData = runningStreams;

            WriteStreamDataToFile();
        }

        private void WriteStreamDataToFile()
        {
            JsonSerializer serializer = new JsonSerializer();
            StreamWriter writer = new StreamWriter(STREAMDATA_FILE, false);

            foreach (KeyValuePair<string, StreamData> pair in _streamData)
            {
                writer.WriteLine(serializer.Serialize(pair.Value));
            }

            writer.Close();
        }
    }
}
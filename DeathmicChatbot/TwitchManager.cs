using System;
using System.IO;
using System.Linq;
using DeathmicChatbot.StreamInfo;
using RestSharp;
using RestSharp.Deserializers;
using System.Collections.Generic;
using RestSharp.Serializers;
using Stream = DeathmicChatbot.StreamInfo.Stream;
using System.Collections.Concurrent;

namespace DeathmicChatbot
{
    public class TwitchManager
    {
        private readonly List<string> _streams;
        private readonly RestClient _client;
        public ConcurrentDictionary<string, StreamData> _streamData = new ConcurrentDictionary<string, StreamData>();

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
                    _streamData.TryAdd(streamData.Stream.Channel.Name, streamData);
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
            stream = stream.ToLower();
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
            stream = stream.ToLower();
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
            // Get all live streams from server
            RootObject obj = GetOnlineStreams();

            // Remove streams that have stopped
            foreach (KeyValuePair<string, StreamData> pair in from pair in _streamData
                                                              let bFound =
                                                                  obj.Streams.Any(
                                                                      stream => pair.Key == stream.Channel.Name)
                                                              where !bFound && StreamStopped != null
                                                              select pair)
            {
                StreamData sd;
                _streamData.TryRemove(pair.Key,out sd);
                StreamStopped(this, new StreamEventArgs(pair.Value));
            }

            // Add new streams that have started
            foreach (Stream stream in obj.Streams.Where(stream => !_streamData.ContainsKey(stream.Channel.Name)))
            {
                _streamData.TryAdd(stream.Channel.Name, new StreamData {Started = DateTime.Now, Stream = stream});
                if (StreamStarted != null) StreamStarted(this, new StreamEventArgs(_streamData[stream.Channel.Name]));
            }

            // Write all running streams to file
            WriteStreamDataToFile();
        }

        public List<String> GetStreamInfoArray()
        {
            List<String> data = new List<String>();
            foreach (StreamData stream in this._streamData.Values)
            {
                String info = String.Format(
                        "{0} is streaming! ===== Game: {1} ===== Message: {2} ===== Started: {3:t} o'clock ({4:HH}:{4:mm} ago) ===== Link: http://www.twitch.tv/{0}",
                        stream.Stream.Channel.Name,
                        stream.Stream.Channel.Game,
                        stream.Stream.Channel.Status,
                        stream.Started,
                        new DateTime(stream.TimeSinceStart.Ticks)
                );
                data.Add(info);
            }
            return data;
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

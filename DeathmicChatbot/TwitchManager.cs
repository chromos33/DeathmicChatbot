using System;
using System.IO;
using System.Linq;
using RestSharp;
using RestSharp.Deserializers;
using System.Collections.Generic;
using RestSharp.Serializers;
using System.Collections.Concurrent;

namespace DeathmicChatbot
{
    public class TwitchManager
    {
        private readonly List<string> _streams;
        private readonly RestClient _client;

        public readonly ConcurrentDictionary<string, StreamData> _streamData =
            new ConcurrentDictionary<string, StreamData>();

        private const string STREAMS_FILE = "streams.txt";
        private const string STREAMDATA_FILE = "streamdata.txt";

        private RootObject _lastroot;

        public event EventHandler<StreamEventArgs> StreamStarted;
        public event EventHandler<StreamEventArgs> StreamStopped;

        private readonly LogManager _log;

        private readonly bool _bDebugMode;

        public TwitchManager(LogManager log, bool bDebugMode = false)
        {
            _log = log;
            _bDebugMode = bDebugMode;
            _client = new RestClient("https://api.twitch.tv");
            _streams = new List<string>();

            LoadStreams();
            LoadStreamData();
        }

        private void LoadStreams()
        {
            if (!File.Exists(STREAMS_FILE)) File.Create(STREAMS_FILE).Close();
            var reader = new StreamReader(STREAMS_FILE);

            while (!reader.EndOfStream)
            {
                var sLine = reader.ReadLine();
                if (_streams.Contains(sLine)) continue;
                _streams.Add(sLine);
                _log.WriteToLog("Information", string.Format("Added stream '{0}' from saved streams file to list.", sLine));
            }
            reader.Close();
        }

        private void LoadStreamData()
        {
            if (!File.Exists(STREAMDATA_FILE)) File.Create(STREAMDATA_FILE).Close();
            var reader = new StreamReader(STREAMDATA_FILE);

            var deserializer = new JsonDeserializer();

            while (!reader.EndOfStream)
            {
                var sLine = reader.ReadLine();

                var response = new RestResponse {Content = sLine};

                var streamData = deserializer.Deserialize<StreamData>(response);

                if (!_streamData.ContainsKey(streamData.Stream.Channel.Name))
                    _streamData.TryAdd(streamData.Stream.Channel.Name, streamData);
            }
            reader.Close();
        }

        private RootObject GetOnlineStreams()
        {
            var req = new RestRequest("/kraken/streams", Method.GET);
            req.AddParameter("channel", ArrayToString(_streams));

            var response = _client.Execute(req);

            if (_bDebugMode) _log.WriteToLog("Debug", string.Format("Got Response from Twitch: {0}",response.Content));

            try
            {
                var des = new JsonDeserializer();
                var data = des.Deserialize<RootObject>(response);
                _lastroot = data;
                return data;
            }
            catch (Exception ex)
            {
                _log.WriteToLog("CaughtException", string.Format("Returning last stream state due to exception: {0}",ex.Message));
                return _lastroot;
            }
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
            var writer = new StreamWriter(STREAMS_FILE, false);
            foreach (var stream in _streams)
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
            var obj = GetOnlineStreams();

            // If querying Twitch for running streams always fails (maybe Twitch
            // is down or the server running the bot doesn't have the necessary
            // SSL certificates installed), this prevents the bot from crashing.
            if (obj == null) return;

            RemoveStoppedStreams(obj);

            AddNewlyStartedStreams(obj);

            WriteStreamDataToFile();
        }

        private void AddNewlyStartedStreams(RootObject obj)
        {
            foreach (
                var stream in
                    obj.Streams.Where(
                        stream => !_streamData.ContainsKey(stream.Channel.Name)))
            {
                _streamData.TryAdd(stream.Channel.Name,
                                   new StreamData
                                   {
                                       Started = DateTime.Now,
                                       Stream = stream
                                   });
                if (StreamStarted != null)
                    StreamStarted(this,
                                  new StreamEventArgs(_streamData[stream.Channel.Name]));
            }
        }

        private void RemoveStoppedStreams(RootObject obj)
        {
            foreach (var pair in from pair in _streamData
                                 let bFound =
                                     obj.Streams.Any(
                                         stream => pair.Key == stream.Channel.Name)
                                 where !bFound && StreamStopped != null
                                 select pair)
            {
                StreamData sd;
                _streamData.TryRemove(pair.Key, out sd);
                StreamStopped(this, new StreamEventArgs(pair.Value));
            }
        }

        public IEnumerable<string> GetStreamInfoArray()
        {
            return
                _streamData.Values.Select(
                    stream =>
                    String.Format(
                        "{0} is streaming! ===== Game: {1} ===== Message: {2} ===== Started: {3:t} o'clock ({4:HH}:{4:mm} ago) ===== Link: http://www.twitch.tv/{0}",
                        stream.Stream.Channel.Name, stream.Stream.Channel.Game, stream.Stream.Channel.Status,
                        stream.Started, new DateTime(stream.TimeSinceStart.Ticks))).ToList();
        }

        private void WriteStreamDataToFile()
        {
            var serializer = new JsonSerializer();
            var writer = new StreamWriter(STREAMDATA_FILE, false);

            foreach (var pair in _streamData)
            {
                writer.WriteLine(serializer.Serialize(pair.Value));
            }

            writer.Close();
        }
    }
}
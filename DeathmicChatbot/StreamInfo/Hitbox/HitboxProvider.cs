#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using Timer = System.Timers.Timer;

#endregion


namespace DeathmicChatbot.StreamInfo.Hitbox
{
    internal class HitboxProvider : IStreamProvider
    {
        private const string STREAMS_FILE = "streams_hitbox.txt";
        private const string STREAMDATA_FILE = "streamdata_hitbox.txt";
        private const int TIME_MS_HITBOX_QUERY_THREAD_SLEEP = 500;
        private readonly RestClient _client;

        private readonly bool _debugMode;
        private readonly Dictionary<string, HitboxRootObject> _lastRequests =
            new Dictionary<string, HitboxRootObject>();
        private readonly LogManager _log;

        private readonly ConcurrentDictionary<string, HitboxStreamData>
            _streamData = new ConcurrentDictionary<string, HitboxStreamData>();
        private readonly List<string> _streams = new List<string>();

        private readonly Queue<string> _streamsToCheck = new Queue<string>();

        public HitboxProvider(LogManager log, bool debugMode)
        {
            _log = log;
            _debugMode = debugMode;

            _client = new RestClient("http://api.hitbox.tv");

            LoadStreams();
            LoadStreamData();

            var hitboxQueryThread = new Thread(QueryHitboxForQueuedStreamTimed);
            hitboxQueryThread.Start();
        }

        #region IStreamProvider Members

        public event EventHandler<StreamEventArgs> StreamStarted;
        public event EventHandler<StreamEventArgs> StreamStopped;

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

        public void CheckStreams()
        {
            foreach (var stream in
                _streams.Where(stream => !_streamsToCheck.Contains(stream)))
                _streamsToCheck.Enqueue(stream);
        }

        public IEnumerable<string> GetStreamInfoArray()
        {
            return
                _streamData.Values.Select(
                    stream =>
                    String.Format(
                        "{0} is streaming! ===== Game: {1} ===== Message: {2} ===== Started: {3:t} o'clock ({4:HH}:{4:mm} ago) ===== Link: {5}/{0}",
                        stream.Stream.media_user_name,
                        stream.Stream.category_name,
                        stream.Stream.media_status,
                        stream.Started,
                        new DateTime(stream.TimeSinceStart.Ticks),
                        GetLink())).ToList();
        }

        public string GetLink() { return "http://www.hitbox.tv"; }

        #endregion

        private void QueryHitboxForQueuedStreamTimed()
        {
            var timer = new Timer(TIME_MS_HITBOX_QUERY_THREAD_SLEEP);
            timer.Elapsed += (sender, args) => QueryHitboxForQueuedStream();
            timer.Start();
        }

        private void QueryHitboxForQueuedStream()
        {
            WriteStreamDataToFile();

            Thread.Sleep(TIME_MS_HITBOX_QUERY_THREAD_SLEEP);

            if (_streamsToCheck.Count <= 0)
                return;

            var stream = _streamsToCheck.Dequeue();
            var result = CheckStreamOnlineStatus(stream);

            if (result == null || result.livestream.Count != 1)
                return;

            if (result.livestream[0].media_is_live == "1")
            {
                if (!_streamData.ContainsKey(stream))
                {
                    var hitboxStreamData = new HitboxStreamData
                    {
                        Started = DateTime.Now,
                        Stream = result.livestream[0]
                    };

                    var bTryAddResult = _streamData.TryAdd(stream,
                                                           hitboxStreamData);

                    if (bTryAddResult && StreamStarted != null)
                    {
                        var stream1 = new Stream
                        {
                            Channel = hitboxStreamData.Stream.media_user_name,
                            Game =
                                hitboxStreamData.Stream.category_name ??
                                "<no game>",
                            Message = hitboxStreamData.Stream.media_status,
                        };

                        var streamData = new StreamData
                        {
                            Started = DateTime.Now,
                            Stream = stream1,
                            StreamProvider = this
                        };

                        var streamEventArgs = new StreamEventArgs(streamData);
                        StreamStarted(this, streamEventArgs);
                    }
                }
            }
            else
            {
                if (_streamData.ContainsKey(stream))
                {
                    HitboxStreamData hitboxStreamData;

                    var bTryRemoveResult = _streamData.TryRemove(stream,
                                                                 out
                                                                     hitboxStreamData);
                    if (bTryRemoveResult && StreamStopped != null)
                    {
                        var stream1 = new Stream
                        {
                            Channel = hitboxStreamData.Stream.media_user_name,
                            Game = hitboxStreamData.Stream.category_name,
                            Message = hitboxStreamData.Stream.media_status
                        };

                        var streamData = new StreamData
                        {
                            Started = DateTime.Now,
                            Stream = stream1,
                            StreamProvider = this
                        };

                        var streamEventArgs = new StreamEventArgs(streamData);

                        StreamStopped(this, streamEventArgs);
                    }
                }
            }
        }

        private void WriteStreamDataToFile()
        {
            var serializer = new JsonSerializer();
            var writer = new StreamWriter(STREAMDATA_FILE, false);

            foreach (var pair in _streamData)
                writer.WriteLine(serializer.Serialize(pair.Value));

            writer.Close();
        }

        private HitboxRootObject CheckStreamOnlineStatus(string sStream)
        {
            var req = new RestRequest("/media/live/" + sStream, Method.GET);

            var response = _client.Execute(req);

            WriteDebugInfoIfDebugMode(response);

            try
            {
                var des = new JsonDeserializer();
                var data = des.Deserialize<HitboxRootObject>(response);

                if (_lastRequests.ContainsKey(sStream))
                    _lastRequests[sStream] = data;
                else
                    _lastRequests.Add(sStream, data);

                return data;
            }
            catch (Exception ex)
            {
                _log.WriteToLog("CaughtException",
                                string.Format(
                                    "Returning last stream state due to exception: {0}",
                                    ex.Message));

                if (response.Content == "no_media_found")
                {
                    _log.WriteToLog("Info",
                                    string.Format(
                                        "Removing stream '{0}': Not found on hitbox.",
                                        sStream));
                    _streams.Remove(sStream);
                }

                return _lastRequests.ContainsKey(sStream)
                           ? _lastRequests[sStream]
                           : null;
            }
        }

        private void WriteStreamsToFile()
        {
            var writer = new StreamWriter(STREAMS_FILE, false);
            foreach (var stream in _streams)
                writer.WriteLine(stream);
            writer.Close();
        }

        private void WriteDebugInfoIfDebugMode(IRestResponse response)
        {
            if (_debugMode)
            {
                _log.WriteToLog("Debug",
                                string.Format("Got Response from Hitbox: {0}",
                                              response.Content));
            }
        }

        private void LoadStreams()
        {
            if (!File.Exists(STREAMS_FILE))
                File.Create(STREAMS_FILE).Close();
            var reader = new StreamReader(STREAMS_FILE);

            while (!reader.EndOfStream)
            {
                var sLine = reader.ReadLine();
                if (_streams.Contains(sLine))
                    continue;
                _streams.Add(sLine);
                _log.WriteToLog("Information",
                                string.Format(
                                    "Added stream '{0}' from saved streams file to list.",
                                    sLine));
            }
            reader.Close();
        }

        private void LoadStreamData()
        {
            if (!File.Exists(STREAMDATA_FILE))
                File.Create(STREAMDATA_FILE).Close();
            var reader = new StreamReader(STREAMDATA_FILE);

            var deserializer = new JsonDeserializer();

            while (!reader.EndOfStream)
            {
                var sLine = reader.ReadLine();

                var response = new RestResponse
                {
                    Content = sLine
                };

                var streamData =
                    deserializer.Deserialize<HitboxStreamData>(response);

                if (!_streamData.ContainsKey(streamData.Stream.media_user_name))
                    _streamData.TryAdd(streamData.Stream.media_user_name,
                                       streamData);
            }
            reader.Close();
        }
    }
}
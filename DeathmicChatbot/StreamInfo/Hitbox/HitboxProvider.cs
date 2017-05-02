using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using DeathmicChatbot.Interfaces;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using DeathmicChatbot.StreamInfo;

namespace DeathmicChatbot.StreamInfo.Hitbox
{
    public sealed class HitboxProvider : IStreamProvider, IDisposable
    {
        private const string STREAMDATA_FILE = "streamdata_hitbox.txt";
        private const int TIME_MS_HITBOX_QUERY_THREAD_SLEEP = 5000;
        private XMLProvider xmlprovider;

        private readonly Dictionary<string, HitboxRootObject> _lastRequests =
            new Dictionary<string, HitboxRootObject>();
        private readonly IRestClientProvider _restClientProvider;

        private readonly ConcurrentDictionary<string, HitboxStreamData>
            _streamData = new ConcurrentDictionary<string, HitboxStreamData>();
        private readonly List<string> _streams = new List<string>();

        private readonly Queue<string> _streamsToCheck = new Queue<string>();
        private Timer _timer;
        

        public HitboxProvider()
        {
            _restClientProvider = new RestClientProvider(new RestClient("http://api.hitbox.tv"));

            LoadStreams();
            //LoadStreamData();
        }
        public void StartTimer()
        {
            _timer = new Timer(TIME_MS_HITBOX_QUERY_THREAD_SLEEP);
            _timer.Elapsed += (sender, args) => QueryHitboxForQueuedStream();
            _timer.Start();
        }

        #region IDisposable Members

        public void Dispose() { _timer.Stop(); }

        #endregion

        #region IStreamProvider Members

        public event EventHandler<StreamEventArgs> StreamStarted;
        public event EventHandler<StreamEventArgs> StreamStopped;
        public event EventHandler<StreamEventArgs> StreamGlobalNotification;

        public bool AddStream(string stream)
        {
            stream = stream.ToLower();
            if (!_streams.Contains(stream))
            {
                _streams.Add(stream);

                if (_streams.Count == 1)
                {
                    _streamsToCheck.Enqueue(stream);
                    QueryHitboxForQueuedStream();
                }

                return true;
            }
            return false;
        }


        public void RemoveStream(string stream)
        {
            stream = stream.ToLower();
            _streams.Remove(stream);
        }

        public void CheckStreams()
        {
            foreach (var stream in
                _streams.Where(stream => !_streamsToCheck.Contains(stream)))
                _streamsToCheck.Enqueue(stream);
        }

        public List<string> GetStreamInfoArray()
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

        private void QueryHitboxForQueuedStream()
        {
                if (_streamsToCheck.Count == 0)
                    return;
                if (StreamStarted == null)
                {
                    return;
                }
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
                        bool globalancounce = false;
                        if (_streamData.Keys.Contains(stream))
                        {
                            globalancounce = true;
                        }
                        var bTryAddResult = _streamData.TryAdd(stream,
                                                               hitboxStreamData);

                        if (bTryAddResult)
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
                            xmlprovider.AddStreamLivedata(hitboxStreamData.Stream.media_user_name, "http://www.hitbox.tv/" + hitboxStreamData.Stream.media_user_name.ToString(),hitboxStreamData.Stream.category_name);

                            var streamEventArgs = new StreamEventArgs(streamData);
                            StreamStarted(this, streamEventArgs);
                            StreamGlobalNotification(this, streamEventArgs);
                        }
                        if (!bTryAddResult && globalancounce)
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
                            StreamGlobalNotification(this, streamEventArgs);
                        }
                    }
                    else
                    {
                        var hitboxStreamData = new HitboxStreamData
                        {
                            Started = DateTime.Now,
                            Stream = result.livestream[0]
                        };
                        bool globalancounce = false;
                        if (_streamData.Keys.Contains(stream))
                        {
                            globalancounce = true;
                        }
                        var bTryAddResult = _streamData.TryAdd(stream,
                                                               hitboxStreamData);
                        if (!bTryAddResult && globalancounce)
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
                            StreamGlobalNotification(this, streamEventArgs);
                        }
                    }
                    //WriteStreamDataToFile();
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
            // If no data gets returned then stream is hidden
            var req = new RestRequest("/media/live/" + sStream, Method.GET);
            var response = _restClientProvider.Execute(req);
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
            catch (Exception)
            {
                

                if (response.Content == "no_media_found")
                {
                    _streams.Remove(sStream);
                }

                return _lastRequests.ContainsKey(sStream)
                           ? _lastRequests[sStream]
                           : null;
            }
        }

        private void LoadStreams()
        {
            
            if (xmlprovider == null) { xmlprovider = new XMLProvider(); }

            string[] streamlist = xmlprovider.StreamList("hitbox",true).Split(',');

            foreach (string item in streamlist)
            {
                if(!_streams.Contains(item))
                {
                    AddStream(item);
                }
            }
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

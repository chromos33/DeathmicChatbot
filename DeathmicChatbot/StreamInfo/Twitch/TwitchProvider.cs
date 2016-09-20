using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeathmicChatbot.Interfaces;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System.Net;
using System.Net.Security;

namespace DeathmicChatbot.StreamInfo.Twitch
{
    public class TwitchProvider : IStreamProvider
    {
        private readonly RestClient _client;
        private const string STREAMDATA_FILE = "streamdata_twitch.txt";
        private readonly ConcurrentDictionary<string, TwitchStreamData>
            _streamData = new ConcurrentDictionary<string, TwitchStreamData>();
        private readonly StreamStopCounter _streamStopCounter =
            new StreamStopCounter();
        private readonly List<string> _streams;

        private TwitchRootObject _lastroot;
        private XMLProvider xmlprovider;

        public TwitchProvider()
        {
            _client = new RestClient("https://api.twitch.tv");
            _streams = new List<string>();

            LoadStreams();
            //LoadStreamData();
        }
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
                return true;
            }
            return false;
        }

        public void RemoveStream(string stream)
        {
            _streams.Remove(stream.ToLower());
        }

        public void CheckStreams()
        {
            // Get all live streams from server
            var obj = GetOnlineStreams();

            // If querying Twitch for running streams always fails (maybe Twitch
            // is down or the server running the bot doesn't have the necessary
            // SSL certificates installed), this prevents the bot from crashing.
            if (obj == null)
                return;
            RemoveStoppedStreams(obj);
            AddNewlyStartedStreams(obj);
            
            //WriteStreamDataToFile();
        }

        public List<string> GetStreamInfoArray()
        {
            List<string> streaminfoarray = new List<string>();
            foreach (var stream in _streamData.Values)
            {

                try
                {
                    if (Convert.ToBoolean(xmlprovider.StreamInfo(stream.Stream.Channel.Name, "running")))
                    {
                        string name = stream.Stream.Channel.Name.ToString();
                        string Game = xmlprovider.StreamInfo(stream.Stream.Channel.Name, "game");
                        string started = xmlprovider.StreamInfo(stream.Stream.Channel.Name, "starttime");
                        DateTime _started = Convert.ToDateTime(started);
                        TimeSpan duration = DateTime.Now.Subtract(_started);

                        streaminfoarray.Add(
                        String.Format(
                            "{0} is streaming! ===== Game: {1} ===== Message: {2} ===== Started: {3:t} o'clock ({4} ago) ===== Link: {5}",
                            name,
                            Game,
                            stream.Stream.Channel.Status,
                            started,
                            duration.ToString(@"hh\:mm"),
                            stream.Stream.Channel.Url.ToString()));
                    }
                }
                catch (FormatException)
                {

                }

            }
            return streaminfoarray;
        }
        public void AddStreamdatatoXML(TwitchStreamData stream)
        {
            try
            {
                xmlprovider.AddStreamdata("twitch", stream);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        public string GetLink() { return "http://www.twitch.tv"; }

        #endregion

        private void LoadStreams()
        {

            if (xmlprovider == null) { xmlprovider = new XMLProvider(); }

            string[] streamlist = xmlprovider.StreamList("twitch").Split(',');
            foreach (string item in streamlist)
            {
                if (!_streams.Contains(item))
                {
                    _streams.Add(item);
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
                    deserializer.Deserialize<TwitchStreamData>(response);

                if (!_streamData.ContainsKey(streamData.Stream.Channel.Name))
                    _streamData.TryAdd(streamData.Stream.Channel.Name,
                                       streamData);
            }
            reader.Close();        }

        private TwitchRootObject GetOnlineStreams()
        {
            var req = new RestRequest("/kraken/streams", Method.GET);
            req.AddHeader("Client-ID", Properties.Settings.Default.TwitchclientID.ToString());
            req.AddParameter("channel", ArrayToString(_streams));
            var response = _client.Execute(req);
            try
            {
                var des = new JsonDeserializer();
                var data = des.Deserialize<TwitchRootObject>(response);
                
                
                _lastroot = data;
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return _lastroot;
            }
        }

        private static string ArrayToString(IEnumerable<string> arr) { return string.Join(",", arr); }

        private void AddNewlyStartedStreams(TwitchRootObject obj)
        {
            if (obj == null || obj.Streams == null || obj.Streams.Count == 0)
                return;

            foreach(var stream in obj.Streams)
            {
                bool globalancounce = false;
                if(_streamData.Keys.Contains(stream.Channel.Name))
                {
                    globalancounce = true;
                }
                bool bTryAddresult = _streamData.TryAdd(stream.Channel.Name, new TwitchStreamData { Started = DateTime.Now, Stream = stream });

                if(bTryAddresult)
                {
                    // Probably helps differentiate between different Game Titles from Response
                    string oldgame = xmlprovider.StreamInfo(stream.Channel.Name, "game");
                    string _game = "";
                    if (oldgame != stream.Channel.Game)
                    {
                        _game = stream.Channel.Game;
                    }
                    if (oldgame != stream.Game)
                    {
                        _game = stream.Game;
                    }
                    var _stream = new DeathmicChatbot.StreamInfo.Stream
                    {
                        Channel = stream.Channel.Name,
                        Game = _game,
                        Message = stream.Channel.Status
                    };
                    var _streamdata = new StreamData
                    {
                        Stream = _stream,
                        Started = DateTime.Now,
                        StreamProvider = this
                    };
                    TwitchStreamData _tstreamdata = new TwitchStreamData();
                    _tstreamdata.Started = DateTime.Now;
                    _tstreamdata.Stream = new DeathmicChatbot.StreamInfo.Twitch.Stream();
                    _tstreamdata.Stream.Channel = new DeathmicChatbot.StreamInfo.Twitch.Channel();
                    _tstreamdata.Stream.Channel.Name = stream.Channel.Name;
                    _tstreamdata.Stream.Channel.Mature = stream.Channel.Mature;
                    _tstreamdata.Stream.Channel.ID = stream.Channel.ID;
                    _tstreamdata.Stream.Channel.Delay = stream.Channel.Delay;
                    _tstreamdata.Stream.Channel.Created_At = stream.Channel.Created_At;
                    _tstreamdata.Stream.Channel.Display_Name = stream.Channel.Display_Name;
                    _tstreamdata.Stream.Channel.Links = stream.Channel.Links;
                    _tstreamdata.Stream.Channel.Profile_Banner = stream.Channel.Profile_Banner;
                    _tstreamdata.Stream.Channel.Url = stream.Channel.Url;
                    _tstreamdata.Stream.Channel.Updated_At = stream.Channel.Updated_At;
                    _tstreamdata.Stream.Game = _game;
                    _tstreamdata.Stream.Channel.Game = _game;

                    AddStreamdatatoXML(_tstreamdata);
                    StreamEventArgs streamEventArgs = new StreamEventArgs(_streamdata);
                    StreamStarted(this, streamEventArgs);
                }
                if(!bTryAddresult && globalancounce)
                {
                    string oldgame = xmlprovider.StreamInfo(stream.Channel.Name, "game");
                    string _game = "";
                    if (oldgame != stream.Channel.Game)
                    {
                        _game = stream.Channel.Game;
                    }
                    if (oldgame != stream.Game)
                    {
                        _game = stream.Game;
                    }

                    var _stream = new DeathmicChatbot.StreamInfo.Stream
                    {
                        Channel = stream.Channel.Name,
                        Game = _game,
                        Message = stream.Channel.Status
                    };
                    var _streamdata = new StreamData
                    {
                        Stream = _stream,
                        Started = DateTime.Now,
                        StreamProvider = this
                    };
                    StreamEventArgs streamEventArgs = new StreamEventArgs(_streamdata);
                    StreamGlobalNotification(this, streamEventArgs);
                }
                
                
            }
        }

        private void RemoveStoppedStreams(TwitchRootObject obj)
        {
            foreach (var pair in from pair in _streamData where StreamStopped != null select pair)
            {
                bool bFound = obj.Streams.Any(stream => pair.Key == stream.Channel.Name);
                if(!bFound)
                {
                    TwitchStreamData sd;
                    _streamStopCounter.StreamTriesToStop(pair.Key);
                    if(_streamStopCounter.StreamHasTriedStoppingEnoughTimes(pair.Key) || !_streamData.TryRemove(pair.Key,out sd))
                    {
                        continue;
                    }
                    _streamStopCounter.StreamHasStopped(pair.Key);
                    var stream = new DeathmicChatbot.StreamInfo.Stream
                    {
                        Channel = pair.Value.Stream.Channel.Name,
                        Game = pair.Value.Stream.Game,
                        Message = pair.Value.Stream.Channel.Status
                    };
                    var streamData = new StreamData
                    {
                        Started = DateTime.Now,
                        Stream = stream,
                        StreamProvider = this
                    };
                    var streamEventArgs = new StreamEventArgs(streamData);
                    StreamStopped(this, streamEventArgs);
                    

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
        public void StartTimer()
        {
            // Only used in Hitbox but since i have to add it to interface ...
        }
    }
}

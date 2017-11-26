using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using BobCore.StreamFunctions.JSON;

namespace BobCore.StreamFunctions
{
    class TwitchChecker : StreamChecker
    {
        private readonly RestClient _client;
        private List<DataClasses.internalStream> _lsStreams;
        private Timer _timer;
        private bool inprogress = false;
        private DataClasses.SecurityVault vault;
        public TwitchChecker()
        {
            _client = new RestClient("https://api.twitch.tv");
        }

        public event EventHandler<StreamEventArgs> StreamOnline;
        public event EventHandler<StreamEventArgs> StreamOffline;

        public void AddSecurityVaultData(DataClasses.SecurityVault _vault)
        {
            vault = _vault;
        }

        public void CheckOnlineStreams()
        {
            try
            {
                if (!inprogress)
                {
                    inprogress = true;

                    var req = new RestRequest("helix/streams", Method.GET);
                    req.AddHeader("Client-ID", vault.TwitchToken);
                    //StreamCheckRequest Setup (OLD API)
                    //var req = new RestRequest("/kraken/streams", Method.GET);
                    //req.AddHeader("Client-ID", Properties.Settings.Default.TwitchclientID.ToString());
                    //Serializing Streams for Request

                    List<string> serializedstream = new List<string>();
                    foreach (DataClasses.internalStream stream in _lsStreams)
                    {
                        //serializedstream.Add(stream.sChannel);
                        req.AddParameter("user_login", stream.sChannel);
                    }
                    //req.AddParameter("channel", string.Join(",", serializedstream.ToArray()));
                    var response = _client.Execute(req);
                    StreamFunctions.JSON.TwitchStreamData streams = Newtonsoft.Json.JsonConvert.DeserializeObject<StreamFunctions.JSON.TwitchStreamData>(response.Content);
                    #region api 5
                    List<string> onlinestreams = new List<string>();
                    if (streams.data != null)
                    {
                        if (streams.data.Count() > 0)
                        {

                            foreach (TwitchStream stream in streams.data)
                            {
                                var _stream = _lsStreams.Where(x => x.sUserID == stream.user_id);
                                //There is a valid Reason for this being not in the else case of the if after this one
                                if (_stream.Count() == 0)
                                {
                                    var IDrequest = new RestRequest("helix/users");
                                    IDrequest.AddHeader("Client-ID", vault.TwitchToken);
                                    IDrequest.AddParameter("id", stream.user_id);
                                    var IDresponse = _client.Execute(IDrequest);
                                    if (IDresponse.StatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        StreamFunctions.JSON.TwitchID user = Newtonsoft.Json.JsonConvert.DeserializeObject<StreamFunctions.JSON.TwitchID>(IDresponse.Content);
                                        if (user.data != null)
                                        {
                                            if (user.data.FirstOrDefault() != null)
                                            {
                                                _stream = _lsStreams.Where(x => x.sChannel.ToLower() == user.data.FirstOrDefault().display_name.ToLower());
                                                if (_stream.Count() > 0)
                                                {
                                                    _stream.FirstOrDefault().sUserID = user.data.FirstOrDefault().id;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (_stream.Count() > 0)
                                {
                                    StreamEventArgs args = new StreamEventArgs();
                                    args.game = stream.title;
                                    _stream.FirstOrDefault().sGame = stream.title;
                                    _stream.FirstOrDefault().sUrl = _stream.FirstOrDefault().getUrl("twitch");
                                    onlinestreams.Add(_stream.FirstOrDefault().sChannel);
                                    if (!_stream.FirstOrDefault().bRunning)
                                    {
                                        args.state = 1;
                                        _stream.FirstOrDefault().StreamStarting();
                                    }
                                    else
                                    {
                                        args.state = 2;
                                        _stream.FirstOrDefault().StreamRelayCheck();
                                    }
                                    args.link = _stream.FirstOrDefault().getUrl("twitch");
                                    args.channel = _stream.FirstOrDefault().sChannel;
                                    if (_stream.FirstOrDefault().bShouldNotify())
                                    {
                                        StreamOnline(this, args);
                                    }
                                }
                            }
                        }
                    }

                    // end all streams that are not in onlinestreams
                    var runningstreams = _lsStreams.Where(x => x.bRunning && !onlinestreams.Contains(x.sChannel));
                    foreach (var runningstream in runningstreams)
                    {
                        StreamEventArgs args = new StreamEventArgs();
                        args.channel = runningstream.sChannel;
                        args.game = "";
                        args.state = 3;
                        args.stream = "";
                        args.link = "";
                        if (runningstream.StopOrTwitchError())
                        {
                            StreamOffline(this, args);
                        }
                    }
                    Administrative.XMLFileHandler.writeFile(_lsStreams, "Streams");
                    /*
                    StreamFunctions.JSON.TwitchStreamData streams = Newtonsoft.Json.JsonConvert.DeserializeObject<StreamFunctions.JSON.TwitchStreamData>(response.Content);

                    if (streams._total > 0)
                    {
                        List<string> onlinestreams = new List<string>();
                        foreach (JSON.TwitchStream stream in streams.streams)
                        {
                            var _stream = _lsStreams.Where(x => x.sChannel.ToLower() == stream.channel.name.ToLower()).FirstOrDefault();
                            StreamEventArgs args = new StreamEventArgs();
                            args.game = stream.game;
                            if (_stream == null || !_stream.bRunning)
                            {
                                args.state = 1;
                                _stream.sUrl = stream.channel.url;
                                _stream.sGame = stream.game;
                                _stream.StreamStarting();
                            }
                            else
                            {
                                args.state = 2;
                                _stream.StreamRelayCheck();
                            }

                            args.link = stream.channel.url;
                            args.channel = stream.channel.name;
                            onlinestreams.Add(stream.channel.name.ToLower());
                            if (_stream.bShouldNotify())
                            {
                                _stream.sGame = stream.game;
                                StreamOnline(this, args);
                            }

                        }
                        var offlinestreams = _lsStreams.Where(x => x.bRunning == true && !onlinestreams.Contains(x.sChannel.ToLower()));
                        foreach (DataClasses.internalStream stream in offlinestreams)
                        {
                            StreamEventArgs args = new StreamEventArgs();
                            args.channel = stream.sChannel;
                            args.game = "";
                            args.state = 3;
                            args.stream = "";
                            args.link = "";
                            if (stream.StopOrTwitchError())
                            {
                                StreamOffline(this, args);

                            }
                        }
                        Administrative.XMLFileHandler.writeFile(_lsStreams, "Streams");
                    }
                    else
                    {
                        //End Running Streams and Notify
                        //If no Streams are online no point in having them "run"
                        var runningstreams = _lsStreams.Where(x => x.bRunning);
                        foreach (var runningstream in runningstreams)
                        {
                            StreamEventArgs args = new StreamEventArgs();
                            args.channel = runningstream.sChannel;
                            args.game = "";
                            args.state = 3;
                            args.stream = "";
                            args.link = "";
                            if (runningstream.StopOrTwitchError())
                            {
                                StreamOffline(this, args);
                            }

                        }
                        Administrative.XMLFileHandler.writeFile(_lsStreams, "Streams");
                    }
                    */
                    #endregion
                    inprogress = false;


                }
            }
            catch (Exception)
            {
                inprogress = false;
                //I don't care i just don't want anny errors if twitch does not respond correctly or something
            }

        }

        public void Start()
        {
            _timer = new Timer(10000);
            _timer.Elapsed += (sender, args) => CheckOnlineStreams();
            _timer.Start();
        }

        public void Start(List<DataClasses.internalStream> Streams)
        {
            _lsStreams = Streams;
            _timer = new Timer(10000);
            _timer.Elapsed += (sender, args) => CheckOnlineStreams();
            _timer.Start();
        }
    }
}

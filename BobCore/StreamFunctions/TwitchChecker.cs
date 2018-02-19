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

                    List<string> serializedstream = new List<string>();
                    foreach (DataClasses.internalStream stream in _lsStreams)
                    {
                        //serializedstream.Add(stream.sChannel);
                        req.AddParameter("user_login", stream.sChannel);
                    }
                    var response = _client.Execute(req);
                    StreamFunctions.JSON.TwitchStreamData TwitchStreamData = Newtonsoft.Json.JsonConvert.DeserializeObject<StreamFunctions.JSON.TwitchStreamData>(response.Content);
                    #region api 5
                    List<string> onlinestreams = new List<string>();
                    if (TwitchStreamData.data != null && TwitchStreamData.data.Count() > 0)
                    {
                        foreach (TwitchStream streamData in TwitchStreamData.data)
                        {
                            var internalStream = _lsStreams.Where(x => x.sUserID == streamData.user_id).FirstOrDefault();
                            GetStreamUserIDFromTwitch();  
                            // Method takes Requests StreamName from Twitch via UserID to then write UserID into stream;
                            void GetStreamUserIDFromTwitch()
                            {
                                if (internalStream == null)
                                {
                                    var IDrequest = new RestRequest("helix/users");
                                    IDrequest.AddHeader("Client-ID", vault.TwitchToken).AddParameter("id", streamData.user_id);
                                    var IDresponse = _client.Execute(IDrequest);
                                    if (IDresponse.StatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        StreamFunctions.JSON.TwitchID user = Newtonsoft.Json.JsonConvert.DeserializeObject<StreamFunctions.JSON.TwitchID>(IDresponse.Content);
                                        if (user.data != null && user.data.FirstOrDefault() != null && (internalStream = _lsStreams.Where(x => x.sChannel.ToLower() == user.data.FirstOrDefault().display_name.ToLower()).FirstOrDefault()) != null)
                                        {
                                            internalStream.sUserID = user.data.FirstOrDefault().id;
                                        }
                                    }
                                }
                            }

                            if (internalStream != null)
                            {
                                StreamEventArgs args;
                                FillStreamArgs();
                                void FillStreamArgs()
                                {
                                    args = new StreamEventArgs();
                                    args.game = streamData.title;
                                    internalStream.sGame = streamData.title;
                                    internalStream.sUrl = internalStream.getUrl("twitch");
                                    args.link = internalStream.getUrl("twitch");
                                    args.channel = internalStream.sChannel;
                                }
                                
                                
                                onlinestreams.Add(internalStream.sChannel);
                                StartStreamOrCheckRelay();
                                void StartStreamOrCheckRelay()
                                {
                                    if (!internalStream.bRunning)
                                    {
                                        args.state = 1;
                                        internalStream.StreamStarting();
                                    }
                                    else
                                    {
                                        args.state = 2;
                                        internalStream.StreamRelayCheck();
                                    }
                                }
                                
                                
                                if (internalStream.bShouldNotify())
                                {
                                    StreamOnline(this, args);
                                }
                            }
                        }
                    }
                    EndAllNotRunningStreams();
                    void EndAllNotRunningStreams()
                    {
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
                    }
                    
                    #endregion
                    inprogress = false;


                }
            }
            catch (Exception)
            {
                inprogress = false;
                //Doesn't need to be handled will be repeated soon afterwards anyway
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

using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using BobCore.StreamFunctions.JSON;
using TwitchLib.Api;
using System.Threading.Tasks;

namespace BobCore.StreamFunctions
{
    class TwitchChecker : StreamChecker
    {
        private List<DataClasses.internalStream> _lsStreams;
        private Timer _timer;
        private bool inprogress = false;
        private DataClasses.SecurityVault vault;
        private static TwitchAPI api;
        public TwitchChecker()
        {
            
        }

        public event EventHandler<StreamEventArgs> StreamOnline;
        public event EventHandler<StreamEventArgs> StreamOffline;

        public void AddSecurityVaultData(DataClasses.SecurityVault _vault)
        {
            vault = _vault;
            InitializeAPI();
           
        }

        public void InitializeAPI()
        {
            api = new TwitchAPI();
            api.Settings.ClientId = vault.TwitchToken;
            api.Settings.AccessToken = vault.TwitchToken;
        }
        
        public async Task CheckOnlineStreams()
        {
            try
            {
                if (!inprogress)
                {
                    inprogress = true;

                    List<string> serializedstream = new List<string>();
                    foreach (DataClasses.internalStream stream in _lsStreams)
                    {
                        serializedstream.Add(stream.sChannel);
                        if(stream.sUserID != null && stream.sUserID != "")
                        {
                            if(await api.Streams.v5.BroadcasterOnlineAsync(stream.sUserID))
                            {
                                Console.WriteLine(stream.sChannel + " is online");
                                StreamEventArgs args = new StreamEventArgs();
                                if (!stream.bRunning)
                                {
                                    var streamdata = api.Streams.v5.GetStreamByUserAsync(stream.sUserID);
                                    string game = streamdata.Result.Stream.Game;
                                    string channel = streamdata.Result.Stream.Channel.Name;
                                    DateTime startdate = streamdata.Result.Stream.CreatedAt;
                                    
                                    FillStreamArgs();
                                    void FillStreamArgs()
                                    {
                                        args.game = game;
                                        stream.sGame = game;
                                        stream.sUrl = stream.getUrl("twitch");
                                        args.link = stream.getUrl("twitch");
                                        args.channel = stream.sChannel;
                                        stream.dtStarttime = streamdata.Result.Stream.CreatedAt;
                                    }
                                }
                                else
                                {
                                    args.game = stream.sGame;
                                    args.link = stream.sUrl;
                                    args.channel = stream.sChannel;
                                }
                                StartStreamOrCheckRelay();
                                void StartStreamOrCheckRelay()
                                {
                                    if (!stream.bRunning)
                                    {
                                        args.state = 1;
                                        stream.StreamStarting();
                                    }
                                    else
                                    {
                                        args.state = 2;
                                        stream.StreamRelayCheck();
                                    }
                                }
                                if (stream.bShouldNotify())
                                {
                                    StreamOnline(this, args);
                                }
                            }
                            else
                            {
                                EndRunningStream(stream);
                                void EndRunningStream(DataClasses.internalStream _stream)
                                {
                                    if(_stream.bRunning)
                                    {
                                        StreamEventArgs args = new StreamEventArgs();
                                        args.channel = _stream.sChannel;
                                        args.game = "";
                                        args.state = 3;
                                        args.stream = "";
                                        args.link = "";
                                        if (_stream.StopOrTwitchError())
                                        {
                                            StreamOffline(this, args);
                                        }
                                        Administrative.XMLFileHandler.writeFile(_lsStreams, "Streams");
                                    }
                                }
                            }
                            
                        }
                    }
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

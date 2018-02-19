using System;
using BobCore.StreamFunctions.Relay;
using System.Threading;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BobCore.DataClasses
{
    public class Stream
    {
        public string name;
        public bool subscribed;
        public bool hourlyannouncement;

        public Stream(string _name, bool _subscribed)
        {
            name = _name;
            subscribed = _subscribed;
        }
        public Stream()
        {

        }
    }
    public class internalStream
    {
        public string sChannel;
        public string sUserID;
        public DateTime dtStarttime;
        public DateTime dtStoptime;
        public bool bRunning;
        public string sProvider;
        public string sGame;
        public string sUrl;
        public DateTime dtLastglobalnotice;
        public string sTwitchchat;
        //public bool bTwoway;
        public bool bRelayActive;
        public string sTargetrelaychannel;
        private DiscordSocketClient client;
        private TwitchRelay Relay;
        private string sTempTargetrelaychannel = "";
        Thread RelayThread;
        List<DataClasses.internalStream> StreamList;
        public string getUrl(string provider)
        {

            switch (provider)
            {
                case "twitch":
                    return "https://go.twitch.tv/" + sChannel;
            }

            return "";
        }
        public string StreamStartedMessage()
        {
            if(Relay != null)
            {
                return String.Format("{0} hat angefangen {1} auf {2} zu streamen. Der Relay läuft im Channel {3}",sChannel,sGame,sUrl,Relay.sTargetChannel);
            }
            return sChannel + " has started streaming " + sGame + " on" + sUrl;
        }
        public string StreamRunningMessage()
        {
            return sChannel + " is streaming " + sGame + " on" + sUrl;
        }
        public internalStream()
        {

        }
        public internalStream(string _channel, string _sTwitchchat, string _sTargetrelaychannel)
        {
            sChannel = _channel;
            sTwitchchat = _sTwitchchat;
            sTargetrelaychannel = _sTargetrelaychannel;
        }
        public void initStreamRelay(DiscordSocketClient socket,List<internalStream> _streams)
        {
            if(bRelayActive)
            {
                StreamList = _streams;
                client = socket;
            }
        }
        public void stopStreamRelay()
        {
            EndStreamRelay();
        }
        public void StreamRelay()
        {
            if (sUrl.Contains("twitch"))
            {
                if (client != null)
                {
                    //CHOOSE RELAY CHANNEL
                    if (sTargetrelaychannel != "" && sTargetrelaychannel != null)
                    {
                        if (Relay == null)
                        {
                            Relay = new TwitchRelay(client, sChannel, sTargetrelaychannel);
                            RelayThread = new Thread(Relay.ConnectToTwitch);
                            RelayThread.Start();
                            while (!RelayThread.IsAlive) { }
                            Thread.Sleep(1);
                        }
                        else
                        {
                            if (Relay.isExit && Relay.bDisconnected)
                            {
                                RelayThread = new Thread(Relay.ConnectToTwitch);
                                RelayThread.Start();
                                while (!RelayThread.IsAlive) { }
                                Thread.Sleep(1);
                            }
                        }
                    }
                    else
                    {
                        List<string> occupiedchannels = new List<string>();
                        foreach (internalStream stream in StreamList)
                        {
                            if (stream.Relay != null && !stream.Relay.bDisconnected)
                            {
                                    occupiedchannels.Add(stream.Relay.sTargetChannel);
                            }
                        }
                        List<string> availablechannels = new List<string>();
                        foreach(var channel in client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().TextChannels)
                        {
                            try
                            {
                                if(channel != null && channel.Name != "")
                                {
                                    if (Regex.Match(channel.Name.ToLower(), @"stream_\d+").Success)
                                    {
                                        availablechannels.Add(channel.Name);
                                    }
                                }
                            }catch(Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            
                            
                        }
                        string targetchannel = availablechannels.Except(occupiedchannels).First();
                        if(targetchannel == null)
                        {
                            targetchannel = "botspam";
                        }
                        if (Relay == null)
                        {
                            Relay = new TwitchRelay(client, sChannel, targetchannel);
                            RelayThread = new Thread(Relay.ConnectToTwitch);
                            RelayThread.Start();
                            while (!RelayThread.IsAlive) { }
                            Thread.Sleep(1);
                        }
                        else
                        {
                            if (Relay.isExit && Relay.bDisconnected)
                            {
                                RelayThread = new Thread(Relay.ConnectToTwitch);
                                RelayThread.Start();
                                while (!RelayThread.IsAlive) { }
                                Thread.Sleep(1);
                            }
                        }

                    }

                }
            }
        }
        public void MoveRelay(string tempchannel)
        {
            if (Relay != null)
            {
                Relay.sTargetChannel = tempchannel;
                sTempTargetrelaychannel = tempchannel;
            }
        }
        public void RelayMessage(string message, string channel)
        {
            try
            {
                if (sTempTargetrelaychannel != "")
                {
                    if (channel.ToLower() == sTempTargetrelaychannel.ToLower())
                    {
                        if(!Useful_Functions.isDebug)
                        {
                            Relay.RelayMessage(message);
                        }
                        else
                        {
                            Relay.RelayMessage(message);
                        }
                        
                    }
                }
                else
                {
                    if (channel.ToLower() == Relay.sTargetChannel)
                    {
                        if (!Useful_Functions.isDebug)
                        {
                            Relay.RelayMessage(message);
                        }
                        else
                        {
                            Relay.RelayMessage(message);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        public bool ActiveRelay()
        {
            // TODO: Add Online Detection for Relay
            if (Relay != null)
            {
                return true;
            }
            return false;
        }
        public internalStream(string _channel)
        {
            sChannel = _channel;
        }
        public bool StopOrTwitchError()
        {
            //false = Should not be stopped yet could be twitch error
            //true = No twitch error aka stop
            if (dtStoptime == DateTime.MinValue)
            {
                dtStoptime = DateTime.Now;
            }
            if (DateTime.Now.Subtract(dtStoptime).TotalMinutes > 10)
            {
                bRunning = false;
                EndStreamRelay();
                GC.Collect();
                return true;
            }
            return false;
        }
        public void StreamStarting()
        {
            bRunning = true;
            dtStarttime = DateTime.Now;
            dtLastglobalnotice = dtStarttime;
            dtStoptime = DateTime.MinValue;
            if (bRelayActive)
            {
                StreamRelay();
            }
        }
        public void StreamRelayCheck()
        {
            if (bRelayActive)
            {
                StreamRelay();
                if (Relay != null)
                {
                    Relay.StopRelayEnd();
                }
            }
        }
        public void EndStreamRelay()
        {
            if (Relay != null)
            {
                Relay.StartRelayEnd();
                while(!Relay.bDisconnected)
                {
                    Thread.Sleep(500);
                }
                Relay = null;
                GC.Collect();
                RelayThread = null;
            }

        }
        public bool bShouldNotify()
        {
            if (dtStarttime == dtLastglobalnotice)
            {
                dtLastglobalnotice = DateTime.Now;
                return true;
            }
            else
            {
                if (DateTime.Now.Subtract(dtLastglobalnotice).TotalMinutes > 60)
                {
                    dtLastglobalnotice = DateTime.Now;
                    return true;
                }
            }
            return false;
        }
    }
}

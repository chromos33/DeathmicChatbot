using System;
using BobCore.StreamFunctions.Relay;
using System.Threading;
using Discord.WebSocket;

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
        public bool bTwoway;
        public string sTargetrelaychannel;
        private DiscordSocketClient client;
        private TwitchRelay Relay;
        private string sTempTargetrelaychannel;
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
            return sChannel + " has started streaming " + sGame + " on" + sUrl;
        }
        public string StreamRunningMessage()
        {
            return sChannel + " is streaming " + sGame + " on" + sUrl;
        }
        public internalStream()
        {

        }
        public internalStream(string _channel, string _sTwitchchat, bool _bTwoway, string _sTargetrelaychannel)
        {
            sChannel = _channel;
            sTwitchchat = _sTwitchchat;
            bTwoway = _bTwoway;
            sTargetrelaychannel = _sTargetrelaychannel;
        }
        public void initStreamRelay(DiscordSocketClient socket)
        {
            if (sTargetrelaychannel != "" && sTargetrelaychannel != null)
            {
                client = socket;
            }
            else
            {
                client = null;
            }

        }
        public void StreamRelay()
        {
            if (sUrl.Contains("twitch"))
            {
                if (client != null)
                {
                    if (sTargetrelaychannel != "" && sTargetrelaychannel != null)
                    {
                        if (Relay == null)
                        {
                            Relay = new TwitchRelay(client, sChannel, sTargetrelaychannel, bTwoway);
                            Thread RelayThread = new Thread(Relay.ConnectToTwitch);
                            RelayThread.Start();
                            while (!RelayThread.IsAlive) { }
                            Thread.Sleep(1);
                        }
                        else
                        {
                            if (Relay.isExit && Relay.bDisconnected)
                            {
                                Thread RelayThread = new Thread(Relay.ConnectToTwitch);
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
                if (sTempTargetrelaychannel != null)
                {
                    if (channel.ToLower() == sTempTargetrelaychannel.ToLower())
                    {
                        Relay.RelayMessage(message);
                    }
                }
                else
                {
                    if (channel.ToLower() == sTargetrelaychannel.ToLower())
                    {
                        Relay.RelayMessage(message);
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
            /*
            if(DateTime.Now.Subtract(dtStoptime).TotalMinutes >20)
            {
                dtStoptime = DateTime.Now;
            }
            */
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
            if (sTargetrelaychannel != "")
            {
                StreamRelay();
            }
        }
        public void StreamRelayCheck()
        {
            if (sTargetrelaychannel != "")
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

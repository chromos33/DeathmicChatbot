using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;
using Discord;
using DeathmicChatbot.Properties;
using System.Threading;
using IrcDotNet.Ctcp;
using System.Timers;

namespace DeathmicChatbot.IRC
{
    class RelayBot : BasicIrcBot
    {
        DiscordClient discordclient;
        public IrcClient thisclient;
        public string clientVersionInfo = "IRC.NET Community Bot";
        public CtcpClient ctcpClient1;
        private AutoResetEvent ctcpClientPingResponseReceivedEvent;
        private AutoResetEvent ctcpClientVersionResponseReceivedEvent;
        private AutoResetEvent ctcpClientTimeResponseReceivedEvent;
        private AutoResetEvent ctcpClientActionReceivedEvent;
        private static TimeSpan clientPingTime;
        private static string clientReceivedTimeInfo;
        private static string clientReceivedVersionInfo;
        private static string clientReceivedActionText;
        private string sServer;
        private bool bTwoWay;
        bool bIsStream;
        string sChannel;
        private static System.Timers.Timer discordreminder;
        public override IrcRegistrationInfo RegistrationInfo
        {
            get
            {
                return new IrcUserRegistrationInfo()
                {
                    NickName = Properties.Settings.Default.Name,
                    UserName = Properties.Settings.Default.Name,
                    RealName = Properties.Settings.Default.Name
                };
            }
        }
        public RelayBot()
        {
        }
        public RelayBot(DiscordClient discord,bool twoway = true, string server = null, bool bStream = false,string channel = null)
        {
            if (server == null)
            {
                sServer = Settings.Default.Server;
            }
            else
            {
                sServer = server;
            }
            discordclient = discord;
            bIsStream = bStream;
            bTwoWay = twoway;
            sChannel = channel;
            //Connect Kram

        }
        public void runBot()
        {
            discordreminder = new System.Timers.Timer(60 * 60 * 1000);
            discordreminder.Elapsed += OnReminderTimerEvent;
            discordreminder.Start();
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                if (thisclient != null)
                {
                    if (!thisclient.IsConnected)
                    {
                        Environment.Exit(0);
                    }
                }

                if (thisclient == null)
                {
                    if (CheckForInternetConnection())
                    {
                        // Integrate Self kill if Connection possible but nor irc connection because nick etc is already in use etc
                        System.Threading.Thread.Sleep(1000);
                        ConnectBot();
                        break;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
                Thread.Sleep(5000);
            }
        }

        private void OnReminderTimerEvent(object sender, ElapsedEventArgs e)
        {
            this.Clients.First().LocalUser.SendMessage(this.Clients.First().Channels.First(), "Errinnerung wir sind auf Discord umgezogen: https://discord.gg/CF4wZ6S");
        }

        public void ConnectBot()
        {
            if(bIsStream)
            {
                RegistrationInfo.NickName = "bobdeathmic";
                RegistrationInfo.Password = "oauth:jfapbrvofxj2q5rkcw4hg32w466zza";
            }
            Connect(sServer, RegistrationInfo);
            if (Settings.Default.Server.Contains("quakenet") && !bIsStream)
            {
                string quakeservername = null;
                foreach (var _client in Clients)
                {
                    while (_client.ServerName == null)
                    {
                        Thread.Sleep(50);
                    }
                    if (_client.ServerName.Contains("quakenet"))
                    {
                        quakeservername = _client.ServerName;
                        thisclient = _client;
                        thisclient.FloodPreventer = new IrcStandardFloodPreventer(1, 4000);
                        /*
                        ctcpClient1 = new CtcpClient(_client);
                        ctcpClient1.ClientVersion = clientVersionInfo;
                        ctcpClient1.PingResponseReceived += ctcpClient_PingResponseReceived;
                        ctcpClient1.VersionResponseReceived += ctcpClient_VersionResponseReceived;
                        ctcpClient1.TimeResponseReceived += ctcpClient_TimeResponseReceived;
                        ctcpClient1.ActionReceived += ctcpClient_ActionReceived;
                        */
                    }

                }
                IrcClient quakeclient = null;
                quakeclient = GetClientFromServerNameMask(quakeservername);

                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.Channel + " " + quakeservername);
                quakeclient.Channels.Join(Properties.Settings.Default.Channel);
            }
            else
            {
                foreach (var _client in Clients)
                {
                    if(sChannel != null)
                    {
                        _client.Channels.Join(sChannel);
                    }
                    else
                    {
                        _client.Channels.Join(Properties.Settings.Default.Channel);
                    }
                    
                    thisclient = _client;
                    thisclient.FloodPreventer = new IrcStandardFloodPreventer(1, 1500);
                    ctcpClient1 = new CtcpClient(_client);
                }
            }
        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        protected override void OnChannelMessageReceived(IrcChannel channel, IrcMessageEventArgs e)
        {
            if(e.Source.Name != "RelayBob")
            {
                if (bIsStream)
                {
                    discordclient.Servers.First().TextChannels.Where(x => x.Name == "stream").First().SendMessage("Relay IRC#" + e.Source.Name + ": " + e.Text);
                }
                else
                {
                    discordclient.Servers.First().TextChannels.Where(x => x.Name == "allgemein").First().SendMessage("Relay IRC#" + e.Source.Name + ": " + e.Text);
                }
            }
            
        }
        public void RelayMessage(string message,string user)
        {
            try
            {
                if(bTwoWay)
                {
                    if (user != "BobDeathmic")
                    {
                        this.Clients.First().LocalUser.SendMessage(this.Clients.First().Channels.First(), "Relay " + user + ": " + message);
                    }
                }
            }
            catch(Exception)
            {

            }
        }
        #region CTCP Client Event Handlers

        public void ctcpClient_PingResponseReceived(object sender, CtcpPingResponseReceivedEventArgs e)
        {
            if (e.User.NickName == thisclient.LocalUser.NickName)
                clientPingTime = e.PingTime;

            if (ctcpClientPingResponseReceivedEvent != null)
                ctcpClientPingResponseReceivedEvent.Set();
        }

        public void ctcpClient_VersionResponseReceived(object sender, CtcpVersionResponseReceivedEventArgs e)
        {
            if (e.User.NickName == thisclient.LocalUser.NickName)
                clientReceivedVersionInfo = e.VersionInfo;

            if (ctcpClientVersionResponseReceivedEvent != null)
                ctcpClientVersionResponseReceivedEvent.Set();
        }

        public void ctcpClient_TimeResponseReceived(object sender, CtcpTimeResponseReceivedEventArgs e)
        {
            if (e.User.NickName == thisclient.LocalUser.NickName)
                clientReceivedTimeInfo = e.DateTime;

            if (ctcpClientTimeResponseReceivedEvent != null)
                ctcpClientTimeResponseReceivedEvent.Set();
        }

        public void ctcpClient_ActionReceived(object sender, CtcpMessageEventArgs e)
        {
            if (e.Source.NickName == thisclient.LocalUser.NickName)
                clientReceivedActionText = e.Text;

            if (ctcpClientActionReceivedEvent != null)
                ctcpClientActionReceivedEvent.Set();
        }

        #endregion
    }
}

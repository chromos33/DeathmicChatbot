using Discord;
using IrcDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Text.RegularExpressions;
using RestSharp.Deserializers;
using RestSharp;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using DeathmicChatbot.StreamInfo.Twitch;
using Newtonsoft.Json.Linq;

namespace DeathmicChatbot.IRC
{
    public class TwitchRelay
    {
        public TwitchRelay()
        {

        }
        private static System.Timers.Timer TimeoutTimer;
        protected System.Timers.Timer DisconnectTimer;
        public string sChannel = "";
        public string sTargetChannel = "";
        public DiscordClient discordclient;
        public bool bTwoWay;
        private IrcDotNet.TwitchIrcClient LocalClient;
        public bool isExit = false;
        bool ReconnectInBound = false;
        RestClient _client;
        public TwitchRelay(DiscordClient discord, string channel, string targetchannel, bool twoway = true)
        {
            
            sChannel = channel;
            sTargetChannel = targetchannel;
            discordclient = discord;
            bTwoWay = twoway;
        }
        public void StartRelayEnd()
        {
            TimeoutTimer = new System.Timers.Timer(10*60*1000);
            TimeoutTimer.Elapsed += OnTimeoutTimerEvent;
            TimeoutTimer.Start();
        }

        private async void OnDisconnectTimer(object sender, ElapsedEventArgs e)
        {
            if(!isExit)
            {
                DisconnectTimer.Stop();
                string urlParameters = "?Client-ID=" + Properties.Settings.Default.TwitchclientID.ToString();
                    
                HttpClient tmpClient = new HttpClient();
                tmpClient.BaseAddress = new Uri("https://tmi.twitch.tv/group/user/" + sChannel + "/chatters");

                tmpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                 // Blocking call!
                int counter = 0;
                bool reconnect = true;
                while(counter < 3)
                {
                    HttpResponseMessage response = tmpClient.GetAsync(urlParameters).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonstring = await response.Content.ReadAsStringAsync();
                        DeathmicChatbot.StreamInfo.Twitch.Chatters.Root json = JsonConvert.DeserializeObject<DeathmicChatbot.StreamInfo.Twitch.Chatters.Root>(jsonstring);
                        if (json.chatters.viewers.Where(x => x.ToLower() == "bobdeathmic").Count() > 0)
                        {
                            counter = 5;
                            reconnect = false;
                        }
                        else
                        {
                            Thread.Sleep(40000);
                        }
                    }
                    
                }
                if(reconnect)
                {
                    LocalClient.Disconnect();
                }
                DisconnectTimer.Start();
            }
        }

        private void OnTimeoutTimerEvent(object sender, ElapsedEventArgs e)
        {
            isExit = true;
        }

        public void StopRelayEnd()
        {
            TimeoutTimer.Stop();
        }
        public void ConnectToTwitch()
        {

            var sServer = "irc.twitch.tv";
            var username = "bobdeathmic";
            var password = "oauth:1hequknt8uxqfgz8u5bcg5y8rw1y6o";
            isExit = false;
            using (var client = new IrcDotNet.TwitchIrcClient())
            {
                client.FloodPreventer = new IrcStandardFloodPreventer(2, 2500);
                client.Disconnected += IrcClient_Disconnected;
                client.Registered += IrcClient_Registered;
                using (var registeredEvent = new ManualResetEventSlim(false))
                {
                    using (var connectedEvent = new ManualResetEventSlim(false))
                    {
                        client.Connected += (sender2, e2) => connectedEvent.Set();
                        client.Registered += (sender2, e2) => registeredEvent.Set();
                        client.Connect(sServer, false,
                            new IrcUserRegistrationInfo()
                            {
                                NickName = username,
                                Password = password,
                                UserName = username
                            });
                        if (!connectedEvent.Wait(10000))
                        {
                            return;
                        }
                    }
                    if (!registeredEvent.Wait(10000))
                    {
                        return;
                    }
                }
                LocalClient = client;
                if(DisconnectTimer == null)
                {
                    _client = new RestClient("https://api.twitch.tv");
                    DisconnectTimer = new System.Timers.Timer(2 * 10 * 1000);
                    DisconnectTimer.Elapsed += OnDisconnectTimer;
                    DisconnectTimer.Start();
                }
                HandleEventLoop(client, sChannel);
            }
        }
        private void HandleEventLoop(IrcDotNet.IrcClient client,string sChannel)
        {
            var channel = discordclient.Servers.First().TextChannels.Where(x => x.Name.ToLower() == sTargetChannel.ToLower());
            if(channel != null)
            {
                channel.First().SendMessage("Twitch Relay Activated");
            }
            while (!isExit)
            {
                if (client.Channels.Where(x => x.Name.ToLower().Contains(sChannel.ToLower())).Count() == 0)
                {
                    client.Channels.Join("#"+sChannel);
                }
                Thread.Sleep(5000);
            }
            discordclient.Servers.First().TextChannels.Where(x => x.Name.ToLower() == sTargetChannel.ToLower()).First().SendMessage("Twitch Relay Terminated");
            client.Disconnect();
        }
        private void IrcClient_Registered(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;

            client.LocalUser.NoticeReceived += IrcClient_LocalUser_NoticeReceived;
            client.LocalUser.MessageReceived += IrcClient_LocalUser_MessageReceived;
            client.LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;
            client.LocalUser.LeftChannel += IrcClient_LocalUser_LeftChannel;
        }

        private void IrcClient_LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            e.Channel.UserJoined -= IrcClient_Channel_UserJoined;
            e.Channel.UserLeft -= IrcClient_Channel_UserLeft;
            e.Channel.MessageReceived -= IrcClient_Channel_MessageReceived;
            e.Channel.NoticeReceived -= IrcClient_Channel_NoticeReceived;
            
        }

        private void IrcClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            e.Channel.UserJoined += IrcClient_Channel_UserJoined;
            e.Channel.UserLeft += IrcClient_Channel_UserLeft;
            e.Channel.MessageReceived += IrcClient_Channel_MessageReceived;
            e.Channel.NoticeReceived += IrcClient_Channel_NoticeReceived;
            
        }

        private void IrcClient_Channel_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            
        }

        private void IrcClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            if (e.Text.Contains("connected"))
            {
                ReconnectInBound = false;
            }
            if (e.Source.Name != LocalClient.LocalUser.NickName)
            {
                string message = e.Text;
                if (e.Text.Contains('@'))
                {
                    Regex r = new Regex(@"@(\w+)");
                    MatchCollection mc = r.Matches(message);
                    foreach(Match match in mc)
                    {
                        string sMatch = match.Value.Replace("@", "");
                        var users = discordclient.Servers.First().Users.Where(x => x.Name.ToLower() == sMatch.ToLower() || (x.Nickname != null && x.Nickname != "" && x.Nickname.ToLower() == sMatch.ToLower()));
                        if(users.Count() > 0)
                        {
                            message = message.Replace(match.Value,users.First().Mention);
                        }
                    }
                }
                discordclient.Servers.First().TextChannels.Where(x => x.Name.ToLower() == sTargetChannel.ToLower()).First().SendMessage("TwitchRelay " + e.Source.Name + ": " + message);
            }
        }
        public void RelayMessage(string message)
        {
            if(bTwoWay)
            {
                LocalClient.LocalUser.SendMessage("#"+sChannel, message);
            }
        }

        private static void IrcClient_Channel_UserLeft(object sender, IrcChannelUserEventArgs e)
        {
        }

        private static void IrcClient_Channel_UserJoined(object sender, IrcChannelUserEventArgs e)
        {
            
        }

        private void IrcClient_LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
        }

        private void IrcClient_LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
        }

        private void IrcClient_Disconnected(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;
            if(!isExit)
            {
                ConnectToTwitch();
            }
        }

        private static void IrcClient_Connected(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;
        }
        public void DisconnectRelay()
        {
            LocalClient.Disconnect();
        }
    }
}

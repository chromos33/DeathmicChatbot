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
        public string sChannel = "";
        public string sTargetChannel = "";
        public DiscordClient discordclient;
        public bool bTwoWay;
        private IrcDotNet.TwitchIrcClient LocalClient;
        public bool isExit = false;
        bool ReconnectInBound = false;
        RestClient _client;
        public bool bDisconnected = false;
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
            TimeoutTimer.Start();//
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
                HandleEventLoop(client, sChannel);
            }
        }
        private void HandleEventLoop(IrcDotNet.IrcClient client,string sChannel)
        {
            var channel = discordclient.Servers.First().TextChannels.Where(x => x.Name.ToLower() == sTargetChannel.ToLower());
            bDisconnected = false;
            if (channel != null)
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
            
        }

        private void IrcClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            e.Channel.UserJoined += IrcClient_Channel_UserJoined;
            e.Channel.UserLeft += IrcClient_Channel_UserLeft;
            e.Channel.MessageReceived += IrcClient_Channel_MessageReceived;
            
        }
        

        private void IrcClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
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
                LocalClient.LocalUser.SendMessage("#" + sChannel, message);
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
            Console.WriteLine("test");
        }

        private void IrcClient_LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            Console.WriteLine("test");
        }

        private void IrcClient_Disconnected(object sender, EventArgs e)
        {
            bDisconnected = true;
            var client = (IrcClient)sender;
        }

        private static void IrcClient_Connected(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;
        }
        public void DisconnectRelay()
        {
            isExit = true;
        }
    }
}

using Discord;
using Discord.WebSocket;
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
using Newtonsoft.Json.Linq;

namespace BobCore.StreamFunctions.Relay
{
    public class TwitchRelay
    {
        public TwitchRelay()
        {

        }
        public string sChannel = "";
        public string sTargetChannel = "";
        public DiscordSocketClient discordclient;
        private IrcDotNet.TwitchIrcClient LocalClient;
        public bool isExit = false;
        public bool bDisconnected = false;
        public TwitchRelay(DiscordSocketClient discord, string channel, string targetchannel)
        {

            sChannel = channel;
            sTargetChannel = targetchannel;
            discordclient = discord;
        }
        public void StartRelayEnd()
        {
            isExit = true;
        }


#pragma warning disable RECS0154 // Parameter is never used
        private void OnTimeoutTimerEvent(object sender, ElapsedEventArgs e)
#pragma warning restore RECS0154 // Parameter is never used
        {
            isExit = true;
        }

        public void StopRelayEnd()
        {
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
        private void HandleEventLoop(IrcDotNet.IrcClient client, string _sChannel)
        {
            while (discordclient.ConnectionState != ConnectionState.Connected)
            {
                Thread.Sleep(1000);
            }
            var channel = discordclient.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().TextChannels.Where(x => x.Name.ToLower() == sTargetChannel).FirstOrDefault();
            bDisconnected = false;
            if (channel != null)
            {
                if(!Useful_Functions.isDebug)
                {
                    channel.SendMessageAsync("Twitch Relay Activated (" + sChannel + ")");
                }
                else
                {
                    Console.WriteLine("Twitch Relay Activated (" + sChannel + ")");
                }
            }
            else
            {
                isExit = true;
            }
            while (!isExit)
            {
                if (client.Channels.Where(x => x.Name.ToLower().Contains(_sChannel.ToLower())).Count() == 0)
                {
                    client.Channels.Join("#" + _sChannel);
                }
                Thread.Sleep(5000);
            }
            if (channel != null)
            {
                if (!Useful_Functions.isDebug)
                {
                    channel.SendMessageAsync("Twitch Relay Terminated (" + sChannel + ")");
                }
                else
                {
                    Console.WriteLine("Twitch Relay Terminated (" + sChannel + ")");
                }
                
            }
            client.Disconnect();
            bDisconnected = true;
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
                    foreach (Match match in mc)
                    {
                        string sMatch = match.Value.Replace("@", "");
                        var users = discordclient.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == sMatch.ToLower());
                        if (users.Count() > 0)
                        {
                            message = message.Replace(match.Value, users.First().Mention);
                        }
                    }
                }
                discordclient.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().TextChannels.Where(x => x.Name.ToLower() == sTargetChannel.ToLower()).First().SendMessageAsync("TwitchRelay " + e.Source.Name + ": " + message);
            }
        }
        private string norepeat = "";
        public void RelayMessage(string message)
        {
            if (message != norepeat)
            {
                if(!Useful_Functions.isDebug)
                {
                    LocalClient.LocalUser.SendMessage("#" + sChannel, message);
                }
                    
                norepeat = message;
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
            bDisconnected = true;
            var client = (IrcClient)sender;
        }

#pragma warning disable RECS0154 // Parameter is never used
        private static void IrcClient_Connected(object sender, EventArgs e)
#pragma warning restore RECS0154 // Parameter is never used
        {
            var client = (IrcClient)sender;
        }
    }
}

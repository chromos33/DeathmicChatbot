using Discord;
using IrcDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DeathmicChatbot.IRC
{
    public class TwitchRelay
    {
        public TwitchRelay()
        {

        }
        public string sChannel = "";
        public string sTargetChannel = "";
        public DiscordClient discordclient;
        public bool bTwoWay;
        private IrcDotNet.TwitchIrcClient LocalClient;
        public TwitchRelay(DiscordClient discord, string channel, string targetchannel, bool twoway = true)
        {
            
            sChannel = channel;
            sTargetChannel = targetchannel;
            discordclient = discord;
            bTwoWay = twoway;
        }
        public void ConnectToTwitch()
        {
            var sServer = "irc.twitch.tv";
            var username = "bobdeathmic";
            var password = "oauth:1hequknt8uxqfgz8u5bcg5y8rw1y6o";
            using (var client = new IrcDotNet.TwitchIrcClient())
            {
                client.FloodPreventer = new IrcStandardFloodPreventer(2, 4000);
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
            bool isExit = false;
            while (!isExit)
            {
                if (client.Channels.Where(x => x.Name.ToLower().Contains(sChannel.ToLower())).Count() == 0)
                {
                    client.Channels.Join("#"+sChannel);
                }
                Thread.Sleep(5000);
            }
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

        private static void IrcClient_Channel_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            
        }

        private void IrcClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            if(e.Source.Name != LocalClient.LocalUser.NickName)
            {
                discordclient.Servers.First().TextChannels.Where(x => x.Name.ToLower() == "stream").First().SendMessage("TwitchRelay " + e.Source.Name + ": " + e.Text);
            }
            //discordclient.Servers.First().TextChannels.Where(x => x.Name == "stream").First().SendMessage("Relay IRC#" + e.Source.Name + ": " + e.Text);
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

        private static void IrcClient_LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
        }

        private static void IrcClient_LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            
        }

        private static void IrcClient_Disconnected(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;
        }

        private static void IrcClient_Connected(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;
        }
    }
}

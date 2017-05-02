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
using Newtonsoft;
using RestSharp;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using Newtonsoft;
using System.Text.RegularExpressions;

namespace DeathmicChatbot.IRC
{
    class HitboxRelay
    {
        DiscordClient discordclient;
        public string sServer;
        public bool bTwoWay;
        public string sChannel;
        private static System.Timers.Timer discordreminder;
        Random RNG = new Random();
        public string sTargetChannel;
        public bool bDisconnected;
        private static System.Timers.Timer TimeoutTimer;
        System.Timers.Timer timer;
        public HitboxRelay()
        {
        }
        public HitboxRelay(DiscordClient discord, bool _bTwoWay = true, string channel = null,string _sTargetChannel = "botspam")
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed +=  new ElapsedEventHandler(SendMessageEvent);
            discordclient = discord;
            sChannel = channel;
            sTargetChannel = _sTargetChannel;
            bTwoWay = _bTwoWay;

        }

        public void StartRelayEnd()
        {
            TimeoutTimer = new System.Timers.Timer(10 * 60 * 1000);
            TimeoutTimer.Elapsed += OnTimeoutTimerEvent;
            TimeoutTimer.Start();//
        }


        private void OnTimeoutTimerEvent(object sender, ElapsedEventArgs e)
        {
            HitBox.Close();
        }

        public void StopRelayEnd()
        {
            if (TimeoutTimer != null)
            {
                TimeoutTimer.Stop();
                TimeoutTimer.Interval = 10 * 60 * 1000;
            }
        }
        private void SendMessageEvent(object sender, ElapsedEventArgs e)
        {
            if(QueuedMessage.Count > 0)
            {
                SendMessage(QueuedMessage.ElementAt(0));
                QueuedMessage.RemoveAt(0);
            }
            
        }

        WebSocket HitBox;
        bool Connected = false;
        string AuthToken = "";
        List<string> QueuedMessage = new List<string>();
        public void runBot()
        {
            var client = new RestClient("https://api.hitbox.tv");
            var request = new RestRequest("/chat/servers", Method.GET);
            IRestResponse response = client.Execute(request);
            JArray servers = JArray.Parse(response.Content);
            List<string> Servers = new List<string>();
            foreach(var _server in servers)
            {
                //JObject serveraddress = JObject.Parse(server.ToString());
                Servers.Add(_server.SelectToken("server_ip").ToString());
            }
            request = new RestRequest("/auth/login",Method.POST);
            request.AddParameter("login", "BobDeathmic", ParameterType.GetOrPost);
            request.AddParameter("pass", "Pgii9m87bfHCwtnuis8k", ParameterType.GetOrPost);
            request.AddParameter("app", "BobDeathmic", ParameterType.GetOrPost);
            response = client.Execute(request);

            string json = response.Content;
            json = json.Replace("5:::", "");
            json = json.Replace(@"\", "");
            json = json.Replace("[" + '"', "[");
            json = json.Replace('"' + "]", "]");
            DeathmicChatbot.HitboxAuth.Root Auth = Newtonsoft.Json.JsonConvert.DeserializeObject<DeathmicChatbot.HitboxAuth.Root>(json);
            AuthToken = Auth.authToken;

            string server = Servers[RNG.Next(0, Servers.Count)];
            client = new RestClient("http://"+ server);
            request = new RestRequest("/socket.io/1");
            response = client.Execute(request);
            string WebSocketID = response.Content.Split(':')[0];
            string WebSocketLink = "ws://" + server+"/socket.io/1/websocket/"+WebSocketID;
            HitBox = new WebSocket(WebSocketLink);
            HitBox.OnMessage += OnMessage;
            HitBox.Connect();
        }

        private void OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if(e.Data == "2::")
            {
                HitBox.Send(e.Data);
            }
            if(e.Data.Contains("5:::"))
            {
                try
                {
                    string json = e.Data.ToString();
                    json = json.Replace("5:::", "");
                    json = json.Replace(@"\", "");
                    json = json.Replace("[" + '"', "[");
                    json = json.Replace('"' + "]", "]");
                    DeathmicChatbot.HitBoxMessage.Root message = Newtonsoft.Json.JsonConvert.DeserializeObject<DeathmicChatbot.HitBoxMessage.Root>(json);
                    if(message.args.First().@params.name != "BobDeathmic")
                    {
                        if (message.args.First().method == "chatMsg")
                        {
                            discordclient.Servers.First().TextChannels.Where(x => x.Name.ToLower() == sTargetChannel.ToLower()).First().SendMessage("HitboxRelay " + message.args.First().@params.name + ": " + MessageParser(message.args.First().@params.text));
                        }
                    }
                }catch(Exception)
                {
                    // If Response probably not in Scope of Dev or Need be
                }
            }
            if(!Connected)
            {
                Connected = true;
                ConnectToChannel();
                discordclient.Servers.First().TextChannels.Where(x => x.Name.ToLower() == sTargetChannel.ToLower()).First().SendMessage("HitboxRelay Activated");
            }
        }
        private string MessageParser(string message)
        {
            if (message.Contains('@'))
            {
                Regex r = new Regex(@"@(\w+)");
                MatchCollection mc = r.Matches(message);
                foreach (Match match in mc)
                {
                    string sMatch = match.Value.Replace("@", "");
                    var users = discordclient.Servers.First().Users.Where(x => x.Name.ToLower() == sMatch.ToLower() || (x.Nickname != null && x.Nickname != "" && x.Nickname.ToLower() == sMatch.ToLower()));
                    if (users.Count() > 0)
                    {
                        return message.Replace(match.Value, users.First().Mention);
                    }
                }
            }
            return message;
        }
        public void RelayMessage(string message)
        {
            if(timer.Enabled == false)
            {
                timer.Start();
            }
            QueuedMessage.Add(message);
        }
        public void SendMessage(string message)
        {
            string Login = "5:::";
            JObject json = JObject.FromObject(new
            {
                name = "message",
                args = new
                {
                    method = "chatMsg",
                    @params = new
                    {
                        channel = sChannel,
                        name = "BobDeathmic",
                        nameColor = "FFFFFF",
                        text = message
                    }
                }
            });
            Login += json;
            HitBox.Send(Login);
        }
        public void ConnectToChannel()
        {
            string Login = "5:::";
            JObject json = JObject.FromObject(new {
                    name = "message",
                    args = new
                    {
                        method = "joinChannel",
                        @params = new
                        {
                            channel = sChannel,
                            name = "BobDeathmic",
                            token = AuthToken,
                            hideBuffered = true
                        }
                    }
            });
            Login += json;
            HitBox.Send(Login);
            SendMessage("Discord Relay Started");

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
    }
}

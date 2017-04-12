using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Threading;
using System.IO;
using DeathmicChatbot.StreamInfo;
using DeathmicChatbot.TransferClasses;
using DeathmicChatbot.Properties;
using DeathmicChatbot.DataFiles;
using System.Globalization;
using System.Xml;
using DeathmicChatbot.StreamInfo.Twitch;
using DeathmicChatbot.StreamInfo.Hitbox;
using DeathmicChatbot.IRC;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DeathmicChatbot.Discord
{
    class DiscordBob :IDisposable
    {
        private DiscordClient bot;
        #region global variable definition
        public List<DataFiles.User> LUserList = new List<DataFiles.User>();
        XMLProvider xmlprovider = new XMLProvider();
        private static StreamProviderManager _streamProviderManager;
        private List<string> ClosedCommandList = new List<string>();
        private List<string> OpenCommandList = new List<string>();
        private static readonly Random Rnd = new Random();
        String[] IgnoreTheseUsers = new String[] { "Q", "AUTH", "Global", "py-ctcp", "peer", Properties.Settings.Default.Name.ToString() };
        private static System.Timers.Timer VoteTimer;
        RelayBot deathmicirc;
        List<TwitchRelay> RelayBots = new List<TwitchRelay>();
        List<Thread> RelayThreads = new List<Thread>();
        #endregion
        public void Connect()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            CommandListInit();
            try
            {
                if (!Properties.Settings.Default.DateTimeFormatCorrected)
                {
                    //xmlprovider.DateTimeCorrection();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            _streamProviderManager = new StreamProviderManager();
            _streamProviderManager.StreamStarted += OnStreamStarted;
            _streamProviderManager.StreamStopped += OnStreamStopped;
            _streamProviderManager.StreamGlobalNotification += OnStreamGlobalNotification;
            _streamProviderManager.AddStreamProvider(new TwitchProvider());
            _streamProviderManager.AddStreamProvider(new HitboxProvider());
            bot = new DiscordClient();
            bot.MessageReceived += Message_Received;
            bot.UserJoined += User_Joined;
            //deathmicirc = new RelayBot(bot,false);
            //Thread RelayThread = new Thread(deathmicirc.runBot);
            //RelayThread.Start();
            //while (!RelayThread.IsAlive) ;
            //Thread.Sleep(1);

            readUsers();
            
            bot.ExecuteAndWait(async () =>
            {
                await bot.Connect("MjYwMTE2OTExOTczNDY2MTEy.CzhvyA.kpEIti2hVnjNIUccob0ERB4QFTw", TokenType.Bot);
            });
            
        }

        private void CommandListInit()
        {
            //Closed just means can only be used in botspam
            ClosedCommandList.Add("!addstream");
            ClosedCommandList.Add("!delstream");
            ClosedCommandList.Add("!resetStreamState");
            ClosedCommandList.Add("!streamcheck");
            ClosedCommandList.Add("!myvisits");
            ClosedCommandList.Add("!removealias");
            ClosedCommandList.Add("!checkusername");
            ClosedCommandList.Add("!addalias"); 
            ClosedCommandList.Add("!subscribablestreams");
            ClosedCommandList.Add("!toggleuserlogin");
            //ClosedCommandList.Add("!setpassword");
            ClosedCommandList.Add("!changesubscription"); 
            ClosedCommandList.Add("!reconnecttwitchrelays");
            ClosedCommandList.Add("!startvoting");
            ClosedCommandList.Add("!endvoting");
            ClosedCommandList.Add("!vote");
            ClosedCommandList.Add("!listvotings");
            ClosedCommandList.Add("!changetwitchchat");
            ClosedCommandList.Add("!changeglobalannouncement");
            ClosedCommandList.Add("!addmyuser");

            OpenCommandList.Add("!help"); 
            OpenCommandList.Add("!roll");
            OpenCommandList.Add("!pickrandomuser");
            OpenCommandList.Add("!removeuserpicklist");
            OpenCommandList.Add("!userpicklist");
            OpenCommandList.Add("!counter");

        }

        public void readUsers()
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "/XML/Usersv2.xml"))
            {
                while (UserListLocked)
                {
                    Thread.Sleep(1000);
                }
                UserListLocked = true;
                var path = Directory.GetCurrentDirectory() + "/XML/Usersv2.xml";
                FileStream fs = new FileStream(path, FileMode.Open);
                System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(LUserList.GetType());


                XmlReader reader = XmlReader.Create(fs);
                LUserList = (List<DataFiles.User>)xmlserializer.Deserialize(reader);
                fs.Close();
                UserListLocked = false;
                updateUsers();
            }
        }
        public void updateUsers()
        {
            string streams = xmlprovider.StreamList("",true);
            List<string> Streams = streams.Split(',').ToList();
            foreach(string stream in Streams)
            {
                DataFiles.Stream addstream = new DataFiles.Stream();
                addstream.hourlyannouncement = false;
                addstream.name = stream;
                addstream.subscribed = true;
                var users = LUserList.Where(x => x.hasStream(stream) == false);
                foreach(DataFiles.User user in users)
                {
                    user.Streams.Add(addstream);
                }
            }
            SaveUserList();
        }
        private void User_Joined(object sender, UserEventArgs e)
        {
            if (Properties.Settings.Default.SilentMode.ToString() != "true")
            {
                if (xmlprovider == null)
                {
                    xmlprovider = new XMLProvider();
                }
                #region whisperstatsonjoin
                NormalisedUser normaliseduser = new NormalisedUser();
                normaliseduser.orig_username = e.User.Name;
                IEnumerable<DataFiles.User> joineduser = LUserList.Where(x => x.isUser(normaliseduser.normalised_username()));
                if (joineduser.Count() > 0)
                {
                    Tuple<DateTime, int> VisitData = joineduser.First().visit();
                    string visitstring = "";
                    switch (VisitData.Item2)
                    {
                        case 1: visitstring = VisitData.Item2 + "st"; break;
                        case 2: visitstring = VisitData.Item2 + "nd"; break;
                        case 3: visitstring = VisitData.Item2 + "rd"; break;
                        default: visitstring = VisitData.Item2 + "th"; break;
                    }
                    String days_since_last_visit = DateTime.Now.Subtract(Convert.ToDateTime(VisitData.Item1)).ToString("d' days 'h':'mm':'ss");
                    String output = "This is " + normaliseduser.normalised_username() + "'s " + visitstring + " visit. Their last visit was on " + VisitData.Item1.ToString("dd-MM-yyyy HH:mm:ss") + " (" + days_since_last_visit + " ago)";
                    bot.Servers.First().AllChannels.Where(x => x.Name == "botspam").First().SendMessage(output);
                }
                else
                {
                    String output = "This is " + normaliseduser.normalised_username() + "'s first visit.";
                    bot.Servers.First().AllChannels.Where(x => x.Name == "botspam").First().SendMessage(output);
                    DataFiles.User newUser = new DataFiles.User();
                    XDocument Streams = XDocument.Load(Directory.GetCurrentDirectory() + "/XML/Streams.xml");
                    IEnumerable<XElement> streamchildren = from streams in Streams.Root.Elements() select streams;
                    foreach (var stream in streamchildren)
                    {
                        newUser.addStream(stream.Attribute("Channel").Value, true);
                    }
                    newUser.Name = normaliseduser.normalised_username();
                    newUser.bIsLoggingOp = false;
                    newUser.password = "";
                    newUser.bMessages = false;
                    newUser.visit();
                    LUserList.Add(newUser);
                    // TODO: Suscribe to all streams;
                }
                #endregion
                SaveUserList();
                foreach (string streamname in xmlprovider.OnlineStreamList())
                {
                    string _streamname = streamname.Replace(",", "");
                    DataFiles.User user = getUser(normaliseduser.normalised_username().ToLower());
                    if (user != null)
                        if (user.isSubscribed(_streamname))
                        {
                            e.User.SendMessage(String.Format(
                                                        "Stream running: _{0}_ ({1}) at {2}",
                                                        _streamname,
                                                        xmlprovider.StreamInfo(_streamname, "game"),
                                                        xmlprovider.StreamInfo(_streamname, "URL")
                                                        ));
                        }
                }
                normaliseduser = null;
            }
        }

        private void Message_Received(object sender, MessageEventArgs e)
        {
            if(CommandFilter(sender,e))
            {

            }
            else
            {
                string user = e.User.Name;
                string message = e.Message.RawText;
                if(e.Channel.Name != "botspam" || e.Channel.Name != "meta" || !e.Channel.Name.Contains(e.User.Name))
                {
                    if(deathmicirc != null)
                    {
                        deathmicirc.RelayMessage(message, user);
                    }
                    if(e.User.Name != "BobDeathmic")
                    {
                        foreach (TwitchRelay relay in RelayBots)
                        {
                            if (relay.bTwoWay && !relay.bDisconnected)
                            {
                                try
                                {
                                    if (e.Channel.Name == relay.sTargetChannel)
                                    {
                                        if (e.User.Nickname != null && e.User.Nickname != "")
                                        {
                                            relay.RelayMessage("DiscordRelay " + e.User.Nickname + ": " + message);
                                        }
                                        else
                                        {
                                            relay.RelayMessage("DiscordRelay " + e.User.Name + ": " + message);
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
        private bool CommandFilter(object sender, MessageEventArgs e)
        {
            string messagecontent = e.Message.RawText;
            List<String> parameters = messagecontent.Split(' ').ToList();
            parameters.RemoveAt(0);
            bool command = false;
            #region ClosedCommands
            if(e.Channel.ToString() == "botspam" || e.Channel.ToString().ToLower().Contains(e.User.Name.ToLower()))
            {
                //Insert anything that has to do with management here
                if (messagecontent.ToLower().StartsWith("!troll") && e.User.Name == "chromos33")
                {
                    Troll(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.StartsWith("(╯°□°）╯︵ ┻━┻"))
                {
                    SaveTable(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!addstream"))
                {
                    AddStream(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!delstream"))
                {
                    DelStream(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!resetStreamState"))
                {
                    ResetStreamState();
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!streamcheck"))
                {
                    StreamCheck(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!myvisits"))
                {
                    MyVisits(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!removealias"))
                {
                    RemoveAlias(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!checkusername"))
                {
                    CheckUserName(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!addalias"))
                {
                    AddAlias(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!subscribablestreams"))
                {
                    SuscribableStreams(sender, e, parameters);
                    command = true;
                }
                /*if (messagecontent.ToLower().StartsWith("!setpassword"))
                {
                    SetPassword(sender, e, parameters);
                    command = true;
                }*/
                if (messagecontent.ToLower().StartsWith("!changesubscription"))
                {
                    ChangeSubscription(sender, e, parameters);
                    command = true;
                }

                if (messagecontent.ToLower().StartsWith("!startvoting"))
                {
                    StartVote(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!endvoting"))
                {
                    EndVote(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!vote"))
                {
                    Vote(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!listvotings"))
                {
                    ListVotes(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!changetwitchchat"))
                {
                    ChangeTwitchChat(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!forcetwitchchat"))
                {
                    ForceTwitchChat(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!reconnecttwitchrelays"))
                {
                    ReconnectTwitchRelays(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!disconnectwitchchat"))
                {
                    DisconnectTwitchChat(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!changeglobalannouncement"))
                {
                    ChangeGlobalAnnouncment(sender, e, parameters);
                    command = true;
                }
                if (messagecontent.ToLower().StartsWith("!addmyuser"))
                {
                    AddMyUser(sender, e, parameters);
                    command = true;
                }
            }
            #endregion
            #region OpenCommands
            // Universally usable commands
            if (messagecontent.ToLower().StartsWith("!help"))
            {
                ShowCommandList(sender, e, parameters);
                command = true;
            }
            if (messagecontent.ToLower().StartsWith("!roll"))
            {
                Roll(sender, e, parameters);
                command = true;
            }

            if (messagecontent.ToLower().StartsWith("!pickrandomuser"))
            {
                PickRandomUser(sender, e, parameters);
                command = true;
            }
            if (messagecontent.ToLower().StartsWith("!removeuserpicklist"))
            {
                RemoveUserPickList(sender, e, parameters);
                command = true;
            }
            if (messagecontent.ToLower().StartsWith("!userpicklist"))
            {
                UserPickList(sender, e, parameters);
                command = true;
            }
            if (messagecontent.ToLower().StartsWith("!counter"))
            {
                Counter(sender, e, parameters);
                command = true;
            }
            #endregion
            return command;
        }

        private void ReconnectTwitchRelays(object sender, MessageEventArgs e, List<string> parameters)
        {
            foreach(TwitchRelay relay in RelayBots)
            {
                if(!relay.isExit)
                {
                    relay.DisconnectRelay();
                }
            }
        }

        private void DisconnectTwitchChat(object sender, MessageEventArgs e, List<string> parameters)
        {
            if (RelayBots.Where(x => x.sChannel.ToLower() == "deathmic").Count() > 0)
            {
                RelayBots.Where(x => x.sChannel.ToLower() == "deathmic").First().DisconnectRelay();
            }
        }

        private void ConnectToTwitchChat(string channel,bool isTwitch)
        {
            //Warning this function simply connects or reconnect make sure to only execute when relay does not exist
            try
            {
                if(isTwitch)
                {
                    Tuple<string, bool> temp = xmlprovider.GetTwitchChatData(channel);
                    bool twoway = temp.Item2;
                    if (temp.Item1 != "")
                    {
                        TwitchRelay tmpbot;
                        if (RelayBots.Where(x => x.sChannel.ToLower() == channel).Count() > 0)
                        {
                            tmpbot = RelayBots.Where(x => x.sChannel.ToLower() == channel).First();
                            tmpbot.sTargetChannel = temp.Item1;
                            tmpbot.bTwoWay = twoway;
                        }
                        else
                        {
                            tmpbot = new TwitchRelay(bot, channel, temp.Item1, twoway);
                        }
                        RelayBots.Add(tmpbot);
                        Thread RelayThread = new Thread(tmpbot.ConnectToTwitch);
                        RelayThread.Start();
                        while (!RelayThread.IsAlive) ;
                        Thread.Sleep(1);
                        RelayThreads.Add(RelayThread);
                    }
                }
            }catch(Exception)
            {

            }
            
        }
        #region Commands
        #region StreamFunctions
        //Command that force connects bot to Twitch IRC for testing only
        private void Troll(object sender, MessageEventArgs e, List<string> parameters)
        {
            e.Channel.Users.Where(x => x.Name.ToLower() == "chromos33").First().SendMessage("Stream started: deathmic(Eliots Quest: Pew Pew) at http://www.twitch.tv/deathmic");
            e.Channel.Users.Where(x => x.Name.ToLower() == "niheka").First().SendMessage("Stream started: deathmic(Eliots Quest: Pew Pew) at http://www.twitch.tv/deathmic");
        }
        private void SaveTable(object sender, MessageEventArgs e, List<string> parameters)
        {
            e.Channel.SendMessage("┬─┬﻿ ノ( ゜-゜ノ)");
        }
        private void ChangeGlobalAnnouncment(object sender, MessageEventArgs e, List<string> parameters)
        {
            if(parameters.Count() == 0 && parameters.Count() != 2 && parameters.Count() != 3 || parameters[0] == "help" && parameters.Count() > 0)
            {
                e.User.SendMessage("Command !changeglobalannouncement [global/stream] [enable/disable/read] [streamname (only with stream as first parameter)]");
                return;
            }
            try
            {
                if (parameters[0] == "global" && parameters.Count() == 2)
                {
                    if (parameters[1] == "enable")
                    {
                        var Users = LUserList.Where(x => x.Name.Equals(e.User.Name, StringComparison.InvariantCultureIgnoreCase));
                        if (Users.Count() > 0)
                        {
                            Users.First().globalhourlyannouncement = true;
                            e.User.SendMessage("global hourly enabled");
                        }
                    }
                    if (parameters[1] == "disable")
                    {
                        var Users = LUserList.Where(x => x.Name.Equals(e.User.Name, StringComparison.InvariantCultureIgnoreCase));
                        if (Users.Count() > 0)
                        {
                            Users.First().globalhourlyannouncement = false;
                            e.User.SendMessage("global hourly disabled");
                        }
                    }
                    if (parameters[1] == "read")
                    {
                        var Users = LUserList.Where(x => x.Name.Equals(e.User.Name, StringComparison.InvariantCultureIgnoreCase));
                        if (Users.Count() > 0)
                        {
                            e.User.SendMessage(Users.First().globalhourlyannouncement.ToString());
                        }
                    }
                    SaveUserList();
                    return;
                }
                if (parameters[0] == "stream" && parameters.Count() == 3)
                {
                    if (parameters[1] == "enable")
                    {
                        var Users = LUserList.Where(x => x.Name.Equals(e.User.Name,StringComparison.InvariantCultureIgnoreCase));
                        if (Users.Count() > 0)
                        {
                            Users.First().Streams.Where(x => x.name.ToLower() == parameters[2]).First().hourlyannouncement = true;
                            e.User.SendMessage("stream hourly enabled");
                        }
                    }
                    if (parameters[1] == "disable")
                    {
                        var Users = LUserList.Where(x => x.Name.Equals(e.User.Name, StringComparison.InvariantCultureIgnoreCase));
                        if (Users.Count() > 0)
                        {
                            Users.First().Streams.Where(x => x.name.ToLower() == parameters[2]).First().hourlyannouncement = false;
                            e.User.SendMessage("stream hourly disabled");
                        }
                    }
                    if (parameters[1] == "read")
                    {
                        var Users = LUserList.Where(x => x.Name.Equals(e.User.Name, StringComparison.InvariantCultureIgnoreCase));
                        if (Users.Count() > 0)
                        {
                            e.User.SendMessage(Users.First().Streams.Where(x => x.name.ToLower() == parameters[2]).First().hourlyannouncement.ToString());
                        }
                    }
                    SaveUserList();
                    return;
                }
            }
            catch(Exception)
            {
                e.User.SendMessage("Error");
            }
        }
        private void ForceTwitchChat(object sender, MessageEventArgs e, List<string> parameters)
        {
            
        }
        private void ChangeTwitchChat(object sender, MessageEventArgs e, List<string> parameters)
        {
            if(parameters[0] == "help" || parameters.Count() == 0)
            {
                e.User.SendMessage("Command !changetwitchchat [streamname] [targetchannel(Discord)] [(optional default 1) twoway (0/1)]");
            }
            else
            {
                int twoway = 1;
                if(parameters.Count() == 3)
                {
                    twoway = Int32.Parse(parameters[2]);
                }
                string streamname = parameters[0];
                string targetchannel = parameters[1];
                int result = xmlprovider.TwitchChatToStream(streamname,targetchannel,twoway);
                if(result == 0)
                {
                    e.User.SendMessage("Some Error Occured");
                }
                else
                {
                    e.User.SendMessage("Details changed");
                }
            }

        }
        private void AddStream(object sender, MessageEventArgs e,List<string> parameters)
        {
            if (parameters.Count > 0)
            {
                _streamProviderManager.AddStream(parameters[0]);
            }
            int message = xmlprovider.AddStream(parameters[0]);
            if (message == 1)
            {
                //Add Streams to user
                foreach (DataFiles.User user in LUserList.Where(x => x.bShouldSubscribe()))
                {
                    user.addStream(parameters[0], true);
                }
                SaveUserList();

                e.Channel.SendMessage(String.Format("{0} added {1} to the streamlist", e.User.Name, parameters[0]));
            }
            else if (message == 2)

            {
                e.Channel.SendMessage(String.Format("{0} you idiot stream {1} is already in the streamlist", e.User.Name, parameters[0]));
            }
            else if (message == 0)
            {
                e.Channel.SendMessage("there has been an error please contact an programmer.");
            }
        }
        private void DelStream(object sender, MessageEventArgs e, List<string> parameters)
        {
            string message = xmlprovider.RemoveStream(parameters[0]);
            e.User.SendMessage(message);
        }
        private void ResetStreamState()
        {
            xmlprovider.ResetStreamState();
        }
        private void StreamCheck(object sender, MessageEventArgs e, List<string> parameters)
        {
            foreach (var stream in xmlprovider.OnlineStreamList())
            {
                string[] streamprovidersplit = stream.Split(',');

                List<NormalisedUser> UsersinChannel = new List<NormalisedUser>();
                NormalisedUser normuser = new NormalisedUser(e.User.Name.ToString());
                DataFiles.User userUser = getUser(normuser.normalised_username().ToLower());
                if (userUser != null)
                    if (userUser.isSubscribed(stream))
                    {
                        e.User.SendMessage(streamprovidersplit[0] + " is currently streaming " + xmlprovider.StreamInfo(streamprovidersplit[0], "game") + " at " + xmlprovider.StreamInfo(streamprovidersplit[0], "URL"));
                    }
                    else
                    {
                        e.User.SendMessage("No Stream is currently running.");
                    }
            }
            if (xmlprovider.OnlineStreamList().Count() == 0)
            {
                e.User.SendMessage("No Stream is currently running.");
            }
        }

        #endregion
        #region UserFunctions
        private void AddMyUser(object sender, MessageEventArgs e, List<string> parameters)
        {
            if(LUserList.Where(x => x.Name.ToLower() == e.User.Name.ToLower()).Count()>0)
            {
                if(LUserList.Where(x => x.Streams.Count()==0).Count() != 0)
                {
                    List<DataFiles.Stream> streamlist = new List<DataFiles.Stream>();
                    foreach(string _stream in xmlprovider.StreamList("", true).Split(','))
                    {
                        DataFiles.Stream newstream = new DataFiles.Stream();
                        newstream.hourlyannouncement = false;
                        newstream.name = _stream;
                        newstream.subscribed = true;
                        streamlist.Add(newstream);
                    }
                    LUserList.Where(x => x.Streams.Count() == 0).First().Streams = streamlist;
                    e.User.SendMessage("User already exists but had no streams attached");
                    SaveUserList();
                    return;
                }
                e.User.SendMessage("User already exists");
            }
            else
            {
                DataFiles.User newuser = new DataFiles.User();
                newuser.Name = e.User.Name.ToLower();
                LUserList.Add(newuser);
                SaveUserList();
                e.User.SendMessage("User added");
            }
        }
        private void MyVisits(object sender, MessageEventArgs e, List<string> parameters)
        {
            NormalisedUser normaliseduser = new NormalisedUser();
            normaliseduser.orig_username = e.User.Name;
            IEnumerable<DataFiles.User> joineduser = LUserList.Where(x => x.isUser(normaliseduser.normalised_username()));
            if (joineduser.Count() > 0)
            {
                e.User.SendMessage(joineduser.First().VisitCounter + " visits");
            }
        }
        private void ShowCommandList(object sender, MessageEventArgs e, List<string> parameters)
        {
            string output = "botspam/private Commands: ";
            foreach(string command in ClosedCommandList)
            {
                output += command + ", ";
            }
            output = output.Substring(0, output.Length - 2);
            output += "; public Commands:";
            foreach(string command in OpenCommandList)
            {
                output += command + ", ";
            }
            e.User.SendMessage(output);
        }
        private void RemoveAlias(object sender, MessageEventArgs e, List<string> parameters)
        {
            if (parameters.Count() > 0)
            {
                if (parameters[0].ToString() == "help")
                {
                    e.User.SendMessage("!removealias AliasName");
                    e.User.SendMessage("This removes the Alias from your User if possible");
                    e.User.SendMessage("Passwort is only essential if set");
                    return;
                }
                string password = "";
                if (parameters.Count() == 2)
                {
                    password = parameters[1];
                }

                NormalisedUser normuser = new NormalisedUser(e.User.Name.ToString());
                NormalisedUser alias = new NormalisedUser(parameters[0]);
                DataFiles.User user = getUser(normuser.normalised_username()).RemoveAlias(alias.normalised_username());
                List<DataFiles.internalStream> Streams = xmlprovider.getStreams();
                foreach (var stream in Streams)
                {
                    user.addStream(stream.sChannel, true);
                }
                LUserList.Add(user);
                if (user != null)
                {
                    e.User.SendMessage("Alias Removed");
                }
                else
                {
                    e.User.SendMessage("Error");
                }

            }
            SaveUserList();
        }
        private void CheckUserName(object sender, MessageEventArgs e, List<string> parameters)
        {
            NormalisedUser normuser = new NormalisedUser(e.User.Name.ToString());
            e.User.SendMessage(normuser.normalised_username());
        }
        private void AddAlias(object sender, MessageEventArgs e, List<string> parameters)
        {
            if (parameters.Count() > 0)
            {
                if (parameters[0].ToString() == "help")
                {
                    e.User.SendMessage("!addalias AliasName");
                    e.User.SendMessage("This adds the Alias to your User if possible");
                    e.User.SendMessage("Passwort is only essential if set");
                    return;
                }
                string password = "";
                if (parameters.Count() == 2)
                {
                    password = parameters[1];
                }

                NormalisedUser normuser = new NormalisedUser(e.User.Name.ToString());
                NormalisedUser alias = new NormalisedUser(parameters[0]);
                DataFiles.User user = getUser(normuser.normalised_username().ToLower());
                if (user != null)
                    if (user.AddAlias(alias.normalised_username(), LUserList))
                    {
                        e.User.SendMessage("Alias Added");
                    }
                    else
                    {
                        e.User.SendMessage("Error");
                    }
            }
            SaveUserList();
        }
        private void SuscribableStreams(object sender, MessageEventArgs e, List<string> parameters)
        {
            e.User.SendMessage(xmlprovider.StreamList());
        }
        private void SetPassword(object sender, MessageEventArgs e, List<string> parameters)
        {
            bool result = false;
            if (parameters[0] == "help" || parameters.Count() == 0)
            {
                e.User.SendMessage("Password Command: '!setpass [New Password] [old Pass]' old Pass is optional on the First time");
                return;
            }
            string password = "";
            if (parameters.Count == 2)
            {
                password = parameters[1];
            }
            result = getUser(NormalizeNickName(e.User.Name)).changePassword(parameters[0], password);
            if (result)
            {
                e.User.SendMessage("new Password Confirmed");
            }
            else
            {
                e.User.SendMessage("Entered current Password is incorrect or not entered, type !SetPassword help for the command");
            }
            SaveUserList();
        }
        private void ChangeSubscription(object sender, MessageEventArgs e, List<string> parameters)
        {
            bool result = false;
            if (parameters[0] == "help" || parameters.Count() == 0)
            {
                e.User.SendMessage("To update Subscripions use following structure '!changesubscription [add/remove] [streamname]");
                return;
            }
            if (parameters.Count() == 3 || parameters.Count() == 2)
            {
                string password = "";
                if (parameters.Count() == 3)
                {
                    password = parameters[2];
                }
                if (parameters[0] == "remove")
                {
                    result = getUser(NormalizeNickName(e.User.Name)).subscribeStream(parameters[1].ToLower(), password, false);
                }
                if (parameters[0] == "add")
                {
                    result = getUser(NormalizeNickName(e.User.Name)).subscribeStream(parameters[1].ToLower(), password, true);
                }
            }
            if (result)
            {
                e.User.SendMessage( "Subscription Changed");
            }
            else
            {
                e.User.SendMessage("Either Subscription not found or password is wrong");
            }
            SaveUserList();
        }
        #endregion
        #region ChatFunctions
        private void Roll(object sender, MessageEventArgs e, List<string> parameters)
        {
            var regex = new Regex(@"(^\d+)[wWdD](\d+$)");
            if (!regex.IsMatch(parameters[0]))
            {
                e.Channel.SendMessage(String.Format("Error: Invalid roll request: {0}.",parameters[0]));
            }
            else
            {
                var match = regex.Match(parameters[0]);

                UInt64 numberOfDice;
                UInt64 sidesOfDice;

                try
                {
                    sidesOfDice = Convert.ToUInt64(match.Groups[2].Value);
                    numberOfDice = Convert.ToUInt64(match.Groups[1].Value);
                }
                catch (OverflowException)
                {
                    e.Channel.SendMessage("Error: Result could make the server explode. Get real, you maniac.");
                    return;
                }

                if (numberOfDice == 0 || sidesOfDice == 0)
                {
                    e.Channel.SendMessage(string.Format("Error: Can't roll 0 dice, or dice with 0 sides."));
                    return;
                }

                if (sidesOfDice >= Int32.MaxValue)
                {
                    e.Channel.SendMessage(string.Format("Error: Due to submolecular limitations, a die can't have more than {0} sides.",Int32.MaxValue - 1));
                    return;
                }

                UInt64 sum = 0;

                var random = new Random();

                var max = numberOfDice * sidesOfDice;
                if (max / numberOfDice != sidesOfDice)
                {
                    e.Channel.SendMessage("Error: Result could make the server explode. Get real, you maniac.");
                    return;
                }

                if (numberOfDice > 100000000)
                {
                    e.Channel.SendMessage("Seriously? ... I'll try. But don't expect the result too soon. It's gonna take me a while.");
                }

                for (UInt64 i = 0; i < numberOfDice; i++)
                    sum += (ulong)random.Next(1, Convert.ToInt32(sidesOfDice) + 1);

                e.Channel.SendMessage(String.Format("{0}: {1}", parameters[0],sum));
            }
        }
        private void Counter(object sender, MessageEventArgs e, List<string> parameters)
        {
            if (parameters[0] == "help" || parameters.Count() == 0)
            {
                e.User.SendMessage( "Error: count needs a counter name. '!counter [countername] [command(read/reset)]' [command] is optional.");
                return;
            }
            if(parameters[0] == "list")
            {
                e.User.SendMessage(xmlprovider.CounterList());
                return;
            }
            if (parameters.Count() == 1)
            {
                e.Channel.SendMessage(xmlprovider.Counter(parameters[0].ToString()));
            }
            else if (parameters.Count() >= 2)
            {
                int custom = 0;
                if (parameters.Count() > 2)
                {
                    e.User.SendMessage( "Warning: Only 2 Parameters needed, ignoring parameters after 2nd.");
                }
                if (parameters[1] == "read")
                {
                    e.Channel.SendMessage(xmlprovider.Counter(parameters[0].ToString(), false, true));
                }
                else if (parameters[1] == "reset")
                {
                    e.User.SendMessage(xmlprovider.Counter(parameters[0].ToString(), true));
                }
                else if (Int32.TryParse(parameters[1],out custom))
                {
                    e.User.SendMessage(xmlprovider.Counter(parameters[0].ToString(), false,false,custom));
                }
                else
                {
                    e.Channel.SendMessage("command has to be either read or reset, if you just want to advance the count use '!counter [countername]'.");
                }
            }
        }
        #region RandomUser
        private int CheckPickRandomUserParam(string param)
        {
            if (param.IndexOf("#") >= 0)
            {
                return 1;
            }
            if (param.IndexOf("R_") >= 0 || param.IndexOf("r_") >= 0)
            {
                return 2;
            }
            if (param.IndexOf("Ig_") >= 0 || param.IndexOf("ig_") >= 0 || param.IndexOf("IG_") >= 0)
            {
                return 3;
            }
            return 0;
        }
        private void UserPickList(object sender, MessageEventArgs e, List<string> parameters)
        {
            try
            {
                string output;
                if (parameters.Count() > 0)
                {
                    if (parameters[0] != "")
                    {
                        output = xmlprovider.ReasonUserList(parameters[0]);
                    }
                    else
                    {
                        output = xmlprovider.ReasonUserList();
                    }
                    if (output == "")
                    {
                        e.User.SendMessage("No Result from this query.");
                    }
                    else
                    {
                        e.User.SendMessage(output);
                    }
                }
                else
                {
                    e.User.SendMessage("No Result from this query (Create Pick with Reasons first).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void RemoveUserPickList(object sender, MessageEventArgs e, List<string> parameters)
        {
            if (parameters.Count() > 0)
            {
                List<string> checkparams = parameters.ToList();
                if (checkparams.Count() > 1)
                {
                    if (checkparams[checkparams.Count() - 1] == "")
                    {
                        checkparams.RemoveAt(checkparams.Count() - 1);
                    }
                }
                if (checkparams[0] == "help")
                {
                    e.User.SendMessage("Command is !removeuserpicklist [nameofpickedlist]");
                }
                else
                {

                    if (xmlprovider.DeletePickData(checkparams[0]))
                    {
                        e.User.SendMessage("Removal succeeded");
                    }
                    else
                    {
                        e.User.SendMessage("No such List in the Data");
                    }

                }
            }
            else
            {
                e.User.SendMessage("You must enter a List to be removed");
            }
        }

        private void PickRandomUser(object sender, MessageEventArgs e, List<string> parameters)
        {
            try
            {
                List<string> unfilteredTargets = new List<string>();
                foreach (var target in e.Channel.Users)
                {
                    unfilteredTargets.Add(target.Name);
                }

                List<string> checkparams = parameters.ToList();
                List<string> filteredTargets = new List<string>();
                List<string> pickeduseroutput = new List<string>();
                bool multiple = false;
                string multiplevalue = "";
                bool reason = false;
                string reasonvalue = "";
                bool additionalignores = false;
                string additionalignoresvalue = "";
                if (checkparams.Count() > 0)
                {
                    if (checkparams[0].ToString() == "help")
                    {
                        e.User.SendMessage("The command to for PickRandomUser looks like this: '!PickRandomUser #[Number of Picks] R_[Reason] Ig_[Ignored User 1],[Ignored User 2]... no space'.");
                        e.User.SendMessage("All parameter (and Order) are optional. [Reason] saves Picks into XML for later use filtering");
                    }
                    else
                    {
                        if (checkparams.Count() == 0 || checkparams.Count() >= 4)
                        {
                            e.User.SendMessage("The command to for PickRandomUser looks like this: '!PickRandomUser #[Number of Picks] R_[Reason] Ig_[Ignored User 1],[Ignored User 2]... no space'.");
                            e.User.SendMessage("All parameter (and Order) are optional. [Reason] saves Picks into XML for later use filtering");
                            return;
                        }
                        if (checkparams[checkparams.Count() - 1] == "")
                        {
                            checkparams.RemoveAt(checkparams.Count() - 1);
                        }
                        for (int i = 0; i < checkparams.Count(); i++)
                        {
                            switch (CheckPickRandomUserParam(checkparams[i].ToString()))
                            {
                                case 1:
                                    multiple = true;
                                    multiplevalue = checkparams[i].ToString().Substring(1, checkparams[i].Length - 1);
                                    break;
                                case 2:
                                    reason = true;
                                    reasonvalue = checkparams[i].ToString().Substring(2, checkparams[i].Length - 2);
                                    break;
                                case 3:
                                    additionalignores = true;
                                    additionalignoresvalue = checkparams[i].ToString().Substring(3, checkparams[i].Length - 3);
                                    break;
                                default:
                                    e.User.SendMessage("The command to for PickRandomUser looks like this: '!PickRandomUser #[Number of Picks] R_[Reason] Ig_[Ignored User 1],[Ignored User 2]... no space'.");
                                    e.User.SendMessage("All parameter (and Order) are optional. [Reason] saves Picks into XML for later use filtering");
                                    return;
                            }
                        }
                        string pickeduser = "";
                        try
                        {
                            string[] tempIgnoreTheseUsers = IgnoreTheseUsers;
                            if (additionalignores)
                            {
                                foreach (string item in additionalignoresvalue.Split(','))
                                {
                                    tempIgnoreTheseUsers = Useful_Functions.String_Array_Push(tempIgnoreTheseUsers, item);
                                }
                            }
                            foreach (var target in unfilteredTargets)
                            {
                                if (!tempIgnoreTheseUsers.Contains(target))
                                {
                                    filteredTargets.Add(target);
                                }
                            }
                            if (multiple && reason)
                            {
                                int tries = int.Parse(multiplevalue);
                                if (int.Parse(multiplevalue) > filteredTargets.Count())
                                {
                                    tries = filteredTargets.Count();
                                }

                                if (filteredTargets.Count() > 0)
                                {
                                    foreach (string user in filteredTargets.ToList())
                                    {
                                        if (xmlprovider.CheckforUserinPick(reasonvalue, user))
                                        {
                                            filteredTargets.Remove(user);
                                        }
                                        if (filteredTargets.Count() == 0)
                                        {
                                            e.Channel.SendMessage("No Users left that have not been chosen yet or that are in the ignore filter");
                                            return;
                                        }
                                    }
                                    for (int i = 0; tries > i; i++)
                                    {
                                        pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                        xmlprovider.CreateUserPick(reasonvalue, pickeduser);
                                        pickeduseroutput.Add(pickeduser);
                                        filteredTargets.Remove(pickeduser);

                                        if (filteredTargets.Count() == 0)
                                        {
                                            e.User.SendMessage("Not enough Users left in the Channel to completely fulfill your request.");
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    e.Channel.SendMessage("No Users left after Ignorefilter");
                                    return;
                                }
                            }
                            else if (multiple)
                            {
                                if (filteredTargets.Count() > 0)
                                {
                                    int tries = int.Parse(multiplevalue);
                                    if (int.Parse(multiplevalue) > filteredTargets.Count())
                                    {
                                        tries = filteredTargets.Count();
                                    }
                                    for (int i = 0; tries > i; i++)
                                    {
                                        pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                        pickeduseroutput.Add(pickeduser);
                                        filteredTargets.Remove(pickeduser);
                                        if (filteredTargets.Count() == 0)
                                        {
                                            e.User.SendMessage("Not enough Users left in the Channel to completely fulfill your request.");
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    e.Channel.SendMessage("No Users left after Ignorefilter");
                                    return;
                                }

                            }
                            else if (reason)
                            {
                                if (filteredTargets.Count() > 0)
                                {
                                    pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                    if ((xmlprovider.CreateUserPick(reasonvalue, pickeduser)))
                                    {
                                        pickeduseroutput.Add(pickeduser);
                                    }
                                }
                                else
                                {
                                    e.Channel.SendMessage("No Users left after Ignorefilter");
                                    return;
                                }
                            }
                            else
                            {
                                if (filteredTargets.Count() > 0)
                                {
                                    pickeduser = filteredTargets[Rnd.Next(filteredTargets.Count())];
                                    if ((xmlprovider.CreateUserPick(reasonvalue, pickeduser)))
                                    {
                                        pickeduseroutput.Add(pickeduser);
                                    }
                                }
                                else
                                {
                                    e.Channel.SendMessage("No Users left after Ignorefilter");
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            e.User.SendMessage("The command to for PickRandomUser looks like this: '!PickRandomUser #[Number of Picks] R_[Reason] Ig_[Ignored User 1],[Ignored User 2]... no space'.");
                            e.User.SendMessage("All parameter (and Order) are optional. [Reason] saves Picks into XML for later use filtering");
                            return;
                        }
                    }
                }
                else
                {
                    if (unfilteredTargets.Count() > 0)
                    {
                        bool add = true;
                        foreach (string element in unfilteredTargets)
                        {
                            if (!IgnoreTheseUsers.Contains(element))
                            {
                                filteredTargets.Add(element);
                            }
                        }
                        if (add)
                        {
                            pickeduseroutput.Add(filteredTargets[Rnd.Next(filteredTargets.Count())]);
                        }
                    }
                    else
                    {
                        //This gets triggered even if there should still be stuff
                        e.Channel.SendMessage("No Users left after Ignorefilter");
                        return;
                    }
                }
                string output = "";
                int j = 1;
                foreach (string finalusers in pickeduseroutput)
                {
                    if (j < pickeduseroutput.Count())
                    {
                        output += finalusers + ",";
                    }
                    else
                    {
                        output += finalusers;
                    }
                    j++;
                }
                if (output != "")
                {
                    e.Channel.SendMessage("The following User/s have been chosen:" + output);
                }
                else
                {
                    if (checkparams.Count() > 0)
                    {
                        if (!(checkparams[0].ToString() == "help"))
                        {
                            e.Channel.SendMessage("There are no more people, that haven't been chosen already for this Reason:" + reasonvalue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                e.User.SendMessage("#[Number of Picks] must be a number, or some other error occured");
            }
        }
        #endregion
        #region Voting
            #region functions
        private int CheckVotingParam(string param)
        {
            if (param.IndexOf("t_") >= 0 || param.IndexOf("T_") >= 0)
            {
                return 1;
            }
            if (param.IndexOf("d_") >= 0 || param.IndexOf("D_") >= 0)
            {
                return 2;
            }
            if (param.IndexOf("q_") >= 0 || param.IndexOf("Q_") >= 0)
            {
                return 3;
            }
            if (param.IndexOf("a_") >= 0 || param.IndexOf("A_") >= 0)
            {
                return 4;
            }
            if (param.IndexOf("m_") >= 0 || param.IndexOf("M_") >= 0)
            {
                return 5;
            }
            return 0;
        }
        #endregion
            #region Commands
            private void ListVotes(object sender, MessageEventArgs e, List<string> parameters)
            {
            List<VoteObject> Votings = xmlprovider.runningVotes();
            if (Votings.Count() > 0)
            {
                e.User.SendMessage("These are the votes currently running");
                foreach (VoteObject vote in Votings)
                {
                    string output = vote.Question + "(" + vote.QuestionID + ")" + " with the answers: ";
                    foreach (Answer answer in vote.Answers)
                    {
                        output += answer.value + " (" + answer.id + "),";
                    }
                    e.User.SendMessage(output);
                }
            }
            else
            {
                e.User.SendMessage("There are no Votes running");
            }
        }

            private void Vote(object sender, MessageEventArgs e, List<string> parameters)
            {
            if (parameters[0] != null)
            {
                if (parameters[0] == "help" || parameters.Count() == 0)
                {
                    e.User.SendMessage(string.Format("The vote command works as follows: !vote [QuestionID] [AnswerID],[AnswerID]..."));
                    e.User.SendMessage(string.Format("If multiple answers aren't allowed in the vote only the first will be used."));
                    return;
                }
            }
            if (parameters.Count() == 2)
            {
                try
                {
                    List<int> answers = new List<int>();
                    if (parameters[1].IndexOf(',') > 0)
                    {
                        foreach (var item in parameters[1].Split(','))
                        {
                            answers.Add(Int32.Parse(item));
                        }
                    }
                    else
                    {
                        answers.Add(Int32.Parse(parameters[1]));
                    }
                    switch (xmlprovider.vote(NormalizeNickName(e.User.Name), Int32.Parse(parameters[0]), answers))
                    {
                        case 0: e.User.SendMessage(string.Format("You have already voted on this Vote")); break;
                        case 1: e.User.SendMessage(string.Format("Your Vote has been counted.")); break;
                        case 3: e.User.SendMessage(string.Format("The Question ID does not match any Questions")); break;
                        default: e.User.SendMessage(string.Format("There was an error in your vote please report to an admin")); break;
                    }
                }
                catch (FormatException)
                {
                    e.User.SendMessage(string.Format("[QuestionID] and [AnswerID] must be whole numbers!"));
                }

            }
        }

            private void EndVote(object sender, MessageEventArgs e, List<string> parameters)
            {
            if (parameters.Count() == 1)
            {
                if (parameters[0] == "help")
                {
                    e.User.SendMessage("To end a vote use !endvotint [Vote_ID]");
                }
                else
                {
                    try
                    {
                        xmlprovider.EndVote(Int32.Parse(parameters[0]));
                        if (xmlprovider.VoteResult(Int32.Parse(parameters[0]), false).Count() == 0)
                        {
                            e.User.SendMessage("The vote id entered does not match any Votes in the Database");
                            return;
                        }
                        foreach (var result in xmlprovider.VoteResult(Int32.Parse(parameters[0]), false))
                        {
                            if (result == "There is no Question matching this ID")
                            {
                                e.User.SendMessage(result.ToString());
                            }
                            e.Channel.SendMessage(result.ToString());
                        }
                    }
                    catch (FormatException)
                    {
                        e.User.SendMessage("[Vote_ID] must be a number!");
                    }
                }
            }
        }

            private void StartVote(object sender, MessageEventArgs e, List<string> parameters)
            {
            if (parameters.Count == 1)
            {
                if (parameters[0] == "help")
                {
                    e.User.SendMessage(string.Format("The works as follows: !startvote t_[time] | d_[date] | q_[question] | a_[answer1,answer2,...] | m_[multiple answers possible y]"));
                    e.User.SendMessage(string.Format("User Time or Date to specify the end of the Voting"));
                    e.User.SendMessage(string.Format("time like [1d][1h][1m] / date[15.05.2015_22:00]"));
                    e.User.SendMessage(string.Format("ignore multiple option if not wanted"));
                    return;
                }
            }
            string time = ""; string date = ""; string question = ""; string answerposibilities = ""; string multiple = "";

            string combinedParameters = combineParameters(parameters);
            string[] splitParamters = combinedParameters.Split('|');
            if (CheckVotingParam(splitParamters[0]) == 1 && CheckVotingParam(splitParamters[1]) == 2)
            {
                e.User.SendMessage(string.Format("You cannot use time and date in the same voting. use !startvote help for information"));
                return;
            }
            foreach (string param in splitParamters)
            {
                switch (CheckVotingParam(param))
                {
                    case 1: time = param; break;
                    case 2: date = param; break;
                    case 3: question = param; break;
                    case 4: answerposibilities = param; break;
                    case 5: multiple = param; break;
                }
            }
            if ((time != "" || date != "") && question != "" && answerposibilities != "")
            {
                string terminationdate = "";
                if (time != null && time != "")
                {
                    time = time.Replace("t_", "");
                    var timeString = time.Trim();
                    var timeRegex = new Regex(@"^(\d+d)?(\d+h)?(\d+m)?(\d+s)?$");
                    var timeMatch = timeRegex.Match(timeString);
                    if (!timeMatch.Success)
                    {
                        e.User.SendMessage("Time needs to be in the following format: [<num>d][<num>h][<num>m]");
                        return;
                    }
                    var span = new TimeSpan();
                    TimeSpan tmpSpan;
                    if (TimeSpan.TryParseExact(timeMatch.Groups[1].Value, "d'd'", null, out tmpSpan))
                        span += tmpSpan;
                    if (TimeSpan.TryParseExact(timeMatch.Groups[2].Value, "h'h'", null, out tmpSpan))
                        span += tmpSpan;
                    if (TimeSpan.TryParseExact(timeMatch.Groups[3].Value, "m'm'", null, out tmpSpan))
                        span += tmpSpan;
                    if (TimeSpan.TryParseExact(timeMatch.Groups[4].Value, "s's'", null, out tmpSpan))
                        span += tmpSpan;
                    var endTime = DateTime.Now + span;
                    terminationdate = endTime.ToString();
                }
                if (date != null && date != "")
                {
                    date = date.Replace("_", " ");
                    date = date.Replace("d_", " ");
                    terminationdate = date;
                }
                question = question.Replace("q_", "");
                answerposibilities = answerposibilities.Replace("a_", "");
                string[] answers = answerposibilities.Split(',');
                bool multi = false;
                if (multiple != "")
                {
                    multiple = multiple.Replace("m_", " ");
                    if (multiple.Trim() == "y") { multi = true; }
                    else { multi = false; };
                }
                int questionid = xmlprovider.startVote(DateTime.Parse(terminationdate), multi, answers, question);
                if (questionid != 0)
                {
                    e.Channel.SendMessage("The following vote has been started: " + question);
                    e.Channel.SendMessage( "Possible answers are:");
                    int i = 1;
                    foreach (var singleanswer in answers)
                    {
                        e.Channel.SendMessage("use '!vote" + questionid + " " + i + "' for " + singleanswer.ToString());
                        i++;
                    }
                    e.Channel.SendMessage(e.User.Name + " has started this Vote");
                    if (VoteTimer.Enabled == false)
                    {
                        VoteTimer.Start();
                        VoteTimer.Enabled = true;
                        CurrentChannel = e;
                    }
                }
            }
            else
            {
                e.User.SendMessage(string.Format("Please use the help command '!startvote help' for information"));
            }
        }
            #endregion
            #region Events
            MessageEventArgs CurrentChannel;
            private void OnVoteTimerEvent(Object source, System.Timers.ElapsedEventArgs e)
            {
                List<int> Votes = xmlprovider.expiredVotes();
                foreach (int vote in Votes)
                {
                    xmlprovider.EndVote(vote);
                    List<string> Question = xmlprovider.VoteResult(vote, true);
                    foreach (string output in Question)
                    {
                        CurrentChannel.Channel.SendMessage(output);
                    }
                }
                if (xmlprovider.runningVotes().Count() == 0)
                {
                    VoteTimer.Enabled = false;
                    VoteTimer.Stop();
                }
            }
            #endregion
        #endregion
        #endregion
        #endregion
        #region generalfunctions
        public string NormalizeNickName(string nick)
        {

            while (nick.EndsWith("_"))
            {
                nick = nick.Substring(0, nick.Length - 1);
            }
            if (nick.EndsWith("afk"))
            {
                nick.Replace("afk", "");
            }
            if (nick.EndsWith("handy"))
            {
                nick.Replace("handy", "");
            }
            if (nick.Contains("_"))
            {
                nick = nick.Split('_')[0];
            }
            if (nick.Contains("|"))
            {
                nick = nick.Split('|')[0];
            }
            string lastchar = "f";
            string secondlastchar = "f";
            try
            {
                lastchar = nick.Substring(nick.Length - 1, 1);
                secondlastchar = nick.Substring(nick.Length - 2, 1);
            }
            catch (Exception)
            {

            }

            int n;
            int m;
            if (int.TryParse(lastchar, out n) && !int.TryParse(secondlastchar, out m))
            {
                nick = nick.Substring(0, nick.Length - 1);
            }
            return nick.ToLower();
        }
        public DataFiles.User getUser(string name)
        {
            IEnumerable<DataFiles.User> users = LUserList.Where(x => x.isUser(name));
            if (users.Count() > 0)
            {
                return users.First();
            }
            return null;
        }
        private string combineParameters(IList<string> parameters)
        {
            string combined = "";
            int stringcount = 0;
            foreach (string insert in parameters)
            {
                if (stringcount < parameters.Count())
                {
                    combined += insert + " ";
                }
                else
                {
                    combined += insert;
                }

                stringcount++;
            }
            return combined;
        }
        public bool UserListLocked = false;
        public void SaveUserList()
        {

            while (UserListLocked)
            {
                Thread.Sleep(1000);
            }
            UserListLocked = true;
            var path = Directory.GetCurrentDirectory() + "/XML/Usersv2.xml";
            var backuppath = Directory.GetCurrentDirectory() + "/XML/Usersv2bck.xml";
            File.Copy(path, backuppath, true);

            var OrderedUserList = LUserList.OrderByDescending(x => x.LastVisit).ToList();
            LUserList = OrderedUserList;
            System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(LUserList.GetType());
            System.IO.FileStream file = System.IO.File.Create(path);
            xmlserializer.Serialize(file, LUserList);
            file.Close();
            UserListLocked = false;
        }
        #endregion
        #region StreamEvents
        
        private void OnStreamStopped(object sender, StreamEventArgs args)
        {
            if (Properties.Settings.Default.SilentMode.ToString() != "true")
            {
                if (xmlprovider == null) { xmlprovider = new XMLProvider(); }
                if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "starttime") != "" && Convert.ToBoolean(xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "running")))
                {
                    if (RelayBots.Where(x => x.sChannel.ToLower() == args.StreamData.Stream.Channel.ToLower()).Count() > 0)
                    {
                        RelayBots.Where(x => x.sChannel.ToLower() == args.StreamData.Stream.Channel.ToLower()).First().StartRelayEnd();
                    }
                    xmlprovider.StreamStartUpdate(args.StreamData.Stream.Channel, true);
                    string duration = DateTime.Now.Subtract(Convert.ToDateTime(xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "starttime"))).ToString("h':'mm':'ss");
                    string output = "Stream stopped after " + duration + ": " + args.StreamData.Stream.Channel;
                    List<string> NoticeTargets = new List<string>();
                    List<string> MsgsTargets = new List<string>();
                    List<NormalisedUser> UsersinChannel = new List<NormalisedUser>();
                    foreach (var user in bot.Servers.First().Users)
                    {
                        if (user.Name != Settings.Default.Name.ToString())
                        {
                                UsersinChannel.Add(new NormalisedUser(user.Name));
                        }
                    }
                    foreach (NormalisedUser user in UsersinChannel)
                    {
                        DataFiles.User userUser = getUser(user.normalised_username().ToLower());
                        if (userUser != null)
                        {
                            if (userUser.isSubscribed(args.StreamData.Stream.Channel))
                            {
                                MsgsTargets.Add(user.orig_username);
                            }
                        }
                        else
                        {
                            if (user.orig_username != "Q")
                            {
                                MsgsTargets.Add(user.orig_username);
                            }
                        }

                    }
                    if (MsgsTargets.Count > 0)
                    {
                        foreach(var username in NoticeTargets)
                        {
                            var client = bot.Servers.First().Users.Where(x => x.Name.ToLower() == username.ToLower()).First();
                            if(client != null)
                            {
                                client.SendMessage(output);
                            }
                        }
                    }
                }
            }
        }
        private void OnStreamGlobalNotification(object sender, StreamEventArgs args)
        {
            if (Properties.Settings.Default.SilentMode.ToString() != "true")
            {
                if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "running") == "true")
                {
                    if (args.StreamData.StreamProvider.GetType().ToString() == "DeathmicChatbot.StreamInfo.Hitbox.HitboxProvider")
                    {
                       
                    }
                    else
                    {
                        //Twitch
                        if(RelayBots.Where(x=>x.sChannel.ToLower() == args.StreamData.Stream.Channel).Count()==0 || RelayBots.Where(x => x.sChannel.ToLower() == args.StreamData.Stream.Channel).Count() >0 && RelayBots.Where(x => x.sChannel.ToLower() == args.StreamData.Stream.Channel).First().bDisconnected)
                        {
                            ConnectToTwitchChat(args.StreamData.Stream.Channel, true);
                        }
                        if(RelayBots.Where(x => x.sChannel.ToLower() == args.StreamData.Stream.Channel).Count() > 0)
                        {
                            foreach(TwitchRelay RelayBot in RelayBots)
                            {
                                RelayBot.StopRelayEnd();
                            }
                        }
                    }
                    
                    if (xmlprovider.GlobalAnnouncementDue(args.StreamData.Stream.Channel))
                    {
                        string game = "";
                        if (args.StreamData.StreamProvider.GetType().ToString() == "DeathmicChatbot.StreamInfo.Hitbox.HitboxProvider")
                        {
                            game = args.StreamData.Stream.Message;
                        }
                        else
                        {
                            game = xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "game");
                        }
                        xmlprovider.AddStreamLivedata(args.StreamData.Stream.Channel, args.StreamData.StreamProvider.GetLink() + "/" + args.StreamData.Stream.Channel, game);
                        string output = "Stream is running: " + args.StreamData.Stream.Channel + "(" + args.StreamData.Stream.Game + ": " + args.StreamData.Stream.Message + ")" + " at " + args.StreamData.StreamProvider.GetLink() + "/" + args.StreamData.Stream.Channel;
                        List<string> NoticeTargets = new List<string>();
                        List<string> MsgsTargets = new List<string>();
                        List<NormalisedUser> UsersinChannel = new List<NormalisedUser>();

                        foreach (var user in bot.Servers.First().Users)
                        {
                            if (user.Name != Settings.Default.Name.ToString())
                            {
                                UsersinChannel.Add(new NormalisedUser(user.Name));
                            }
                        }
                        foreach (NormalisedUser user in UsersinChannel)
                        {
                            DataFiles.User userUser = getUser(user.normalised_username().ToLower());
                            if (userUser != null)
                            {

                                if (userUser.isSubscribed(args.StreamData.Stream.Channel) && userUser.isGlobalAnnouncment(args.StreamData.Stream.Channel))
                                {
                                    MsgsTargets.Add(user.orig_username);
                                }
                            }
                            else
                            {
                                MsgsTargets.Add(user.orig_username);
                            }

                        }
                        if (MsgsTargets.Count > 0)
                        {
                            foreach (var username in MsgsTargets)
                            {
                                var client = bot.Servers.First().Users.Where(x => x.Name.ToLower() == username.ToLower()).First();
                                if (client != null)
                                {
                                    client.SendMessage(output);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnStreamStarted(object sender, StreamEventArgs args)
        {
            if (Properties.Settings.Default.SilentMode.ToString() != "true")
            {
                if (xmlprovider == null) { xmlprovider = new XMLProvider(); }
                if (xmlprovider.isinStreamList(args.StreamData.Stream.Channel))
                {
                    if (xmlprovider.StreamInfo(args.StreamData.Stream.Channel, "running") == "false")
                    {
                        if (args.StreamData.StreamProvider.GetType().ToString() == "DeathmicChatbot.StreamInfo.Hitbox.HitboxProvider")
                        {

                        }
                        else
                        {
                            if (RelayBots.Where(x => x.sChannel.ToLower() == args.StreamData.Stream.Channel.ToLower()).Count() > 0 && RelayBots.Where(x => x.sChannel.ToLower() == args.StreamData.Stream.Channel.ToLower()).First().isExit == false)
                            {
                                RelayBots.Where(x => x.sChannel.ToLower() == args.StreamData.Stream.Channel.ToLower()).First().StopRelayEnd();
                            }
                            else
                            {
                                ConnectToTwitchChat(args.StreamData.Stream.Channel, true);
                            }
                        }
                        
                        string game = args.StreamData.Stream.Game;

                        if (args.StreamData.Stream.Message != null)
                        {
                            game = args.StreamData.Stream.Message;
                        }
                        xmlprovider.AddStreamLivedata(args.StreamData.Stream.Channel, args.StreamData.StreamProvider.GetLink() + "/" + args.StreamData.Stream.Channel, game);
                        string output = "Stream started: " + args.StreamData.Stream.Channel + "(" + args.StreamData.Stream.Game + ": " + args.StreamData.Stream.Message + ")" + " at " + args.StreamData.StreamProvider.GetLink() + "/" + args.StreamData.Stream.Channel;
                        List<string> NoticeTargets = new List<string>();
                        List<string> MsgsTargets = new List<string>();
                        List<NormalisedUser> UsersinChannel = new List<NormalisedUser>();
                        foreach (var user in bot.Servers.First().Users)
                        {
                            if (user.Name != Settings.Default.Name.ToString())
                            {
                                UsersinChannel.Add(new NormalisedUser(user.Name));
                            }
                        }
                        foreach (NormalisedUser user in UsersinChannel)
                        {
                            DataFiles.User userUser = getUser(user.normalised_username().ToLower());
                            if (userUser != null)
                            {
                                if (userUser.isSubscribed(args.StreamData.Stream.Channel))
                                {
                                    MsgsTargets.Add(user.orig_username);
                                }
                            }
                            else
                            {
                                if (user.orig_username != "Q")
                                {
                                    MsgsTargets.Add(user.orig_username);
                                }
                            }

                        }
                        if (MsgsTargets.Count > 0)
                        {
                            foreach (var username in MsgsTargets)
                            {
                                var client = bot.Servers.First().Users.Where(x => x.Name.ToLower() == username.ToLower()).First();
                                if (client != null)
                                {
                                    try
                                    {
                                        if(client.Status.Value != "offline" || client.Status.Value != "Offline")
                                        {
                                            Console.WriteLine(client.Name);
                                            client.SendMessage(output);
                                        }
                                    }catch(Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                    
                                }
                            }
                        }
                        xmlprovider.GlobalAnnouncementDue(args.StreamData.Stream.Channel);
                    }
                    xmlprovider.StreamStartUpdate(args.StreamData.Stream.Channel);

                }
            }

        }

        public void Dispose()
        {
        }

        #endregion

    }

}

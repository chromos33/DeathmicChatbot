using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using DeathmicChatbot.StreamInfo.Twitch;
using System.Globalization;
using System.Threading;
using DeathmicChatbot.DataFiles;
using IrcDotNet;
using DeathmicChatbot.TransferClasses;
using IrcDotNet.Ctcp;

namespace DeathmicChatbot
{
    class XMLProvider
    {
        protected XDocument Users;
        protected XDocument Streams;
        protected XDocument UserPicks;
        protected XDocument Votes;
        protected XDocument Counters;
        public XMLProvider()
        {
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                Streams = XDocument.Load(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
            }
            else
            {
                Streams = new XDocument();
                Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml"))
            {
                UserPicks = XDocument.Load(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml");
            }
            else
            {
                UserPicks = new XDocument();
                UserPicks.Save(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Votes.xml"))
            {
                Votes = XDocument.Load(Directory.GetCurrentDirectory()+"/XML/Votes.xml");
            }
            else
            {
                Votes = new XDocument();
                Votes.Save(Directory.GetCurrentDirectory()+"/XML/Votes.xml");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Counters.xml"))
            {
                Counters = XDocument.Load(Directory.GetCurrentDirectory()+"/XML/Counters.xml");
            }
            else
            {
                Counters = new XDocument(new XElement("Counters",""));
                Counters.Save(Directory.GetCurrentDirectory()+"/XML/Counters.xml");
            }
        }
        #region User Stuff
        public string UserInfo(string nick)
        {
            //TODO change from string to Tuple<int,DateTime>
            //https://msdn.microsoft.com/en-us/library/system.tuple%28v=vs.110%29.aspx
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            nick = nick.ToLower();
            string answer = "";
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                IEnumerable<XElement> childlist = from users in Users.Root.Elements()
                                                  where (users.Element("Alias").Attribute("Value").Value == nick
                                                  || users.Attribute("Nick").Value == nick) && users.Attribute("Nick").Value != "BotDeathmic"
                                                  select users;
                if (childlist.Count() > 0)
                {
                    foreach (var user in childlist)
                    {
                        int count = Int32.Parse(user.Attribute("VisitCount").Value);
                        count++;
                        answer += count.ToString() + ",";
                        answer += user.Attribute("LastVisit").Value;
                    }
                }
                else
                {
                    return "1," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            return answer;
        }
        public string ToggleUserLogging(string nick)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            string answer = "";
            nick = nick.ToLower();
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                IEnumerable<XElement> childlist = from users in Users.Root.Elements() where users.Attribute("Nick").Value == nick select users;
                foreach (var item in childlist)
                {
                    if (item.Attribute("isloggingOp").Value == "true")
                    {
                        item.Attribute("isloggingOp").Value = "false";
                        answer = "Logging Messages disabled";
                    }
                    else
                    {
                        item.Attribute("isloggingOp").Value = "true";
                        answer = "Logging Messages enabled";
                    }
                }
                Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");

            }
            return answer;
        }
        // returns All Users Ever Joined/Added and returns them in CSV data as string
        public string AllUserEverJoinedList()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            string answer = "";


            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                IEnumerable<XElement> childlist = from users in Users.Root.Elements() select users;
                foreach (var user in childlist)
                {
                    answer += user.Attribute("Nick").Value + ",";
                }
                answer = answer.Substring(answer.Length - 1, answer.Length);

            }
            else
            {
            }


            return answer;
        }

        public List<String> LoggingUser()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            List<String> answer = new List<string>();


            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                IEnumerable<XElement> childlist = from users in Users.Root.Elements() where users.Attribute("isloggingOp").Value == "true" select users;
                foreach (var user in childlist)
                {
                    answer.Add(user.Attribute("Nick").Value);
                }
            }
            return answer;
        }
        public void AddAllStreamsToUser(String password = "")
        {
            string path = Environment.CurrentDirectory + "/password.txt";
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string line = "";
            try
            {
                line = file.ReadLine();
            }
            catch (Exception) { }
            if(password == line)
            {
                string[] streams = StreamList().Split(',');
                if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
                {
                    if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/Backup"))
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/Backup");
                    }
                    string filename = Directory.GetCurrentDirectory()+"/XML/Backup/Usersbackup" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".xml";
                    Users.Save(filename);
                }
                string suscribestreams = "";
                foreach (string stream in streams)
                {
                    suscribestreams += stream;
                }
                foreach (string user in AllUser())
                { 
                    AddorUpdateSuscription(user, suscribestreams.ToLower(), "", false, true);
                    Thread.Sleep(50);
                }
            }
        }
        public List<String> AllUser()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            List<String> answer = new List<string>();


            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                IEnumerable<XElement> childlist = from users in Users.Root.Elements() select users;
                foreach (var user in childlist)
                {
                    answer.Add(user.Attribute("Nick").Value);
                }
            }
            return answer;
        }
        //Adds User or Upates information like Visit Count and Last Visit
        public string AddorUpdateUser(string nick, bool leave = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            nick = nick.ToLower();
            string answer = "";
            //Query XML File for User Update
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                try
                {
                    IEnumerable<XElement> childlist = Users.Root.Elements().Where(user => user.Attribute("Nick").Value == nick);

                    if (childlist.Count() > 0)
                    {

                        foreach (XElement element in childlist)
                        {

                            element.Attribute("LastVisit").Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            if (!leave)
                            {
                                int _visitcount = Int32.Parse(element.Attribute("VisitCount").Value);
                                _visitcount++;
                                element.Attribute("VisitCount").Value = _visitcount.ToString();
                                if (element.Attribute("oldcounter") != null)
                                {
                                    int _oldvisitcount = Int32.Parse(element.Attribute("oldcounter").Value);
                                    _oldvisitcount++;
                                    element.Attribute("oldcounter").Value = _oldvisitcount.ToString();
                                }
                                answer = "User updated";
                            }
                        }
                    }
                    else
                    {
                        var _element = new XElement("User",
                            new XAttribute("Nick", nick),
                            new XAttribute("LastVisit", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                            new XAttribute("VisitCount", "1"),
                            new XAttribute("isloggingOp", "false"),
                            new XAttribute("Streams", StreamList()),
                            new XElement("Alias", new XAttribute("Value", ""))
                            );
                        Users.Element("Users").Add(_element);
                        answer = "User added";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    answer = "Add Failure";
                }
            }
            Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");
            return answer;
        }


        public bool AddorUpdatePassword(string nick, string pass, string oldpass="")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            nick = nick.ToLower();
            //Query XML File for User Update
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                try
                {
                    IEnumerable<XElement> childlist = Users.Root.Elements().Where(user => user.Attribute("Nick").Value == nick);

                    if (childlist.Count() > 0)
                    {

                        foreach (XElement element in childlist)
                        {
                            if (element.Attribute("password") != null)
                            {
                                if(element.Attribute("password").Value == oldpass)
                                {
                                    element.Attribute("password").Value = pass;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                element.Add(new XAttribute("password", pass));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");
            return true;
        }
        public bool ToggleStreamMsgs(string nick)
        {
            bool toggle = false;
            nick = nick.ToLower();
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                IEnumerable<XElement> childlist = Users.Root.Elements().Where(user => user.Attribute("Nick").Value == nick);
                if (childlist.Count() > 0)
                {
                    foreach (XElement element in childlist)
                    {
                        if(element.Attribute("streammsgs") != null)
                        {
                            if(element.Attribute("streammsgs").Value == "true")
                            {
                                element.Attribute("streammsgs").Value = "false";
                                toggle = false;
                            }
                            else
                            {
                                element.Attribute("streammsgs").Value = "true";
                                toggle = true;
                            }
                        }
                        else
                        {
                            element.Add(new XAttribute("streammsgs","true"));
                            toggle = true;
                        }
                    }
                    Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");
                }
            }
            return toggle;
        }
        public bool CheckStreamMsgsState(string nick)
        {
            bool toggle = false;
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                IEnumerable<XElement> childlist = Users.Root.Elements().Where(user => user.Attribute("Nick").Value == nick);
                if (childlist.Count() > 0)
                {
                    foreach (XElement element in childlist)
                    {
                        if (element.Attribute("streammsgs") != null)
                        {
                            if (element.Attribute("streammsgs").Value == "true")
                            {
                                toggle = true;
                            }
                            else
                            {
                                toggle = false;
                            }
                        }
                        else
                        {
                            toggle = false;
                        }
                    }
                    Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");
                }
            }
            return toggle;
        }
        public bool CheckPassword(string nick, string pass)
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                try
                {
                    string test = Users.Root.Elements().Where(user => user.Attribute("Nick").Value == nick).Select(user => user.Attribute("password").Value).First();
                    if (test == pass)
                    {
                        return true;
                    }
                }
                catch(Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public bool AddorUpdateSuscription(string nick, string streamname,string pass,bool remove, bool ignorepass = false)
        {
            //ignorepass for internal Tasks
            nick = nick.ToLower();
            if(CheckPassword(nick,pass) || ignorepass)
            {
                //Query XML File for User Update
                if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
                }
                if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
                {
                    IEnumerable<XElement> childlist = from el in Users.Root.Elements() where el.Attribute("Nick").Value == nick select el;
                    if (childlist.Count() > 0)
                    {
                        foreach (XElement item in childlist)
                        {
                           if(item.Attribute("Streams") == null)
                           {
                                item.Add(new XAttribute("Streams", streamname));
                           }
                           else
                           {
                                if(item.Attribute("Streams").Value.Contains(streamname))
                                {
                                    if(remove)
                                    {
                                        item.Attribute("Streams").Value = item.Attribute("Streams").Value.Replace(streamname, "");
                                    }
                                }
                                else
                                {
                                    item.Attribute("Streams").Value += streamname;
                                }
                           }
                        }
                        Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");
                        return true;
                    }
                }
            }
            return false; 
        }
        public bool CheckSuscription(string nick, string streamname)
        {

            nick = nick.ToLower();
                //Query XML File for User Update
                if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
                }
                if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
                {
                    int i = Users.Root.Elements().Where(User => User.Attribute("Nick").Value.ToLower() == nick.ToLower()).Where(User => User.Attribute("Streams") != null).Where(User => User.Attribute("Streams").Value.ToLower().Contains(streamname.ToLower())).Count();
                    if (i > 0)
                    {
                        return true;
                    }
                }
                return false;
        }
        public List<string> SuscribedUsers(string streamname,IEnumerable<string> users)
        {
            List<string> result = new List<string>();
            streamname = streamname.ToLower();
            string userlist = "";
            foreach(string user in users)
            {
                userlist += user.ToLower();
            }
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                try
                {
                    var _Users = Users.Root.Elements().Where(User => userlist.Contains(User.Attribute("Nick").Value)).Where(User => User.Attribute("Streams").Value.Contains(streamname));
                    foreach (var item in _Users)
                    {
                        result.Add(item.Attribute("Nick").Value);
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return result;
        }
        public List<string> SuscribedUsers(string streamname, IEnumerable<NormalisedUser> users)
        {
            List<string> result = new List<string>();
            streamname = streamname.ToLower();
            string userlist = "";
            foreach (NormalisedUser user in users)
            {
                userlist += user.normalised_username().ToLower();
            }
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                try
                {
                    var _Users = Users.Root.Elements().Where(User => userlist.Contains(User.Attribute("Nick").Value)).Where(User => User.Attribute("Streams").Value.Contains(streamname));
                    foreach (var item in _Users)
                    {
                        result.Add(item.Attribute("Nick").Value);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return result;
        }
        public bool isSuscribed(string streamname,string username)
        {
            bool suscribed = false;
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                try
                {
                    var _Users = Users.Root.Elements().Where(User => User.Attribute("Nick").Value == username).Where(User => User.Attribute("Streams").Value.Contains(streamname.ToLower()));
                    if(_Users.Count() > 0)
                    {
                        suscribed = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return suscribed;
        }
        #endregion
        public void DateTimeCorrection()
        {
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                IEnumerable<XElement> childlist = from el in Users.Root.Elements() select el;
                if (childlist.Count() > 0)
                {
                    foreach (XElement item in childlist)
                    {
                        try
                        {
                            DateTime OldDateTime = DateTime.ParseExact(item.Attribute("LastVisit").Value, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                            item.Attribute("LastVisit").Value = OldDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        catch(Exception)
                        {
                        }
                        
                    }
                    Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");
                }
            }
        }
        //returns data of User as CSV data in following order VisitCount, LastVisit
        //Adds Alias to User (?where to use no idea implemented because SQlite structure suggested Usage)
        public string AddAlias(string nick, string alias)
        {
            nick = nick.ToLower();
            alias = alias.ToLower();
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                IEnumerable<XElement> childlist = from el in Users.Root.Elements() where el.Attribute("Nick").Value == nick select el;
                if (childlist.Count() > 0)
                {
                    foreach (XElement item in childlist)
                    {
                        bool aliascontained = false;
                        foreach (XElement test in item.Elements("Alias"))
                        {
                            if (test.Attribute("Value").Value == alias)
                            {
                                aliascontained = true;
                            }
                        }
                        if (aliascontained == false)
                        {
                            item.Add(new XElement("Alias", new XAttribute("Value", alias)));
                            Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");
                            IEnumerable<XElement> childlist2 = from el in Users.Root.Elements() where el.Attribute("Nick").Value == alias select el;
                            if (childlist2.Count() > 0)
                            {
                                foreach (XElement item2 in childlist2)
                                {
                                    if (item2.Attribute("hasbeenmerged") == null)
                                    {
                                        int count = Int32.Parse(item.Attribute("VisitCount").Value);
                                        item.Add(new XAttribute("oldcounter", item.Attribute("VisitCount").Value));
                                        item.Attribute("VisitCount").Value = (Int32.Parse(item2.Attribute("VisitCount").Value) + Int32.Parse(item.Attribute("VisitCount").Value)).ToString();
                                        item2.Add(new XAttribute("oldcounter", item2.Attribute("VisitCount").Value));
                                        item2.Attribute("VisitCount").Value = (Int32.Parse(item2.Attribute("VisitCount").Value) + count).ToString();
                                        item2.Add(new XAttribute("hasbeenmerged", 1));
                                        Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");
                                    }

                                }

                            }
                            return "Alias was added to the User";
                        }
                        else
                        {
                            return "Alias already added";
                        }


                    }
                }
                else
                {
                    return "No User by this nick found";
                }
            }
            else
            {
                //Will probably never happen but just to make sure
                return "Users.xml does not exist, so no alias can be added, please address this bots administrator";
            }
            return "";
        }

        public void RemoveUnnecessaryNicks()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            int i = 0;
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                try
                {
                    IEnumerable<XElement> childlist = Users.Root.Elements();
                    bool change = false;
                    if(childlist.Count()>0)
                    {
                        Console.WriteLine(childlist.Count());
                        while(i <= childlist.Count())
                        {
                            i++;
                            foreach (XElement user in childlist)
                            {
                                string lastchar = "f";
                                string secondlastchar = "f";
                                try
                                {
                                    lastchar = user.Attribute("Nick").Value.Substring(user.Attribute("Nick").Value.Length - 1, 1);
                                    secondlastchar = user.Attribute("Nick").Value.Substring(user.Attribute("Nick").Value.Length - 2, 1);
                                }catch(Exception)
                                {

                                }
                                
                                int n;
                                int m;
                                if (int.TryParse(lastchar,out n) && !int.TryParse(secondlastchar,out m))
                                {
                                    user.Remove();
                                    change = true;
                                }
                                else
                                if(user.Attribute("Nick").Value.EndsWith("afk")|| user.Attribute("Nick").Value.EndsWith("handy"))
                                {
                                    user.Remove();
                                    change = true;
                                }
                                else
                                if (user.Attribute("Nick").Value.Contains("andchat"))
                                {
                                    user.Remove();
                                    change = true;
                                }
                                else
                                {
                                    if (user.Attribute("Nick").Value.EndsWith("_"))
                                    {
                                        user.Remove();
                                        change = true;
                                    }
                                    else
                                    {
                                        if (user.Attribute("Nick").Value.Contains("|"))
                                        {
                                            IEnumerable<XElement> sublist = Users.Root.Elements().Where(_user => _user.Attribute("Nick").Value == user.Attribute("Nick").Value.Split('|')[0]);
                                            if (sublist.Count() > 0)
                                            {
                                                foreach (XElement subuser in sublist)
                                                {
                                                    subuser.Attribute("VisitCount").Value = ((Int32.Parse(subuser.Attribute("VisitCount").Value.ToString())) + (Int32.Parse(user.Attribute("VisitCount").Value.ToString()))).ToString();
                                                    user.Remove();
                                                    change = true;
                                                }
                                            }
                                            else
                                            {
                                                user.Remove();
                                                change = true;
                                            }
                                        }
                                        if (user.Attribute("Nick").Value.Contains("_"))
                                        {
                                            IEnumerable<XElement> sublist = Users.Root.Elements().Where(_user => _user.Attribute("Nick").Value == user.Attribute("Nick").Value.Split('_')[0]);
                                            if (sublist.Count() > 0)
                                            {
                                                foreach (XElement subuser in sublist)
                                                {
                                                    subuser.Attribute("VisitCount").Value = ((Int32.Parse(subuser.Attribute("VisitCount").Value.ToString())) + (Int32.Parse(user.Attribute("VisitCount").Value.ToString()))).ToString();
                                                    user.Remove();
                                                    change = true;
                                                }
                                            }
                                            else
                                            {
                                                user.Remove();
                                                change = true;
                                            }
                                        }
                                    }
                                }
                            }
                            childlist = Users.Root.Elements();
                        }
                    }
                    if(change)
                    {
                        Console.Write(i);
                        Users.Save(Directory.GetCurrentDirectory()+"/XML/Users.xml");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
       

        #region stream stuff
        public int TwitchChatToStream(string channel,string targetchannel, int twoway)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            int answer = 0;
            //Query XML File for User Update
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory() + "/XML/Streams.xml"))
            {
                IEnumerable<XElement> childlist = from el in Streams.Root.Elements() where el.Attribute("Channel").Value == channel select el;
                if (childlist.Count() > 0)
                {
                    try
                    {
                        if(childlist.First().Attribute("twitchchat") != null)
                        {
                            childlist.First().Attribute("twitchchat").Value = channel;
                        }
                        else
                        {
                            childlist.First().Add(new XAttribute("twitchchat", channel));
                            
                        }

                        if (childlist.First().Attribute("twoway") != null)
                        {
                            childlist.First().Attribute("twoway").Value = twoway.ToString();
                        }
                        else
                        {
                            childlist.First().Add(new XAttribute("twoway", twoway.ToString()));
                        }

                        if (childlist.First().Attribute("targetchannel") != null)
                        {
                            childlist.First().Attribute("targetchannel").Value = targetchannel;
                        }
                        else
                        {
                            childlist.First().Add(new XAttribute("targetchannel", targetchannel));
                        }
                        Streams.Save(Directory.GetCurrentDirectory() + "/XML/Streams.xml");
                        return 1;
                    }
                    catch(Exception)
                    {

                    }
                }
            }
            return 0;
        }
        public Tuple<string,int> GetTwitchChatData(string channel)
        {
            Tuple<string, int> result = new Tuple<string,int>("",3);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            //Query XML File for User Update
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory() + "/XML/Streams.xml"))
            {
                IEnumerable<XElement> childlist = from el in Streams.Root.Elements() where el.Attribute("Channel").Value == channel select el;
                if (childlist.Count() > 0)
                {
                    try
                    {
                        int twoway;
                        string targetchannel;
                        if (childlist.First().Attribute("twoway") != null)
                        {
                            twoway = Int32.Parse(childlist.First().Attribute("twoway").Value);
                        }
                        else
                        {
                            twoway = 3;
                        }

                        if (childlist.First().Attribute("targetchannel") != null)
                        {
                            targetchannel = childlist.First().Attribute("targetchannel").Value;
                        }
                        else
                        {
                            targetchannel = "";
                        }
                        result = new Tuple<string, int>(targetchannel, twoway);

                    }
                    catch (Exception)
                    {

                    }
                }
            }

            return result;
        }
        public int AddStream(string channel)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            int answer = 0;
            //Query XML File for User Update
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                IEnumerable<XElement> childlist = from el in Streams.Root.Elements() where el.Attribute("Channel").Value == channel select el;
                if (childlist.Count() > 0)
                {
                    answer = 2;
                }
                else
                {
                    try
                    {
                        var _element = new XElement("Stream",
                           new XAttribute("Channel", channel.ToLower()),
                            new XAttribute("starttime", ""),
                            new XAttribute("stoptime", ""),
                            new XAttribute("running", "false"),
                            new XAttribute("provider", "")
                           );
                        Streams.Element("Streams").Add(_element);
                        answer = 1;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            //Backup just in case something goes wrong
            /*
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Users.xml"))
            {
                if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/Backup"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/Backup");
                }
                string filename = Directory.GetCurrentDirectory()+"/XML/Backup/Usersbackup" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".xml";
                Users.Save(filename);
            }

            if (answer == 1)
            {
                foreach(string user in AllUser())
                {
                    AddorUpdateSuscription(user, channel.ToLower(), "", false, true);
                    Thread.Sleep(1000);
                }
            }
            Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
            */
            return answer;
        }

        public string RemoveStream(string channel)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            string answer = "";
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                try
                {
                    Streams.Element("Streams").Elements("Stream").Where(x => x.Attribute("Channel").Value == channel).Remove();
                    answer = "Stream removed";

                    Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    answer = "Stream remove Failure - Talk to the programmer!";
                }
            }
            else
            {
                answer = "This stream is not in the list.";
            }
            return answer;
        }
        public String stripNonValidXMLCharacters(string textIn)
        {
            System.Text.StringBuilder textOut = new System.Text.StringBuilder(); // Used to hold the output.
            char current; // Used to reference the current character.


            if (textIn == null || textIn == string.Empty) return string.Empty; // vacancy test.
            for (int i = 0; i < textIn.Length; i++)
            {
                current = textIn[i];


                if ((current == 0x9 || current == 0xA || current == 0xD) ||
                    ((current >= 0x20) && (current <= 0xD7FF)) ||
                    ((current >= 0xE000) && (current <= 0xFFFD)) ||
                    ((current >= 0x10000) && (current <= 0x10FFFF)))
                {
                    textOut.Append(current);
                }
            }
            return textOut.ToString();
        }
        // Twitch Double Game Title Fix
        public bool AddStreamdata(string provider ,TwitchStreamData stream)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            bool answer = false;
            try
            {
                if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
                {
                    if (provider == "twitch")
                    {
                        IEnumerable<XElement> childlist = from el in Streams.Root.Elements() where el.Attribute("Channel").Value == stream.Stream.Channel.Name.ToString().ToLower() select el;
                        foreach (var element in childlist)
                        {
                            if (element.Attribute("game") == null)
                            {
                                element.Add(new XAttribute("game", ""));
                            }
                            string game = "";
                            string currentgame = element.Attribute("game").Value;
                            if (currentgame != stream.Stream.Channel.Game.ToString() && stream.Stream.Channel.Game.ToString() != "")
                            {
                                game = stream.Stream.Channel.Game.ToString();
                            }
                            if (currentgame != stream.Stream.Game.ToString() && stream.Stream.Game.ToString() != "")
                            {
                                game = stream.Stream.Game.ToString();
                            }
                            game = stripNonValidXMLCharacters(game);
                            game = game.Replace(",", "");
                            game = game.Replace(";", "");

                            if (element.Attribute("game") == null)
                            {
                                element.Add(new XAttribute("game", game));
                            }
                            else
                            {
                                element.Attribute("game").Value = game;
                            }


                            if (element.Attribute("URL") == null)
                            {
                                element.Add(new XAttribute("URL", stream.Stream.Channel.Url));
                            }
                            else
                            {
                                element.Attribute("URL").Value = stream.Stream.Channel.Url;
                            }
                        }
                    }
                    answer = true;
                    Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return answer;
        }

        public void AddStreamLivedata(string Channel, string URL,string Game)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                try
                {
                    IEnumerable<XElement> childlist = from el in Streams.Root.Elements() where el.Attribute("Channel").Value == Channel.ToLower() select el;
                    foreach (var element in childlist)
                    {
                        if (element.Attribute("game") == null)
                        {
                            element.Add(new XAttribute("game", Game));
                            Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                        }
                        else
                        {
                            element.Attribute("game").Value = Game;
                        }
                        if (element.Attribute("URL") == null)
                        {
                            element.Add(new XAttribute("URL", URL));
                            Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                        }
                        else
                        {
                            element.Attribute("URL").Value = URL;
                        }
                    }
                    Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }
        //returns Streamlist as CSV data
        public string StreamList(string provider = "")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            //Maybe add provider filtering but have to somewhere add the provider
            string answer = "";

            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                System.Diagnostics.Debug.WriteLine(provider);
                IEnumerable<XElement> childlist = from streams in Streams.Root.Elements() select streams;

                if (childlist.Count() > 0)
                {
                    foreach (var stream in childlist)
                    {
                        answer += stream.Attribute("Channel").Value + ",";
                    }
                    answer = answer.Substring(0, answer.Length - 1);
                }
            }
            return answer;
        }
        public void ResetStreamState()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            //Maybe add provider filtering but have to somewhere add the provider
            string answer = "";

            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                IEnumerable<XElement> childlist = from streams in Streams.Root.Elements() select streams;

                if (childlist.Count() > 0)
                {
                    foreach (var stream in childlist)
                    {
                        stream.Attribute("running").Value = "false";
                    }
                    Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                }
            }
            return;
        }
        public string[] OnlineStreamList()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            //Maybe add provider filtering but have to somewhere add the provider
            List<string> answer = new List<string>();

            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                IEnumerable<XElement> childlist = from streams in Streams.Root.Elements() where streams.Attribute("running").Value == "true" select streams;

                if (childlist.Count() > 0)
                {
                    foreach (var stream in childlist)
                    {
                        answer.Add(stream.Attribute("Channel").Value);
                    }
                }
            }
            return answer.ToArray();
        }
        public bool isinStreamList(string stream)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            stream = stream.ToLower();
            //Maybe add provider filtering but have to somewhere add the provider
            bool answer = false;

            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                IEnumerable<XElement> childlist = from streams in Streams.Root.Elements() where streams.Attribute("Channel").Value == stream select streams;
                if (childlist.Count() > 0)
                {
                    answer = true;
                }
            }
            return answer;
        }

        public void StreamStartUpdate(string channel, bool end = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                if (!end)
                {

                    IEnumerable<XElement> childlist = from streams in Streams.Root.Elements() where streams.Attribute("Channel").Value == channel select streams;
                    if (childlist.Count() > 0)
                    {
                        foreach (var stream in childlist)
                        {
                            if (stream.Attribute("running").Value == "false")
                            {

                                try
                                {
                                    DateTime start = DateTime.Now;
                                    System.Diagnostics.Debug.WriteLine(start);
                                    DateTime stop = Convert.ToDateTime(stream.Attribute("stoptime").Value);
                                    System.Diagnostics.Debug.WriteLine(stop);
                                    TimeSpan diff = (start - stop).Duration();
                                    System.Diagnostics.Debug.WriteLine(diff.TotalSeconds);
                                    if (diff.TotalSeconds > 600)
                                    {
                                        stream.Attribute("starttime").Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                    
                                }
                                catch (FormatException)
                                {
                                    stream.Attribute("starttime").Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                                stream.Attribute("running").Value = "true";
                            }
                            if(stream.Attribute("lastglobalnotice") == null)
                            {
                                stream.Add(new XAttribute("lastglobalnotice", Convert.ToString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))));
                            }
                        }
                        Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                    }
                }
                else
                {
                    IEnumerable<XElement> childlist = from streams in Streams.Root.Elements() where streams.Attribute("Channel").Value == channel select streams;
                    if (childlist.Count() > 0)
                    {

                        foreach (var stream in childlist)
                        {
                            if (stream.Attribute("running").Value == "true")
                            {
                                stream.Attribute("running").Value = "false";
                                stream.Attribute("stoptime").Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }
                        Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                    }
                }
            }
            return;
        }
        public string StreamInfo(string channel, string inforequested)
        {

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            string answer = "";
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                IEnumerable<XElement> childlist = from streams in Streams.Root.Elements() where streams.Attribute("Channel").Value == channel select streams;
                if (childlist.Count() > 0)
                {
                    foreach (var stream in childlist)
                    {
                        if(stream.Attribute(inforequested) != null)
                        {
                            answer = stream.Attribute(inforequested).Value;
                        }
                        
                    }
                }
            }
            return answer;
        }

        public bool GlobalAnnouncementDue(string channel)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            bool answer = false;

            channel = channel.ToLower();
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Streams.xml"))
            {
                IEnumerable<XElement> childlist = from streams in Streams.Root.Elements() where streams.Attribute("Channel").Value == channel select streams;
                if (childlist.Count() > 0)
                {
                    foreach (var stream in childlist)
                    {
                        if (stream.Attribute("lastglobalnotice") == null)
                        {
                            stream.Add(new XAttribute("lastglobalnotice", Convert.ToString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))));
                            answer = true;
                            Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                        }
                        else
                        {
                            if(stream.Attribute("lastglobalnotice").Value == "")
                            {
                                stream.Attribute("lastglobalnotice").Value = Convert.ToString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                answer = true;
                                Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                            }
                            else
                            {
                                DateTime lastglobalnotice = Convert.ToDateTime(stream.Attribute("lastglobalnotice").Value);
                                TimeSpan difference = DateTime.Now.Subtract(lastglobalnotice);
                                if (difference.TotalMinutes >= 60)
                                {
                                    answer = true;
                                    stream.Attribute("lastglobalnotice").Value = Convert.ToString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    Streams.Save(Directory.GetCurrentDirectory()+"/XML/Streams.xml");
                                }
                            }
                            
                        }
                    }
                }
            }
            return answer;
        }
        #endregion

        #region PickUserStuff etc
        public bool CreateUserPick(string Reason,string User)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Reason = Reason.ToLower();
            User = User.ToLower();
            bool anser = false;
            //Query XML File for User Update
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml"))
            {
                try
                {
                    IEnumerable<XElement> childlist = from Reasons in Streams.Root.Elements() where Reasons.Attribute("Reason").Value == Reason select Reasons;
                    if(childlist.Count() > 0)
                    {
                        
                        foreach (XElement item in childlist)
                        {
                            bool contained = false;
                            foreach(XElement item_item in item.Elements("User"))
                            {
                                if(item_item.Attribute("Value").Value == User)
                                {
                                    contained = true;
                                }
                            }
                            if (contained == false)
                            {
                                item.Add(new XElement("User", new XAttribute("Value", User)));
                                anser = true;
                            }
                            else
                            {
                                anser = false;
                            }
                        }
                        
                    }
                    else
                    {
                        var _element = new XElement("UserPickedList", 
                                new XAttribute("Reason", Reason),
                                new XElement("User", new XAttribute("Value", User))
                        );
                        Streams.Element("UserPickedLists").Add(_element);
                    }
                    Streams.Save(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml");

                    
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    anser = false;
                }
            }
            return anser;
            
        }
        public bool CheckforUserinPick(string Reason,string User)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Reason = Reason.ToLower();
            User = User.ToLower();
            //Query XML File for User Update
            XDocument xdoc = new XDocument();
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml"))
            {
                xdoc = XDocument.Load(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml");
                try
                {
                    IEnumerable<XElement> childlist = from Reasons in xdoc.Root.Elements() where Reasons.Attribute("Reason").Value == Reason select Reasons;
                    if (childlist.Count() > 0)
                    {

                        foreach (XElement item in childlist)
                        {
                            Console.WriteLine(childlist.Count());
                            bool contained = false;
                            foreach (XElement item_item in item.Elements("User"))
                            {
                                Console.WriteLine(item_item.Attribute("Value").Value);
                                if (item_item.Attribute("Value").Value == User)
                                {
                                    return true;
                                }
                            }
                        }
                        return false;

                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }
        public string ReasonUserList(string Reason ="")
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            string answer = "";
            Reason = Reason.ToLower();
            //Query XML File for User Update
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml"))
            {
                try
                {
                    IEnumerable<XElement> childlist;
                    if(Reason != "")
                    {
                        childlist = from Reasons in UserPicks.Root.Elements() where Reasons.Attribute("Reason").Value == Reason select Reasons;
                    }
                    else
                    {
                        childlist = from Reasons in UserPicks.Root.Elements() select Reasons;
                    }
                    
                    if (childlist.Count() > 0)
                    {

                        foreach (XElement item in childlist)
                        {
                            if(Reason != "")
                            {
                                foreach (XElement item_item in item.Elements("User"))
                                {
                                    answer += item_item.Attribute("Value").Value +",";
                                }
                            }
                            else
                            {
                                answer += item.Attribute("Reason").Value + ",";
                            }
                            
                        }

                    }
                    else
                    { 
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
            }
            if(answer == "")
            {
                return answer;
            }

            return answer.Substring(0, answer.Length - 1);

        }
        public bool DeletePickData(string Reason)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            bool answer = false;
            Reason = Reason.ToLower();
            //Query XML File for User Update
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml"))
            {
                try
                {
                    IEnumerable<XElement> childlist = from Reasons in UserPicks.Root.Elements() where Reasons.Attribute("Reason").Value == Reason select Reasons;

                    if (childlist.Count() > 0)
                    {
                        foreach (XElement item in childlist)
                        {
                            item.Remove();
                            answer = true;
                            UserPicks.Save(Directory.GetCurrentDirectory()+"/XML/UserPicks.xml");
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return answer;
        }

        #endregion

        #region Counter stuff
        public string Counter(string counter,bool reset = false, bool read = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            int count = 0;
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Counters.xml"))
            {
                IEnumerable<XElement> childlist = from Counter in Counters.Root.Elements() where Counter.Attribute("Name").Value == counter select Counter;
                if (childlist.Count() > 0)
                {
                    foreach (var _counter in childlist)
                    {
                        if(read)
                        {
                            count = int.Parse(_counter.Attribute("Value").Value);
                        }
                        else
                        {
                            if (reset)
                            {
                                _counter.Attribute("Value").Value = "0";
                            }
                            else
                            {
                                
                                count = int.Parse(_counter.Attribute("Value").Value);
                                count++;
                                _counter.Attribute("Value").Value = count.ToString();
                            }
                        }
                        
                    }
                }
                else
                {
                    try
                    {
                        Counters.Element("Counters").Add(new XElement("Counter", new XAttribute("Value", "1"), new XAttribute("Name", counter)));
                    } catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    
                    count = 1;
                }
            }
            else
            {
                Counters = new XDocument(new XElement("Counters", new XElement("Counter", new XAttribute("Value", "1"),new XAttribute("Name", counter))));
                count = 1;
            }
            Counters.Save(Directory.GetCurrentDirectory()+"/XML/Counters.xml");
            if(reset)
            {
                return "The Counter " + counter + " has been reset and is at " + count;
            }

            return "The Counter " + counter + " is at " + count;
        }
            
        #endregion


        #region Voting stuff
        public int startVote(DateTime enddate,bool multiple,string[] answers, string question)
        {
            
            int output = 0;
            if (!Directory.Exists(Directory.GetCurrentDirectory()+"/XML/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/XML/");
            }
            if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Votes.xml"))
            {
                IEnumerable<XElement> childlist = Votes.Root.Elements();
                output = childlist.Count() + 1;
                XElement Question;

                Question = new XElement("question", new XAttribute("value", question.ToLower().Trim()), new XAttribute("enddate", enddate.ToString("yyyy-MM-dd HH:mm:ss")), new XAttribute("running", "true"), new XAttribute("ID", childlist.Count()+1));
                if (multiple)
                {
                    Question.Add(new XAttribute("multiple", "true"));
                }
                else
                {
                    Question.Add(new XAttribute("multiple", "false"));
                }
                int i = 1;
                foreach (string answer in answers)
                {
                    Question.Add(new XElement("answer", new XAttribute("ID", i.ToString()), new XAttribute("value", answer.ToLower().Trim())));
                    i++;
                }
                Votes.Root.Add(Question);
            }
            else
            {
                output = 1;
                XElement Questions = new XElement("questions");
                XElement Question = new XElement("question", new XAttribute("value", question.ToLower().Trim()), new XAttribute("enddate", enddate.ToString("yyyy-MM-dd HH:mm:ss")), new XAttribute("running", "true"),new XAttribute("ID","1"));
                if(multiple)
                {
                    Question.Add(new XAttribute("multiple","true"));
                }
                else
                {
                    Question.Add(new XAttribute("multiple", "false"));
                }
                int i = 1;
                foreach(string answer in answers)
                {
                    Question.Add(new XElement("answer", new XAttribute("ID",i.ToString()), new XAttribute("value", answer.ToLower().Trim())));
                    i++;
                }
                Questions.Add(Question);
                Votes.Add(Questions);
            }
            Votes.Save(Directory.GetCurrentDirectory()+"/XML/Votes.xml");
            return output;
            
        }
        public int vote(string user,int question,List<int> answer)
        {
            
            try
            {
                bool vote = true;
                if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Votes.xml"))
                {
                    IEnumerable<XElement> checklist = Votes.Root.Elements().Where(_question => _question.Attribute("ID").Value == question.ToString()).Elements();
                    foreach(var item in checklist)
                    {
                        foreach(var _user in item.Elements())
                        {
                            if(_user.Attribute("value").Value == user)
                            {
                                vote = false;
                            }
                        }
                    }
                    if(vote == true)
                    {
                        bool answered = false;
                        foreach(var _answer in answer)
                        {
                            IEnumerable<XElement> childlist = Votes.Root.Elements().Where(_question => _question.Attribute("ID").Value == question.ToString()).Elements().Where(_answers => _answers.Attribute("ID").Value == _answer.ToString());
                            if (childlist.Count() > 0)
                            {
                                foreach (var child in childlist)
                                {
                                    child.Add(new XElement("User", new XAttribute("value", user.ToString())));
                                }
                                Votes.Save(Directory.GetCurrentDirectory()+"/XML/Votes.xml");
                                answered = true;
                            }
                        } 
                        if(answered)
                        {
                            return 1;
                        }
                    }
                }
                else
                {
                    return 3;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 2;
            }
            

            return 0;
        }
        public List<string> VoteResult(int question,bool auto)
        {
            
            List<string> results = new List<string>();
            try
            {
                if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Votes.xml"))
                {
                    IEnumerable<XElement> Question = Votes.Root.Elements().Where(_question => _question.Attribute("ID").Value == question.ToString() && _question.Attribute("running").Value == "false");
                    IEnumerable<XElement> childlist = Votes.Root.Elements().Where(_question => _question.Attribute("ID").Value == question.ToString() && _question.Attribute("running").Value == "false").Elements();
                    if (childlist.Count() > 0)
                    {
                        results.Add("The result for the vote(" + Question.ElementAt(0).Attribute("value").Value + ") are:");
                        foreach (var answer in childlist)
                        {
                            results.Add(answer.Attribute("value").Value + ": " + answer.Elements().Count());
                        }
                    }
                    else
                    {
                        if(!auto)
                        {
                            results.Add("There is no Question matching this ID");
                        }
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return results;
        }
        public int EndVote(int question)
        {
            
            int output = 0;

            try
            {
                if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Votes.xml"))
                {
                    IEnumerable<XElement> childlist = Votes.Root.Elements().Where(_question => _question.Attribute("ID").Value == question.ToString() && _question.Attribute("running").Value == "true");
                    if (childlist.Count() > 0)
                    {
                        foreach(var child in childlist)
                        {
                            child.Attribute("running").Value = "false";
                        }
                        output = 1;
                        Votes.Save(Directory.GetCurrentDirectory()+"/XML/Votes.xml");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return output;
        }

        public List<int> expiredVotes()
        {
            
            List<int> output = new List<int>();

            try
            {
                if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Votes.xml"))
                {
                    IEnumerable<XElement> Question = Votes.Root.Elements().Where(_question => DateTime.Parse(_question.Attribute("enddate").Value) < DateTime.Now && _question.Attribute("running").Value == "true");
                    if(Question.Count() > 0)
                    {
                        foreach (var expired in Question)
                        {
                            output.Add(Int32.Parse(expired.Attribute("ID").Value));
                        }
                    }
                    
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return output;
        }
        public List<VoteObject> runningVotes()
        {
            
            List<VoteObject> output = new List<VoteObject>();

            try
            {
                if (File.Exists(Directory.GetCurrentDirectory()+"/XML/Votes.xml"))
                {
                    IEnumerable<XElement> Question = Votes.Root.Elements().Where(_question => _question.Attribute("running").Value == "true");
                    if(Question != null)
                    {
                        if (Question.Count() > 0)
                        {
                            foreach (var expired in Question)
                            {
                                VoteObject Vote = new VoteObject();
                                Vote.Question = expired.Attribute("value").Value;
                                Vote.QuestionID = Int32.Parse(expired.Attribute("ID").Value);
                                foreach (var answer in Question.Elements())
                                {
                                    Answer input = new Answer();
                                    input.id = Int32.Parse(answer.Attribute("ID").Value);
                                    input.value = answer.Attribute("value").Value;
                                    Vote.Answers.Add(input);
                                }
                                output.Add(Vote);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return output;
        }
        #endregion
      }
}


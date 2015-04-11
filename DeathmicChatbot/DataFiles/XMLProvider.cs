using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using DeathmicChatbot.StreamInfo.Twitch;
using System.Globalization;
using System.Threading;


namespace DeathmicChatbot
{
    class XMLProvider
    {
        #region User Stuff
        public string UserInfo(string nick)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            nick = nick.ToLower();
            string answer = "";
            if (File.Exists("XML/Users.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Users.xml");
                IEnumerable<XElement> childlist = from users in xdoc.Root.Elements()
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
                    return "0," + DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
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
            if (File.Exists("XML/Users.xml"))
            {

                XDocument xdoc = XDocument.Load("XML/Users.xml");
                IEnumerable<XElement> childlist = from users in xdoc.Root.Elements() where users.Attribute("Nick").Value == nick select users;
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
                xdoc.Save("XML/Users.xml");

            }
            return answer;
        }
        // returns All Users Ever Joined/Added and returns them in CSV data as string
        public string AllUserEverJoinedList()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            string answer = "";


            if (File.Exists("XML/Users.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Users.xml");
                IEnumerable<XElement> childlist = from users in xdoc.Root.Elements() select users;
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


            if (File.Exists("XML/Users.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Users.xml");
                IEnumerable<XElement> childlist = from users in xdoc.Root.Elements() where users.Attribute("isloggingOp").Value == "true" select users;
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
            XDocument xdoc = new XDocument();
            if (!Directory.Exists("XML"))
            {
                Directory.CreateDirectory("XML");
            }
            if (File.Exists("XML/Users.xml"))
            {
                xdoc = XDocument.Load("XML/Users.xml");
                try
                {
                    IEnumerable<XElement> childlist = xdoc.Root.Elements().Where(user => user.Attribute("Nick").Value == nick || user.Elements("Alias").Any(alias => alias.Attribute("Value").Value == nick));

                    if (childlist.Count() > 0)
                    {

                        foreach (XElement element in childlist)
                        {

                            element.Attribute("LastVisit").Value = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
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
                            new XAttribute("LastVisit", DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ")),
                            new XAttribute("VisitCount", "1"),
                            new XAttribute("isloggingOp", "false"),
                            new XElement("Alias", new XAttribute("Value", ""))
                            );
                        xdoc.Element("Users").Add(_element);
                        answer = "User added";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    answer = "Add Failure";
                }
            }
            else
            {

                xdoc = new XDocument(new XElement("Users", new XElement("User",
                        new XAttribute("Nick", nick),
                        new XAttribute("LastVisit", DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ")),
                        new XAttribute("VisitCount", "1"),
                        new XAttribute("isloggingOp", "false"),
                            new XElement("Alias", new XAttribute("Value", ""))
                            )));
                answer = "User added";
            }
            xdoc.Save("XML/Users.xml");
            return answer;
        }
        #endregion
        //returns data of User as CSV data in following order VisitCount, LastVisit
        //Adds Alias to User (?where to use no idea implemented because SQlite structure suggested Usage)
        public string AddAlias(string nick, string alias)
        {
            nick = nick.ToLower();
            alias = alias.ToLower();
            XDocument xdoc = new XDocument();
            if (!Directory.Exists("XML"))
            {
                Directory.CreateDirectory("XML");
            }
            if (File.Exists("XML/Users.xml"))
            {
                xdoc = XDocument.Load("XML/Users.xml");
                IEnumerable<XElement> childlist = from el in xdoc.Root.Elements() where el.Attribute("Nick").Value == nick select el;
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
                            xdoc.Save("XML/Users.xml");
                            IEnumerable<XElement> childlist2 = from el in xdoc.Root.Elements() where el.Attribute("Nick").Value == alias select el;
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
                                        xdoc.Save("XML/Users.xml");
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

        #region stream stuff
        public int AddStream(string channel)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            int answer = 0;
            //Query XML File for User Update
            XDocument xdoc = new XDocument();
            if (!Directory.Exists("XML"))
            {
                Directory.CreateDirectory("XML");
            }
            if (File.Exists("XML/Streams.xml"))
            {
                xdoc = XDocument.Load("XML/Streams.xml");
                IEnumerable<XElement> childlist = from el in xdoc.Root.Elements() where el.Attribute("Channel").Value == channel select el;
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
                        xdoc.Element("Streams").Add(_element);
                        answer = 1;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            else
            {

                xdoc = new XDocument(new XElement("Streams", new XElement("Stream",
                            new XAttribute("Channel", channel),
                            new XAttribute("starttime", ""),
                            new XAttribute("stoptime", ""),
                            new XAttribute("running", "false"),
                            new XAttribute("provider", "")
                            )));
                answer = 1;
            }
            xdoc.Save("XML/Streams.xml");
            return answer;
        }

        public string RemoveStream(string channel)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            string answer = "";
            if (File.Exists("XML/Streams.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Streams.xml");
                try
                {
                    xdoc.Element("Streams").Elements("Stream").Where(x => x.Attribute("Channel").Value == channel).Remove();
                    answer = "Stream removed";

                    xdoc.Save("XML/Streams.xml");
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

        // Twitch Double Game Title Fix
        public bool AddStreamdata(string provider ,TwitchStreamData stream)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            bool answer = false;
            try
            {
                if (File.Exists("XML/Streams.xml"))
                {
                    XDocument xdoc = XDocument.Load("XML/Streams.xml");
                    if (provider == "twitch")
                    {
                        IEnumerable<XElement> childlist = from el in xdoc.Root.Elements() where el.Attribute("Channel").Value == stream.Stream.Channel.Name.ToString().ToLower() select el;
                        foreach (var element in childlist)
                        {
                            if (element.Attribute("game") == null)
                            {
                                element.Add(new XAttribute("game", ""));
                            }
                            string game = "";
                            string currentgame = element.Attribute("game").Value;
                            if (currentgame != stream.Stream.Channel.Game.ToString())
                            {
                                game = stream.Stream.Channel.Game.ToString();
                            }
                            if (currentgame != stream.Stream.Game.ToString())
                            {
                                game = stream.Stream.Game.ToString();
                            }
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
                    xdoc.Save("XML/Streams.xml");
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
            if (File.Exists("XML/Streams.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Streams.xml");
                try
                {
                    IEnumerable<XElement> childlist = from el in xdoc.Root.Elements() where el.Attribute("Channel").Value == Channel.ToLower() select el;
                    foreach (var element in childlist)
                    {
                        if (element.Attribute("game") == null)
                        {
                            element.Add(new XAttribute("game", Game));
                            xdoc.Save("XML/Streams.xml");
                        }
                        else
                        {
                            element.Attribute("game").Value = Game;
                        }
                        if (element.Attribute("URL") == null)
                        {
                            element.Add(new XAttribute("URL", URL));
                            xdoc.Save("XML/Streams.xml");
                        }
                        else
                        {
                            element.Attribute("URL").Value = URL;
                        }
                    }
                    xdoc.Save("XML/Streams.xml");
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

            if (File.Exists("XML/Streams.xml"))
            {
                System.Diagnostics.Debug.WriteLine(provider);
                XDocument xdoc = XDocument.Load("XML/Streams.xml");
                IEnumerable<XElement> childlist = from streams in xdoc.Root.Elements() select streams;

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
        public string[] OnlineStreamList()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            //Maybe add provider filtering but have to somewhere add the provider
            List<string> answer = new List<string>();

            if (File.Exists("XML/Streams.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Streams.xml");
                IEnumerable<XElement> childlist = from streams in xdoc.Root.Elements() where streams.Attribute("running").Value == "true" select streams;

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

            if (File.Exists("XML/Streams.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Streams.xml");
                IEnumerable<XElement> childlist = from streams in xdoc.Root.Elements() where streams.Attribute("Channel").Value == stream select streams;
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
            if (File.Exists("XML/Streams.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Streams.xml");
                if (!end)
                {

                    IEnumerable<XElement> childlist = from streams in xdoc.Root.Elements() where streams.Attribute("Channel").Value == channel select streams;
                    if (childlist.Count() > 0)
                    {

                        foreach (var stream in childlist)
                        {
                            if (stream.Attribute("running").Value == "false")
                            {

                                try
                                {
                                    DateTime start = Convert.ToDateTime(stream.Attribute("starttime").Value);
                                    System.Diagnostics.Debug.WriteLine(start);
                                    DateTime stop = Convert.ToDateTime(stream.Attribute("stoptime").Value);
                                    System.Diagnostics.Debug.WriteLine(stop);
                                    TimeSpan diff = (start - stop).Duration();
                                    System.Diagnostics.Debug.WriteLine(diff.TotalSeconds);
                                    if (diff.TotalSeconds > 600)
                                    {
                                        stream.Attribute("starttime").Value = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
                                    }
                                    
                                }
                                catch (FormatException)
                                {
                                    stream.Attribute("starttime").Value = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
                                }
                                stream.Attribute("running").Value = "true";
                            }
                            if(stream.Attribute("lastglobalnotice") == null)
                            {
                                stream.Add(new XAttribute("lastglobalnotice", Convert.ToString(DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ"))));
                            }
                        }
                        xdoc.Save("XML/Streams.xml");
                    }
                }
                else
                {
                    IEnumerable<XElement> childlist = from streams in xdoc.Root.Elements() where streams.Attribute("Channel").Value == channel select streams;
                    if (childlist.Count() > 0)
                    {

                        foreach (var stream in childlist)
                        {
                            if (stream.Attribute("running").Value == "true")
                            {
                                stream.Attribute("running").Value = "false";
                                stream.Attribute("stoptime").Value = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
                            }
                        }
                        xdoc.Save("XML/Streams.xml");
                    }
                }
            }
        }
        public string StreamInfo(string channel, string inforequested)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            string answer = "";
            if (File.Exists("XML/Streams.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Streams.xml");


                IEnumerable<XElement> childlist = from streams in xdoc.Root.Elements() where streams.Attribute("Channel").Value == channel select streams;
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
            else
            {
                
            }
            return answer;
        }

        public bool GlobalAnnouncementDue(string channel)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            bool answer = false;

            channel = channel.ToLower();
            if (File.Exists("XML/Streams.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Streams.xml");


                IEnumerable<XElement> childlist = from streams in xdoc.Root.Elements() where streams.Attribute("Channel").Value == channel select streams;
                if (childlist.Count() > 0)
                {
                    foreach (var stream in childlist)
                    {
                        if (stream.Attribute("lastglobalnotice") == null)
                        {
                            stream.Add(new XAttribute("lastglobalnotice", Convert.ToString(DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ"))));
                            answer = true;
                            xdoc.Save("XML/Streams.xml");
                        }
                        else
                        {
                            DateTime lastglobalnotice = Convert.ToDateTime(stream.Attribute("lastglobalnotice").Value);
                            TimeSpan difference = DateTime.Now.Subtract(lastglobalnotice);
                            if (difference.TotalMinutes >= 60)
                            {
                                answer = true;
                                stream.Attribute("lastglobalnotice").Value = Convert.ToString(DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ"));
                                xdoc.Save("XML/Streams.xml");
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
            XDocument xdoc = new XDocument();
            if (!Directory.Exists("XML"))
            {
                Directory.CreateDirectory("XML");
            }
            if (File.Exists("XML/UserPicks.xml"))
            {
                xdoc = XDocument.Load("XML/UserPicks.xml");
                try
                {
                    IEnumerable<XElement> childlist = from Reasons in xdoc.Root.Elements() where Reasons.Attribute("Reason").Value == Reason select Reasons;
                    if(childlist.Count() > 0)
                    {
                        
                        foreach (XElement item in childlist)
                        {
                            bool contained = false;
                            foreach(XElement item_item in item.Elements("User"))
                            {
                                Console.WriteLine(item_item.Attribute("Value").Value);
                                if(item_item.Attribute("Value").Value == User)
                                {
                                    Console.WriteLine("contained");
                                    contained = true;
                                }
                            }
                            Console.WriteLine(contained);
                            if (contained == false)
                            {
                                Console.WriteLine(contained);
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
                        xdoc.Element("UserPickedLists").Add(_element);
                    }
                    xdoc.Save("XML/UserPicks.xml");

                    
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    anser = false;
                }
            }
            else
            {

                xdoc = new XDocument(new XElement("UserPickedLists",
                            new XElement("UserPickedList", 
                                new XAttribute("Reason", Reason),
                                new XElement("User", new XAttribute("Value", User))
                            )
                        ));
                xdoc.Save("XML/UserPicks.xml");
                anser = true;
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
            if (!Directory.Exists("XML"))
            {
                Directory.CreateDirectory("XML");
            }
            if (File.Exists("XML/UserPicks.xml"))
            {
                xdoc = XDocument.Load("XML/UserPicks.xml");
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
            XDocument xdoc = new XDocument();
            if (!Directory.Exists("XML"))
            {
                Directory.CreateDirectory("XML");
            }
            if (File.Exists("XML/UserPicks.xml"))
            {
                xdoc = XDocument.Load("XML/UserPicks.xml");
                try
                {
                    IEnumerable<XElement> childlist;
                    if(Reason != "")
                    {
                        childlist = from Reasons in xdoc.Root.Elements() where Reasons.Attribute("Reason").Value == Reason select Reasons;
                    }
                    else
                    {
                        childlist = from Reasons in xdoc.Root.Elements() select Reasons;
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
            XDocument xdoc = new XDocument();
            if (!Directory.Exists("XML"))
            {
                Directory.CreateDirectory("XML");
            }
            if (File.Exists("XML/UserPicks.xml"))
            {
                xdoc = XDocument.Load("XML/UserPicks.xml");
                try
                {
                    IEnumerable<XElement> childlist = from Reasons in xdoc.Root.Elements() where Reasons.Attribute("Reason").Value == Reason select Reasons;

                    if (childlist.Count() > 0)
                    {
                        foreach (XElement item in childlist)
                        {
                            item.Remove();
                            answer = true;
                            xdoc.Save("XML/UserPicks.xml");
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
            XDocument xdoc = new XDocument();
            int count = 0;
            if (!Directory.Exists("XML"))
            {
                Directory.CreateDirectory("XML");
            }
            if (File.Exists("XML/Counters.xml"))
            {
                xdoc = XDocument.Load("XML/Counters.xml");
                IEnumerable<XElement> childlist = from Counter in xdoc.Root.Elements() where Counter.Name == counter select Counter;
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
                                Console.WriteLine(_counter.Attribute("Value").Value);
                                count++;
                                _counter.Attribute("Value").Value = count.ToString();
                            }
                        }
                        
                    }
                }
                else
                {
                    xdoc.Add(new XElement("Counters", new XElement(counter, new XAttribute("Value", "1"))));
                    count = 1;
                }
            }
            else
            {
                xdoc = new XDocument(new XElement("Counters", new XElement(counter, new XAttribute("Value", "1"))));
                count = 1;
            }
            xdoc.Save("XML/Counters.xml");
            if(reset)
            {
                return "The Counter " + counter + " has been reset and is at " + count;
            }

            return "The Counter " + counter + " is at " + count;
        }
            
        #endregion

    }
}

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
using System.Xml;

namespace DeathmicChatbot
{
    class XMLProvider
    {
        protected XDocument Users;
        protected List<DataFiles.User> lUsers;
        protected XDocument Streams;
        protected List<DataFiles.internalStream> lStreams;
        protected XDocument UserPicks;
        protected List<DataFiles.UserPickedList> lUserPickLists;
        protected XDocument Votes;
        protected XDocument Counters;
        char slash = Path.DirectorySeparatorChar;
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
            lStreams = new List<DataFiles.internalStream>();
            readFile("Streamsv2.xml", "streams");



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
        public void readFile(string sfilename,string sObject)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + slash+ "XML"+ slash + sfilename))
            {
                
                var path = Directory.GetCurrentDirectory() + slash+"XML"+slash + sfilename;
                FileStream fs = new FileStream(path, FileMode.Open);
                System.Xml.Serialization.XmlSerializer xmlserializer;
                XmlReader reader;
                switch (sObject)
                {
                    case "user":
                        xmlserializer = new System.Xml.Serialization.XmlSerializer(lUsers.GetType());
                        reader = XmlReader.Create(fs);
                        lUsers = (List<DataFiles.User>)xmlserializer.Deserialize(reader);
                        fs.Close();
                        break;
                    case "streams":
                        try
                        {
                            xmlserializer = new System.Xml.Serialization.XmlSerializer(lStreams.GetType());
                            reader = XmlReader.Create(fs);
                            lStreams = (List<DataFiles.internalStream>)xmlserializer.Deserialize(reader);
                            fs.Close();
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        
                        break;
                }
            }
            else
            {
                switch (sObject)
                {
                    case "user":
                        break;
                    case "streams":
                        lStreams = new List<DataFiles.internalStream>();
                        if(Streams!= null)
                        {
                            foreach (var stream in Streams.Element("Streams").Elements("Stream"))
                            {
                                string twitchrelay = "";
                                if(stream.Attribute("twitchchat") != null)
                                {
                                    twitchrelay = stream.Attribute("twitchchat").Value;
                                }
                                bool twoway = false;
                                if (stream.Attribute("twoway") != null)
                                {
                                    if(stream.Attribute("twoway").Value == "1")
                                    {
                                        twoway = true;
                                    }
                                }
                                string targetchannel = "";
                                if (stream.Attribute("targetchannel") != null)
                                {
                                    targetchannel = stream.Attribute("targetchannel").Value;
                                }
                                DataFiles.internalStream newstream = new DataFiles.internalStream(stream.Attribute("Channel").Value, twitchrelay, twoway, targetchannel);
                                lStreams.Add(newstream);
                            }
                        }
                        var path = Directory.GetCurrentDirectory() + slash+"XML"+ slash + sfilename;
                        System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(lStreams.GetType());
                        System.IO.FileStream file = System.IO.File.Create(path);
                        xmlserializer.Serialize(file, lStreams);
                        file.Close();
                        break;
                }
            }
        }
        public void saveFile(string sObject)
        {
            switch(sObject)
            {
                case "streams":
                    var path = Directory.GetCurrentDirectory() + slash + "XML" + slash + "Streamsv2.xml";
                    System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(lStreams.GetType());
                    System.IO.FileStream file = System.IO.File.Create(path);
                    xmlserializer.Serialize(file, lStreams);
                    file.Close();
                    break;
            }
        }
        //returns data of User as CSV data in following order VisitCount, LastVisit
        //Adds Alias to User (?where to use no idea implemented because SQlite structure suggested Usage)
        
        #region stream stuff
        public int TwitchChatToStream(string channel,string targetchannel, int twoway)
        {
            channel = channel.ToLower();
            //Query XML File for User Update
            if(lStreams.Where(x => x.sChannel.ToLower() == channel).Count() > 0)
            {
                lStreams.Where(x => x.sChannel.ToLower() == channel).First().sTargetrelaychannel = targetchannel;
                if(twoway == 1)
                {
                    lStreams.Where(x => x.sChannel.ToLower() == channel).First().bTwoway = true;
                }
                saveFile("streams");
                return 1;
            }
            
            return 0;
        }
        public Tuple<string,bool> GetTwitchChatData(string channel)
        {
            Tuple<string, bool> result = new Tuple<string,bool>("",false);

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
                var stream = lStreams.Where(x => x.sChannel.ToLower() == channel);
                if(stream.Count()>0)
                {
                    result = new Tuple<string, bool>(stream.First().sTargetrelaychannel, stream.First().bTwoway);
                }
            }
            return result;
        }
        public int AddStream(string channel)
        {
            channel = channel.ToLower();
            int answer = 0;
            // 2 there 1 not there added
            if (lStreams.Where(x => x.sChannel.ToLower() == channel.ToLower()).Count() > 0)
            {
                return 2;
            }
            else
            {
                lStreams.Add(new DataFiles.internalStream(channel));
                saveFile("streams");
                return 1;
            }
        }

        public string RemoveStream(string channel)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            channel = channel.ToLower();
            string answer = "";
            if(lStreams.Where(x => x.sChannel.ToLower()==channel.ToLower()).Count() >0)
            {
                try
                {
                    lStreams.RemoveAt(lStreams.FindIndex(x => x.sChannel.ToLower() == channel.ToLower()));
                    answer = "Stream removed";
                    saveFile("streams");
                }
                catch(Exception)
                {
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

                if (provider == "twitch")
                {
                    internalStream iSStream;
                    var streams = lStreams.Where(x => x.sChannel.ToLower() == stream.Stream.Channel.Name.ToString().ToLower());
                    if(streams.Count() >0)
                    {
                        iSStream = streams.First();
                        string game = "";
                        if (iSStream.sGame != stream.Stream.Channel.Game.ToString() && stream.Stream.Channel.Game.ToString() != "")
                        {
                            game = stream.Stream.Channel.Game.ToString();
                        }
                        if (iSStream.sGame != stream.Stream.Game.ToString() && stream.Stream.Game.ToString() != "")
                        {
                            game = stream.Stream.Game.ToString();
                        }
                        game = stripNonValidXMLCharacters(game);
                        game = game.Replace(",", "");
                        game = game.Replace(";", "");
                        iSStream.sGame = game;
                        iSStream.sUrl = stream.Stream.Channel.Url;
                        saveFile("streams");
                    }
                }
                answer = true;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return answer;
        }

        public void AddStreamLivedata(string Channel, string URL,string Game)
        {
                try
                {
                    internalStream iSStream;
                    var streams = lStreams.Where(x => x.sChannel.ToLower() == Channel.ToLower());
                    if (streams.Count() > 0)
                    {
                        iSStream = streams.First();
                        iSStream.sGame = Game;
                        iSStream.sUrl = URL;
                        saveFile("streams");
                    }
                }
                catch (Exception)
                {
                }
        }
        //returns Streamlist as CSV data
        public string StreamList(string provider = "",bool csv = false)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            //Maybe add provider filtering but have to somewhere add the provider
            string answer = "";
            if(csv)
            {
                foreach (var stream in lStreams)
                {
                    answer += stream.sChannel + ",";
                }
                answer = answer.Substring(0, answer.Length - 1);
            }
            else
            {
                int counter = 0;
                bool last = true;
                foreach (var stream in lStreams)
                {
                    answer += stream.sChannel + "  |  ";
                    counter++;
                    last = false;
                    if(counter%5==0)
                    {
                        last = true;
                        answer += System.Environment.NewLine;
                    }
                }
                if(!last)
                {
                    answer = answer.Substring(0, answer.Length - 5);
                }
            }
            
            return answer;
        }
        // Continue porting to new xml HERE MARKER
        public void ResetStreamState()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            //Maybe add provider filtering but have to somewhere add the provider

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
                catch (Exception)
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
                catch (Exception)
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
                catch (Exception)
                {
                }
            }

            return answer;
        }

        #endregion

        #region Counter stuff
        public string Counter(string counter,bool reset = false, bool read = false,int customincrease = 0)
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
                                if(customincrease > 0)
                                {
                                    count += customincrease;
                                }
                                else
                                {
                                    count++;
                                }
                                
                                _counter.Attribute("Value").Value = count.ToString();
                            }
                        }
                        
                    }
                }
                else
                {
                    try
                    {
                        if(customincrease>0)
                        {
                            Counters.Element("Counters").Add(new XElement("Counter", new XAttribute("Value", customincrease.ToString()), new XAttribute("Name", counter)));
                            count = customincrease;
                        }
                        else
                        {
                            Counters.Element("Counters").Add(new XElement("Counter", new XAttribute("Value", "1"), new XAttribute("Name", counter)));
                            count = 1;
                        }
                        
                    } catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    
                    
                }
            }
            else
            {
                if (customincrease > 0)
                {
                    Counters = new XDocument(new XElement("Counters", new XElement("Counter", new XAttribute("Value", customincrease.ToString()), new XAttribute("Name", counter))));
                    count = customincrease;
                }
                else
                {
                    Counters = new XDocument(new XElement("Counters", new XElement("Counter", new XAttribute("Value", "1"), new XAttribute("Name", counter))));
                    count = 1;
                }
                
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


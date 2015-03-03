using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeathmicChatbot.Models;
using System.Xml.Linq;
using System.IO;

namespace DeathmicChatbot
{
    class XMLProvider
    {
        //returns data of User as CSV data in following order VisitCount, LastVisit
        public string UserInfo(string nick)
        {
            string answer = "";
            if (File.Exists("XML/Users.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Users.xml");
                IEnumerable<XElement> childlist = from users in xdoc.Root.Elements() 
                                                  where users.Attribute("nick").Value == nick
                                                  || users.Element("Alias").Attribute("Value").Value == nick
                                                  select users;
                foreach (var user in childlist)
                {
                    answer += user.Attribute("VisitCount").Value + ",";
                    answer += user.Attribute("LastVisit").Value;
                }

            }
            else
            {
                throw new FileNotFoundException(@"File XML/Streams.xml not found");
            }
            return answer;
        }
        public void ToggleUserLogging(string nick)
        {
            if (File.Exists("XML/Users.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Users.xml");
                IEnumerable<XElement> childlist = from users in xdoc.Root.Elements() where users.Attribute("Nick").Value == nick select users;
                foreach(var item in childlist)
                {
                    if(item.Attribute("isloggingOp").Value == "true")
                    {
                        item.Attribute("isloggingOp").Value = "false";
                    }else
                    {
                        item.Attribute("isloggingOp").Value = "true";
                    }
                }
            }     
        }
        // returns All Users Ever Joined/Added and returns them in CSV data as string
        public string AllUserEverJoinedList()
        {
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
                throw new FileNotFoundException(@"File XML/Streams.xml not found");
            }


            return answer;
        }
        //Adds User or Upates information like Visit Count and Last Visit
        public string AddorUpdateUser(string nick, bool leave = false)
        {
            string answer ="";
            //Query XML File for User Update
            XDocument xdoc = new XDocument();
            if(!Directory.Exists("XML"))
            {
                Directory.CreateDirectory("XML");
            }
            if (File.Exists("XML/Users.xml"))
            {
                xdoc = XDocument.Load("XML/Users.xml");
                try
                {
                    IEnumerable<XElement> childlist = from el in xdoc.Root.Elements() where el.Attribute("Nick").Value == nick select el;
                    if (childlist.Count() > 0)
                    {

                        foreach (XElement element in childlist)
                        {

                            element.Attribute("LastVisit").Value = DateTime.Now.ToString();
                            if(!leave)
                            {
                                int _visitcount = Int32.Parse(element.Attribute("VisitCount").Value);
                                _visitcount++;
                                element.Attribute("VisitCount").Value = _visitcount.ToString();
                                answer = "User updated";
                            }
                        }
                    }
                    else
                    {
                        var _element = new XElement("User",
                            new XAttribute("Nick", nick),
                            new XAttribute("LastVisit", DateTime.Now.ToString()),
                            new XAttribute("VisitCount", "1")
                            );
                        xdoc.Element("Users").Add(_element);
                        answer = "User added";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    answer ="Add Failure";
                }
            }
            else
            {

                xdoc = new XDocument(new XElement("Users",new XElement("User",
                        new XAttribute("Nick", nick),
                        new XAttribute("LastVisit", DateTime.Now.ToString()),
                        new XAttribute("VisitCount", "1"),
                        new XAttribute("isloggingOp","false")
                            )));
                answer = "User added";
            }
            xdoc.Save("XML/Users.xml");
            return answer;
        }
        //Adds Alias to User (?where to use no idea implemented because SQlite structure suggested Usage)
        public string AddAlias(string nick ,string alias)
        {
             XDocument xdoc = new XDocument();
            if(!Directory.Exists("XML"))
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
                           if(test.Attribute("Value").Value == alias)
                           {
                               aliascontained = true;
                           }
                        }
                        if(aliascontained == false)
                        {
                            item.Add(new XElement("Alias", new XAttribute("Value", alias)));
                            xdoc.Save("XML/Users.xml");
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

        public string AddStream(string channel)
        {
            string answer = "";
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
                    
                    answer = "Stream already added";
                }
                else
                {
                    try
                    {
                        var _element = new XElement("Stream",
                           new XAttribute("Channel", channel),
                            new XAttribute("starttime", ""),
                            new XAttribute("running", "false")
                           );
                        xdoc.Element("Streams").Add(_element);
                        answer = "Stream added";

                    }catch(Exception ex)
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
                            new XAttribute("running","false")
                            )));
                answer = "Stream added";
            }
            xdoc.Save("XML/Streams.xml");
            return answer;
        }

        public string RemoveStream(string channel)
        {
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

        //returns Streamlist as CSV data
        public string StreamList()
        {
            string answer = "";


            if (File.Exists("XML/Streams.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Streams.xml");
                IEnumerable<XElement> childlist = from streams in xdoc.Root.Elements() select streams;
                foreach(var stream in childlist)
                {
                    answer += stream.Attribute("Channel").Value + ",";
                }
                answer = answer.Substring(answer.Length - 1, answer.Length);
                
            }
            else
            {
                throw new FileNotFoundException(@"File XML/Streams.xml not found");
            }

            return answer;
        }

        public void StreamStartUpdate(string channel,bool end = false)
        {
            
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
                                stream.Attribute("starttime").Value = DateTime.Now.ToString();
                                stream.Attribute("running").Value = "true";
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
                            }
                        }
                        xdoc.Save("XML/Streams.xml");
                    }
                }
            }
            else
            {
                throw new FileNotFoundException(@"File XML/Streams.xml not found");
            }

        }
        public string StreamInfo(string channel, string inforequested)
        {
            string answer = "";
            if (File.Exists("XML/Streams.xml"))
            {
                XDocument xdoc = XDocument.Load("XML/Streams.xml");


                IEnumerable<XElement> childlist = from streams in xdoc.Root.Elements() where streams.Attribute("Channel").Value == channel select streams;
                if (childlist.Count() > 0)
                {

                    foreach (var stream in childlist)
                    {
                        answer = stream.Attribute(inforequested).Value;
                    }
                    xdoc.Save("XML/Streams.xml");
                }                
            }
            else
            {
                throw new FileNotFoundException(@"File XML/Streams.xml not found");
            }
            return answer;
        } 
    }
}

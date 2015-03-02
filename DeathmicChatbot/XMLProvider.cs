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
        public string AddorUpdateUser(string nick)
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
                            int _visitcount = Int32.Parse(element.Attribute("VisitCount").Value);
                            _visitcount++;
                            element.Attribute("VisitCount").Value = _visitcount.ToString();
                            answer ="User updated";
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
                        new XAttribute("VisitCount", "1"))
                            ));
                answer = "User added";
            }
            xdoc.Save("XML/Users.xml");
            return answer;
        }

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
                return "Users.xml does not exist, so no alias can be added";
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
                            new XAttribute("Message", ""),
                            new XAttribute("Game", "")
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
                            new XAttribute("Message", ""),
                            new XAttribute("Game","")
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
    }
}

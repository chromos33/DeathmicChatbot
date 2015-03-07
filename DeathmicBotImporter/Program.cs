using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace DeathmicBotImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            List<UserModel> userlist = new List<UserModel>();
            if (Directory.Exists("join_logging"))
            {
                string[] userfiles = Directory.GetFiles(Directory.GetCurrentDirectory(),"join_logging\\*");
                foreach (var file in userfiles)
                {
                    UserModel user = new UserModel();

                    string customize = file.Substring(13);
                    user.Nick = customize.Substring(0, customize.Length - 4);
                    string[] filecontent = System.IO.File.ReadAllLines(file);
                    user.LastVisit = filecontent[filecontent.Length - 1];
                    user.VisitCount = filecontent.Length.ToString();
                    userlist.Add(user);
                }
                foreach (var insertuser in userlist)
                {
                    AddorUpdateUser(insertuser.Nick, insertuser.LastVisit, insertuser.VisitCount);
                }
            }
            try
            {
                string[] streamfilecontent = System.IO.File.ReadAllLines("streams_twitch.txt");
                foreach (var streamname in streamfilecontent)
                {
                    AddStream(streamname, "Admin");
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("streams_twitch.txt not found");
            }



        }
        public static string AddorUpdateUser(string nick, string LastVisit, string VisitCount)
        {
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
                    }
                    else
                    {
                        var _element = new XElement("User",
                            new XAttribute("Nick", nick),
                            new XAttribute("LastVisit", LastVisit),
                            new XAttribute("VisitCount", VisitCount),
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
                        new XAttribute("LastVisit", LastVisit),
                        new XAttribute("VisitCount", VisitCount),
                        new XAttribute("isloggingOp", "false"),
                            new XElement("Alias", new XAttribute("Value", ""))
                            )));
                answer = "User added";
            }
            xdoc.Save("XML/Users.xml");
            return answer;
        }
        public static string AddStream(string channel, string user)
        {
            channel = channel.ToLower();
            user = user.ToLower();
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

                    answer = user + " wanted to readd Stream to the streamlist.";
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
                        answer = user + " added Stream to the streamlist";

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
                answer = user + " added Stream to the streamlist";
            }
            xdoc.Save("XML/Streams.xml");
            return answer;
        }
    }
}

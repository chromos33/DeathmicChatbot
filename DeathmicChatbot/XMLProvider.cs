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
        public void AddorUpdateUser(string nick)
        {
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
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.ReadKey();
                }
            }
            else
            {
                List<UserModel> _users = new List<UserModel>();

                UserModel _user = new UserModel();
                _user.nick = "Init";
                _user.lastvisit = new DateTime(2015, 03, 01);
                _user.visitcount = 0;
                _users.Add(_user);

                xdoc = new XDocument(new XElement("Users",
                    from user in _users
                    select new XElement("User",
                        new XAttribute("Nick", nick),
                        new XAttribute("LastVisit", user.lastvisit),
                        new XAttribute("VisitCount", user.visitcount))
                            ));
            }
            xdoc.Save("XML/Users.xml");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace DeathmicChatbot.DataFiles
{
    public class User
    {
        
        public String Name;
        public DateTime LastVisit;
        public int VisitCounter;
        public bool bIsLoggingOp;
        public string password;
        public List<Stream> Streams = new List<Stream>();
        public List<Alias> Aliase = new List<Alias>();
        public bool bMessages;
        public bool globalhourlyannouncement;
        public bool isGlobalAnnouncment(string channel)
        {
            if(Streams.Where(x=>x.name.ToLower() == channel.ToLower() && x.hourlyannouncement).Count()>0)
            {
                return true;
            }
            if (globalhourlyannouncement)
            {
                return true;
            }
            return false;
        }
        public bool toggleLoggingOp()
        {
            bIsLoggingOp = !bIsLoggingOp;
            return bIsLoggingOp;
        }
        public bool bShouldSubscribe()
        {
            if(LastVisit < DateTime.Now.AddYears(-1))
            {
                return false;
            }

            return true;
        }
        public User()
        {
            if(password == null)
            {
                password = "";
            }
        }
        public bool toggleMessages()
        {
            bMessages = !bMessages;
            return bMessages;
        }
        public bool checkPassword(string _password)
        {
            if(_password == password)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool changePassword(string newpass, string oldpass= "")
        {
            if(password == oldpass)
            {
                password = newpass;
                return true;
            }
            return false;
            
        }

        public bool isSubscribed(string streamname)
        {
            IEnumerable<Stream> stream = Streams.Where(x => x.name == streamname.ToLower());
            if(stream.Count() > 0)
            {
                return stream.First().subscribed;
            }
            return false;
        }
        public bool subscribeStream(string streamname,string _password,bool subscribe)
        {
            if(/*_password == password*/true)
            {
                IEnumerable<Stream> streams = Streams.Where(x => x.name.ToLower() == streamname.ToLower());
                if (streams.Count() > 0)
                {
                    streams.First().subscribed = subscribe;
                }
                return true;
            }
            return false;
            
        }
        public bool addStream(string streamname,bool subscribe = false)
        {
            IEnumerable<Stream> streams = Streams.Where(x => x.name == streamname.ToLower());
            if(streams.Count() > 0)
            {
                return false;
            }
            Streams.Add(new Stream(streamname.ToLower(), subscribe));
            return true;
        }
        public void removeStream(string streamname)
        {
            Streams.RemoveAll(x=>x.name.ToLower() == streamname.ToLower());
        }
        public Tuple<DateTime, int> visit()
        {
            if(LastVisit == null)
            {
                LastVisit = DateTime.Now;
            }
            VisitCounter++;
            Tuple<DateTime, int> returnvalue = new Tuple<DateTime, int>(LastVisit, VisitCounter);
            LastVisit = DateTime.Now;
            return returnvalue;
        }
        public void leave()
        {
            if (LastVisit == null)
            {
                LastVisit = DateTime.Now;
            }
            LastVisit = DateTime.Now;
        }
        public bool HasAlias(string Alias)
        {
            IEnumerable<Alias> aliase = Aliase.Where(x => x.Name == Alias);
            if(aliase.Count() > 0)
            {
                return true;
            }
            return false;
        }
        public bool isUser(string name)
        {
            if(Name.ToLower() == name.ToLower())
            {
                return true;
            }
            IEnumerable<Alias> aliasfound = Aliase.Where(x => x.Name.ToLower() == name.ToLower());
            if(aliasfound.Count() >0)
            {
                return true;
            }
            return false;
        }
        public bool AddAlias(string childname,List<User> userlist)
        {
            IEnumerable<User> children = userlist.Where(x => x.Name == childname);
            User child = null;
            if(children.Count()>0)
            {
                child = children.First();
            }
            if(child != null)
            {
                if(!HasAlias(childname))
                {
                    Aliase.Add(new Alias(child.Name, child.VisitCounter, child.LastVisit));
                    userlist.RemoveAll(x => x.Name.ToLower() == childname.ToLower());
                    return true;
                }
            }
            return false;
        }
        public User RemoveAlias(string aliasname)
        {
            IEnumerable<Alias> _Aliase = Aliase.Where(x => x.Name.ToLower() == aliasname.ToLower());
            if(_Aliase.Count()>0)
            {
                User RestoredUser = new User();
                RestoredUser.LastVisit = _Aliase.FirstOrDefault().LastVisitBackup;
                RestoredUser.Name = _Aliase.FirstOrDefault().Name;
                RestoredUser.VisitCounter = _Aliase.FirstOrDefault().VisitCounterBackup;
                Aliase.RemoveAll(x => x.Name.ToLower() == aliasname.ToLower());
                return RestoredUser;
            }
            else
            {
                return null;
            }
            
        }
    }
}

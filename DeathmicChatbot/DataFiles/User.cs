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
        public User()
        {

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
        public bool addStream(string streamname,bool subscribe = false)
        {
            IEnumerable<Stream> streams = Streams.Where(x => x.name == streamname.ToLower());
            if(streams.Count() > 0)
            {
                return false;
            }
            Streams.Add(new Stream(streamname, subscribe));
            return true;
        }
        public bool updateSubscription(string streamname,bool suscribe = true)
        {
            IEnumerable<Stream> streams = Streams.Where(x => x.name == streamname.ToLower());

            if(streams.Count()>0)
            {
                foreach (Stream stream in streams)
                {
                    stream.subscribed = suscribe;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public Tuple<string,int> visit()
        {
            VisitCounter++;
            LastVisit = DateTime.Now;
            return new Tuple<string, int>(LastVisit.ToString("yyyy-MM-dd HH:mm:ss"), VisitCounter);
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
        public bool AddAlias(string parentname,string childname,List<User> userlist)
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
                    return true;
                }
            }
            return false;

        }
    }
}

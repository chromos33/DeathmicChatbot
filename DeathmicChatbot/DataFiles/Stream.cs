using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathmicChatbot.DataFiles
{
    class Stream
    {
        public string name;
        public bool subscribed;

        public Stream(string _name,bool _subscribed)
        {
            name = _name;
            subscribed = _subscribed;
        }
    }
}

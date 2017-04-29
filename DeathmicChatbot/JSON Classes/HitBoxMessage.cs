using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathmicChatbot.HitBoxMessage
{
    public class Root
    {
        public string name;
        public Arguments[] args;
    }
    public class Arguments
    {
        public string method;
        public Parameters @params;
        
    }
    public class Parameters
    {
        public string channel;
        public string name;
        public string token;
        public string hideBuffered;
        public string role;
        public string nameColor;
        public string text;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathmicChatbot.StreamInfo.Twitch.Chatters
{
    public class Root
    {
        public Links _links { get; set;}
        public int chatter_count { get; set; }
        public chatter chatters { get; set; }
        
    }
    public class chatter
    {
        public string[] moderators { get; set; }
        public string[] staff { get; set; }
        public string[] admins { get; set; }
        public string[] global_mods { get; set; }
        public string[] viewers {get;set;} 
    }
}

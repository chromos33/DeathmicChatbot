using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathmicChatbot.DataFiles
{
    public class TriggerReply
    {
        public string sTrigger;
        public string sReply;
        public TriggerReply()
        {

        }
        public TriggerReply(string _trigger,string _reply)
        {
            sTrigger = _trigger;
            sReply = _reply;
        }
    }
}

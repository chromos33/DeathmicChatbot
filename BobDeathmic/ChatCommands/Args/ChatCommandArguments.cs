using BobDeathmic.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ChatCommands.Args
{
    public class ChatCommandArguments
    {
        public string Message { get; set; }
        public string Sender { get; set; }
        public ChatType Type {get; set;}
        public string ChannelName { get; set; }

        public bool elevatedPermissions { get; set; }
        

        
    }
}

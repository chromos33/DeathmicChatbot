using BobDeathmic.Eventbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ChatCommands.Args
{
    public class ChatCommandOutput
    {
        public EventType Type { get; set; }
        public dynamic EventData { get; set; }
        public bool ExecuteEvent { get; set; }
    }
}

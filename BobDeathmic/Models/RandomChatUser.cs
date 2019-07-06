using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models
{
    public class RandomChatUser
    {
        public int ID { get; set; }
        public string Stream { get; set; }
        public int Sort { get; set; }
        public string ChatUser { get; set; }
        public bool lastchecked { get; set; }

    }
}

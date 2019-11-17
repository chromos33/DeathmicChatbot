using BobDeathmic;
using BobDeathmic.Data.DBModels.Commands;
using BobDeathmic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Data.DBModels.Commands
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

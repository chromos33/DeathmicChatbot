using BobDeathmic.Models;
using BobDeathmic.Models.GiveAwayModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Args
{
    public class GiveAwayEventArgs
    {
        public string channel { get; set; }
        public ChatUserModel winner { get; set; }
    }
}

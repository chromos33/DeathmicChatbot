using BobDeathmic.Data.DBModels.GiveAway;
using BobDeathmic.Data.DBModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic;
using BobDeathmic.Models;

namespace BobDeathmic.ViewModels.GiveAway
{
    public class GiveAwayAdminViewModel
    {
        public string Item;
        public List<String> Channels;
        public List<string> Applicants;
        public string Link { get; set; }
    }
}

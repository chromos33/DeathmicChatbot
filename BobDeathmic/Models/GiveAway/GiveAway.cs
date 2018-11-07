using BobDeathmic.Helper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.GiveAwayModels
{
    public class GiveAway
    {
        public int GiveAwayId { get; set; }
        public string Title { get; set; }
        public List<GiveAwayItem> GiveAwayItems { get; set; }
        public ChatUserModel Admin { get; set; }
    }
}

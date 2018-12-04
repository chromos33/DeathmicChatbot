using BobDeathmic.Models.GiveAwayModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.GiveAway
{
    public class User_GiveAwayItem
    {
        public int User_GiveAwayItemID;

        public string UserID { get; set; }
        public ChatUserModel User { get; set; }

        public int giveawayitemID { get; set; }
        public GiveAwayItem giveawayitem { get; set; }
    }
}

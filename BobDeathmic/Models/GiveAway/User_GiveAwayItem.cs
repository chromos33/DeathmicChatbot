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

        public int GiveAwayItemID { get; set; }
        public GiveAwayItem GiveAwayItem { get; set; }

        public User_GiveAwayItem()
        {

        }
        public User_GiveAwayItem(ChatUserModel user, GiveAwayItem item)
        {
            UserID = user.Id;
            User = user;
            GiveAwayItemID = item.GiveAwayItemID;
            GiveAwayItem = item;
        }
    }
}

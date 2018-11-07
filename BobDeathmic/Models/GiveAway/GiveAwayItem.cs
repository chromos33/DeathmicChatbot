using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.GiveAwayModels
{
    public class GiveAwayItem
    {
        public int GiveAwayItemId { get; set; }
        public string Title { get; set; }
        public string Key { get; set; }
        public string SteamID { get; set; }
        public string Link { get; set; }
        public int Views { get; set; }
        public GiveAway GiveAway {get;set;}
        
        public ChatUserModel Owner { get; set; }
        
        public ChatUserModel Receiver { get; set; }
        
    }
}

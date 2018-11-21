using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.GiveAwayModels
{
    public class GiveAwayItem
    {
        public int GiveAwayItemId { get; set; }

        [Required]
        public string Title { get; set; }
        public string Key { get; set; }
        public string SteamID { get; set; }
        [Required]
        [Url]
        public string Link { get; set; }
        public int Views { get; set; }
        
        public ChatUserModel Owner { get; set; }
        
        public ChatUserModel Receiver { get; set; }
        
    }
}

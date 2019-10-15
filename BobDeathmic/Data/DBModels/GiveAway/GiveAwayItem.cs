using BobDeathmic.Models.GiveAway;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.GiveAwayModels
{
    public class GiveAwayItem
    {
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string Key { get; set; }
        public string SteamID { get; set; }
        [Required]
        [Url]
        public string Link { get; set; }
        public int Views { get; set; }
        public bool current { get; set; }
        public string OwnerID { get; set; }
        public ChatUserModel Owner { get; set; }
        public string ReceiverID { get; set; }
        public ChatUserModel Receiver { get; set; }

        public List<User_GiveAwayItem> Applicants { get; set; }

        public string Announcement()
        {
            return "GiveAway für " + Title + " mit !Gapply teilnehmen";
        }
        public string WinnerAnnouncment()
        {
            return "Gewonnen hat " + Receiver.ChatUserName;
        }

    }
}

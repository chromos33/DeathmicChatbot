using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.EventDateFinder.ManyMany
{
    public class ChatUserModel_Calendar
    {
        [Key]
        public string ChatUserModel_CalendarID;
        public string CalendarID { get; set; }
        public Calendar Calendar { get; set; }
        public string ChatUserModelID { get; set; }
        public ChatUserModel ChatUserModel { get; set; }
    }
}

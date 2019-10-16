using BobDeathmic;
using BobDeathmic.Data.DBModels.EventCalendar.manymany;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Models;
using BobDeathmic.Models.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Data.DBModels.EventCalendar.manymany
{
    public class ChatUserModel_Event
    {
        public string ChatUserModel_EventID { get; set; }

        public int CalendarID { get; set; }
        public Event Calendar { get; set; }
        public string ChatUserModelID { get; set; }
        public ChatUserModel ChatUserModel { get; set; }
    }
}

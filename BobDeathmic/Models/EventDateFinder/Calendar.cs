using BobDeathmic.Models.EventDateFinder.ManyMany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.EventDateFinder
{
    public class Calendar
    {
        public string ID { get; set; }
        public List<AppointmentRequestTemplate> AppointmentRequestTemplate { get; set; }
        public List<ChatUserModel_Calendar> Members { get; set; }
        public ChatUserModel Admin { get; set; }
        public List<EventDate> EventDates { get; set; }
    }
}

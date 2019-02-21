using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.EventDateFinder
{
    //Missing counter relation to ChatUserModel
    public class AppointmentRequest
    {
        public string ID { get; set; }
        public EventDate EventDate { get; set; }
        public ChatUserModel Owner { get; set; }
    }
}


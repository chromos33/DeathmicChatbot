using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.EventDateFinder
{
    public class AppointmentRequestTemplate
    {
        public string ID { get; set; }
        public Calendar Calendar { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
    }
}

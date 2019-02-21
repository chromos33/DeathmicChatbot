using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.EventDateFinder
{
    //Wird in der Abstimmung generiert für z.b. 14 Tage im Vorraus und dann jeden Tag wird das älteste bzw "15 te" gelöscht
    public class EventDate
    {
        public string ID { get; set; }
        public Calendar Calendar { get; set; }
        public List<AppointmentRequest> Teilnahmen { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.Events
{
    //Wird in der Abstimmung generiert für z.b. 14 Tage im Vorraus und dann jeden Tag wird das älteste bzw "15 te" gelöscht
    public class EventDate
    {
        public string ID { get; set; }
        public int CalendarId { get; set; }
        public Event Event { get; set; }
        public string EventDateTemplateID { get; set; }
        public List<AppointmentRequest> Teilnahmen { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime Date { get; set; }

        internal string AdminNotification(string name = "Calendar")
        {
            return $"Möglicher Termin für {name}: {Date.ToString("dd.MM.yyyy HH:mm")}";
        }
    }
}
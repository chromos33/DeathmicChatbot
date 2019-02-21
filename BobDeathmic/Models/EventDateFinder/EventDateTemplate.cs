using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.EventDateFinder
{
    //Template aus dem EventDates generiert werden
    //TODO Rename is confusing cause in reality it is a EventDateTemplate
    public class EventDateTemplate
    {
        public string ID { get; set; }
        public Calendar Calendar { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public Day Day { get; set; }
    }
    public enum Day
    {
        Montag = 1,
        Dienstag = 2,
        Mittwoch = 3,
        Donnerstag = 4,
        Freitag = 5,
        Samstag = 6,
        Sonntag = 7
    }
}

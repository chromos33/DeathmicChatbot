using BobDeathmic.Data.DBModels.EventCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Models.Events;
using BobDeathmic;
using BobDeathmic.Models;

namespace BobDeathmic.Data.DBModels.EventCalendar
{
    //Template aus dem EventDates generiert werden
    //TODO Rename is confusing cause in reality it is a EventDateTemplate
    public class EventDateTemplate
    {
        public string ID { get; set; }
        public Event Event { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public Day Day { get; set; }

        //Pos 1 / 2 cause only showing 2 weeks)
        public EventDate CreateEventDate(int Pos)
        {
            EventDate Clone = new EventDate();
            Clone.Event = Event;
            Clone.StartTime = StartTime;
            Clone.StopTime = StopTime;
            TimeSpan add = new TimeSpan();
            DateTime today = DateTime.Today.Add(new TimeSpan(1, StartTime.Hour, StartTime.Minute, 0));

            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysUntilTargetDay = (((int)Day - (int)today.DayOfWeek + 7) % 7) + Pos * 7;
            DateTime nextDayOccurence = today.AddDays(daysUntilTargetDay);
            Clone.Date = nextDayOccurence;
            return Clone;
        }
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

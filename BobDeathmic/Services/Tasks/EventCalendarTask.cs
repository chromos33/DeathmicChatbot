using BobDeathmic.Data;
using BobDeathmic.Models.EventDateFinder;
using BobDeathmic.Services.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Tasks
{
    public class EventCalendarTask : IScheduledTask
    {
        public string Schedule => "* * * * *";
        private readonly ApplicationDbContext _context;
        public EventCalendarTask(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if(_context != null)
            {
                var ApplicableCalendars = _context.EventCalendar.Include(x => x.EventDateTemplates).Include(x => x.EventDates).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Members.Count() > 0 && x.EventDateTemplates.Count() > 0);
                RemovePassedEventDates();
                foreach (Calendar calendar in ApplicableCalendars)
                {
                    AddEventDatesOnCalendar(calendar);
                    UpdateEventDates(calendar);
                    _context.SaveChanges();
                }
            }
        }
        private void RemovePassedEventDates()
        {
            
            var EventDatesToRemove = _context.EventDates.Where(x => x.Date < DateTime.Now);
            foreach(EventDate remove in EventDatesToRemove)
            {
                _context.EventDates.Remove(remove);

            }
            foreach(Calendar calendar in _context.EventCalendar)
            {
                var EventDatesInCalendarToRemove = calendar.EventDates.Where(x => x.Date < DateTime.Now);
                foreach (EventDate remove in EventDatesInCalendarToRemove)
                {
                    calendar.EventDates.Remove(remove);
                }
            }
        }
        private void AddEventDatesOnCalendar(Calendar calendar)
        {
            // Next 2 weeks
            for(int week = 0; week < 2;week++ )
            {
                foreach(EventDateTemplate template in calendar.EventDateTemplates)
                {
                    EventDate eventdate = template.CreateEventDate(week);
                    eventdate.EventDateTemplateID = template.ID;
                    if(calendar.EventDates.Where( x => x.Date == eventdate.Date && x.StartTime == eventdate.StartTime && x.StopTime == eventdate.StopTime).Count() == 0)
                    {
                        eventdate.Teilnahmen = calendar.GenerateAppointmentRequests(eventdate);
                        _context.EventDates.Add(eventdate);
                        calendar.EventDates.Add(eventdate);
                    }
                }
            }
        }
        private void UpdateEventDates(Calendar calendar)
        {
            foreach (EventDate update in calendar.EventDates)
            {
                var template = _context.EventDateTemplates.Where(x => x.ID == update.EventDateTemplateID).FirstOrDefault();
                if(template != null)
                {
                    update.StartTime = template.StartTime;
                    update.StopTime = template.StopTime;
                    update.Date =  DateTime.ParseExact(update.Date.ToString("dd-MM-yyyy") + " " + update.StartTime.ToString("HH:mm"), "dd-MM-yyyy HH:mm",
                                    System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }
    }
}

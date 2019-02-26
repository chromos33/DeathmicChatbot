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
                var ApplicableCalendars = _context.EventCalendar.Include(x => x.EventDateTemplates).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Members.Count() > 0 && x.EventDateTemplates.Count() > 0);
                foreach(Calendar calendar in ApplicableCalendars)
                {
                    RemovePassedEventDates(calendar);
                    UpdateEventDatesOnCalendar(calendar);
                    _context.SaveChanges();
                }
            }
        }
        private void RemovePassedEventDates(Calendar calendar)
        {
            var EventDatesToRemove = calendar.EventDates.Where(x => x.Date < DateTime.Now);
            foreach(EventDate remove in EventDatesToRemove)
            {
                calendar.EventDates.Remove(remove);
            }
        }
        private void UpdateEventDatesOnCalendar(Calendar calendar)
        {
            // Next 2 weeks
            for(int week = 0; week < 2;week++ )
            {
                foreach(EventDateTemplate template in calendar.EventDateTemplates)
                {
                    var eventdate = template.CreateEventDate(week);
                    if(calendar.EventDates.Where( x => x.Date == eventdate.Date).Count() == 0)
                    {
                        //TODO Add AppointmentRequests to EventDate
                        eventdate.Teilnahmen = calendar.GenerateAppointmentRequests(eventdate);
                        calendar.EventDates.Add(eventdate);
                    }
                }
            }
        }
    }
}

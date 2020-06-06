using BobDeathmic.Args;
using BobDeathmic.Cron.Setup;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.EventCalendar;
using BobDeathmic.Eventbus;
using BobDeathmic.Helper;
using BobDeathmic.Helper.EventCalendar;
using BobDeathmic.Models.Events;
using BobDeathmic.Services.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Cron
{
    public class EventCalendarTask : IScheduledTask
    {
        public string Schedule => "0 18 * * *";
        private readonly IServiceScopeFactory _scopeFactory;
        private IEventBus _eventBus;
        private IConfiguration _configuration;
        public EventCalendarTask(IServiceScopeFactory scopeFactory, IEventBus eventBus, IConfiguration Configuration)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            _configuration = Configuration;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (_context != null)
                {

                    var ApplicableCalendars = _context.Events.Include(x => x.EventDateTemplates).Include(x => x.EventDates).ThenInclude(x => x.Teilnahmen).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Members.Count() > 0 && x.EventDateTemplates.Count() > 0);
                    RemovePassedEventDates();
                    foreach (Event calendar in ApplicableCalendars)
                    {
                        AddEventDatesOnCalendar(calendar.Id);
                        UpdateEventDates(calendar.Id);
                        _context.SaveChanges();
                        NotifyUsers(calendar);
                        NotifyAdmin(calendar);
                    }
                }
            }
        }

        private void NotifyAdmin(Event calendar)
        {
            var NotifiableEventDates = calendar.EventDates.AsQueryable().Where(x => x.Teilnahmen.Where(y => y.State == AppointmentRequestState.Available || y.State == AppointmentRequestState.IfNeedBe).Count() == x.Teilnahmen.Count());
            foreach (EventDate date in NotifiableEventDates)
            {
                _eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, new MessageArgs { RecipientName = calendar.Admin.ChatUserName, Message = date.AdminNotification(calendar.Name) });
            }
        }

        private void NotifyUsers(Event calendar)
        {
            string address = _configuration.GetValue<string>("WebServerWebAddress");
            List<MutableTuple<string, string>> GroupedNotifications = new List<MutableTuple<string, string>>();
            foreach (EventDate date in calendar.EventDates.AsQueryable().Where(x => x.Date.Date == DateTime.Now.Add(TimeSpan.FromDays(5)).Date || x.Date.Date == DateTime.Now.Add(TimeSpan.FromDays(2)).Date || x.Date.Date == DateTime.Now.Add(TimeSpan.FromDays(1)).Date))
            {
                foreach (AppointmentRequest request in date.Teilnahmen.Where(x => x.State == AppointmentRequestState.NotYetVoted))
                {
                    if (GroupedNotifications.Where(x => x.First == request.Owner.UserName).Count() == 0)
                    {
                        GroupedNotifications.Add(new MutableTuple<string, string>(request.Owner.UserName, $"Freundliche Errinnerung {request.Owner.UserName} du musst im Kalendar {request.EventDate.Event.Name} für den {request.EventDate.Date.ToString("dd.MM.yyyy HH:mm")} abstimmen. {address}/Events/VoteOnCalendar/{calendar.Id}"));
                    }
                    else
                    {
                        GroupedNotifications.Where(x => x.First == request.Owner.UserName).FirstOrDefault().Second += Environment.NewLine + $"Freundliche Errinnerung {request.Owner.UserName} du musst im Kalender {request.EventDate.Event.Name} für den {request.EventDate.Date.ToString("dd.MM.yyyy HH:mm")} abstimmen. {address}/Events/VoteOnCalendar/{calendar.Id}";
                    }
                }
            }
            foreach (MutableTuple<string, string> tuple in GroupedNotifications)
            {

                _eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, new MessageArgs { RecipientName = tuple.First, Message = tuple.Second });
            }
        }

        private void RemovePassedEventDates()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var EventDatesToRemove = _context.EventDates.AsQueryable().Where(x => x.Date < DateTime.Now);
                foreach (EventDate remove in EventDatesToRemove)
                {
                    _context.EventDates.Remove(remove);

                }
                _context.SaveChanges();
                foreach (Event calendar in _context.Events)
                {
                    var EventDatesInCalendarToRemove = calendar.EventDates.AsQueryable().Where(x => x.Date < DateTime.Now);
                    foreach (EventDate remove in EventDatesInCalendarToRemove)
                    {
                        calendar.EventDates.Remove(remove);
                    }
                }
                _context.SaveChanges();
            }
            
        }
        private void AddEventDatesOnCalendar(int id)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var calendar = _context.Events.Include(x => x.EventDateTemplates).Include(x => x.EventDates).ThenInclude(x => x.Teilnahmen).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == id).FirstOrDefault();
                if(calendar != null)
                {
                    // Next 2 weeks
                    for (int week = 0; week < 2; week++)
                    {
                        foreach (EventDateTemplate template in calendar.EventDateTemplates)
                        {
                            EventDate eventdate = template.CreateEventDate(week);
                            eventdate.EventDateTemplateID = template.ID;
                            if (calendar.EventDates.AsQueryable().Where(x => x.Date == eventdate.Date).Count() == 0)
                            {
                                eventdate.Teilnahmen = calendar.GenerateAppointmentRequests(eventdate);
                                _context.EventDates.Add(eventdate);
                                calendar.EventDates.Add(eventdate);
                            }
                        }
                    }
                    _context.SaveChanges();
                }
            }
        }
        private void UpdateEventDates(int id)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var calendar = _context.Events.Include(x => x.EventDateTemplates).Include(x => x.EventDates).ThenInclude(x => x.Teilnahmen).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == id).FirstOrDefault();
                if(calendar != null)
                {
                    foreach (EventDate update in calendar.EventDates)
                    {
                        var template = _context.EventDateTemplates.AsQueryable().Where(x => x.ID == update.EventDateTemplateID).FirstOrDefault();
                        if (template != null)
                        {
                            update.StartTime = template.StartTime;
                            update.StopTime = template.StopTime;
                            update.Date = DateTime.ParseExact(update.Date.ToString("dd-MM-yyyy") + " " + update.StartTime.ToString("HH:mm"), "dd-MM-yyyy HH:mm",
                                            System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                    _context.SaveChanges();
                }
            } 
        }
    }
}

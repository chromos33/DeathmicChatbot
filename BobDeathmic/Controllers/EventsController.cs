using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Data;
using BobDeathmic.Models;
using BobDeathmic.Models.Events;
using BobDeathmic.Models.Events.ManyMany;
using BobDeathmic.ReactDataClasses.Events.OverView;
using BobDeathmic.ReactDataClasses.Events.Vote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BobDeathmic.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private UserManager<ChatUserModel> _userManager;
        public EventsController(ApplicationDbContext context, IConfiguration configuration, UserManager<ChatUserModel> userManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> OverViewData()
        {
            OverView data = new OverView();
            data.AddCalendarLink = this.Url.Action("CreateEvent");
            ChatUserModel user = await _userManager.GetUserAsync(this.User);
            data.Calendars = getRelevantCalendars(user);
            return Json(data);
        }

        private List<ReactDataClasses.Events.OverView.Calendar> getRelevantCalendars(ChatUserModel user)
        {
            
            List<ReactDataClasses.Events.OverView.Calendar> Calendars = new List<ReactDataClasses.Events.OverView.Calendar>();
            foreach(Models.Events.Event calendar in _context.Events.Include(x => x.Admin).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Admin.Id == user.Id || x.Members.Where(y => y.ChatUserModelID == user.Id).Count() > 0))
            {
                //TODO make a custom CalendarMember object to facilitate better security (no need to lay open all data)
                if(calendar.Admin == user)
                {
                    Calendars.Add(new ReactDataClasses.Events.OverView.Calendar { Id = calendar.Id, key = calendar.Id, DeleteLink = this.Url.Action("Delete", "Events", new { ID = calendar.Id }), EditLink = this.Url.Action("EditCalendar", "Events", new { ID = calendar.Id }), Name = calendar.Name, VoteLink = this.Url.Action("VoteOnCalendar", "Events", new { ID = calendar.Id }) });
                }
                else
                {
                    Calendars.Add(new ReactDataClasses.Events.OverView.Calendar { Id = calendar.Id, key = calendar.Id, DeleteLink = "", EditLink = "", Name = calendar.Name, VoteLink = this.Url.Action("VoteOnCalendar", "Events", new { ID = calendar.Id }) });
                }
            }
            return Calendars;
        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task UpdateEventTitle(string ID,string Title)
        {
            if (Int32.TryParse(ID, out int _ID))
            {
                var Calendar = _context.Events.Where(x => x.Id == _ID).FirstOrDefault();
                if (Calendar != null)
                {
                    Calendar.Name = Title;
                    await _context.SaveChangesAsync();
                }
            }
            
            
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calendar = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calendar == null)
            {
                return NotFound();
            }

            return View(calendar);
        }

        // POST: Streams2/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stream = await _context.Events.FindAsync(id);
            _context.Events.Remove(stream);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task AddInvitedUser(string ID, string ChatUser)
        {
            if (Int32.TryParse(ID, out int _ID))
            {
                var Calendar = _context.Events.Where(x => x.Id == _ID).FirstOrDefault();
                if (Calendar != null)
                {
                    if(Calendar.Members.Where(x => x.ChatUserModel.ChatUserName == ChatUser).Count() == 0)
                    {
                        ChatUserModel user = _context.ChatUserModels.Where(x => x.ChatUserName.ToLower() == ChatUser.ToLower()).FirstOrDefault();
                        if(user != null)
                        {
                            ChatUserModel_Event newrelation = new ChatUserModel_Event();
                            newrelation.Calendar = Calendar;
                            newrelation.ChatUserModel = user;
                            if(Calendar.Members == null)
                            {
                                Calendar.Members = new List<ChatUserModel_Event>();
                            }
                            Calendar.Members.Add(newrelation);
                            if(user.Calendars == null)
                            {
                                user.Calendars = new List<ChatUserModel_Event>();
                            }
                            user.Calendars.Add(newrelation);
                            _context.ChatUserModel_Event.Add(newrelation);
                            await _context.SaveChangesAsync();
                        }
                    }
                    
                }
            }
        }
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task UpdateRequestState(string requestID, string state)
        {
            var Request = _context.AppointmentRequests.Where(x => x.ID == requestID).FirstOrDefault();
            if(Request != null)
            {
                Request.State = (AppointmentRequestState) Enum.Parse(typeof(AppointmentRequestState), state);
                _context.SaveChanges();
            }

        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task RemoveInvitedUser(string ID, string ChatUser)
        {
            if (Int32.TryParse(ID, out int _ID))
            {
                var RelationToDelete = _context.ChatUserModel_Event.Include(x => x.ChatUserModel).Where(x => x.CalendarID == _ID && x.ChatUserModel.ChatUserName == ChatUser).FirstOrDefault();
                if (RelationToDelete != null)
                {
                    _context.ChatUserModel_Event.Remove(RelationToDelete);
                    _context.SaveChanges();
                }
            }
        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> InvitableUsers(int ID)
        {
            Models.Events.Event _calendar = _context.Events.Include(x => x.Members).Where(x => x.Id == ID).FirstOrDefault();
            if (_calendar != null)
            {
                //TODO get real Filtered Members
                List<ChatUserModel_Event> relation = _context.ChatUserModel_Event.Include(x=>x.ChatUserModel).Where(x => x.Calendar == _calendar).ToList();
                List<ChatUserModel> ToFilterMembers = new List<ChatUserModel>();
                foreach(ChatUserModel_Event temp in relation)
                {
                    ToFilterMembers.Add(temp.ChatUserModel);
                }
                List<ChatUserModel> FilteredMembers = _context.ChatUserModels.Where(x => !ToFilterMembers.Contains(x)).ToList();
                List<ChatUser> Members = new List<ChatUser>();
                foreach (ChatUserModel Member in FilteredMembers)
                {
                    Members.Add(new ChatUser { Name = Member.ChatUserName });
                }
                return Json(Members.ToArray());
            }
            return null;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> GetEventDates(int ID)
        {
            List<EventDate> EventDates = _context.EventDates.Include(x => x.Event).Include(x => x.Teilnahmen).ThenInclude(x => x.Owner).Where(x => x.Event.Id == ID).OrderBy(x => x.Date).ThenBy(x => x.StartTime).Take(6).ToList();
            if (EventDates.Count() >0)
            {
                VoteReactData ReactData = new VoteReactData();
                ReactData.Header = new List<EventDateHeader>();
                ReactData.User = new List<VoteChatUser>();
                ChatUserModel user = await _userManager.GetUserAsync(this.User);
                foreach (EventDate EventDate in EventDates)
                {
                    if(ReactData.Header.Where(x => x.Date == EventDate.Date.ToString("dd.MM.yy") && x.Time == EventDate.StartTime.ToString("HH:mm") + " - " + EventDate.StopTime.ToString("HH:mm")).Count() == 0)
                    {
                        ReactData.Header.Add(new EventDateHeader { Date = EventDate.Date.ToString("dd.MM.yy"), Time = EventDate.StartTime.ToString("HH:mm") + " - " + EventDate.StopTime.ToString("HH:mm") });
                        foreach (AppointmentRequest request in EventDate.Teilnahmen.OrderBy(x => x.EventDate.Date).ThenBy(x => x.EventDate.StartTime))
                        {
                            if (ReactData.User.Where(x => x.Name.ToLower() == request.Owner.ChatUserName.ToLower()).Count() == 0)
                            {
                                var userdata = new VoteChatUser { key = request.Owner.ChatUserName, Name = request.Owner.ChatUserName };
                                userdata.canEdit = request.Owner == user;
                                userdata.Requests = new List<VoteRequest>();
                                VoteRequest tmp = new VoteRequest();
                                tmp.AppointmentRequestID = request.ID;
                                tmp.UserName = request.Owner.ChatUserName;
                                tmp.State = request.State;
                                tmp.Date = request.EventDate.Date;
                                tmp.Time = request.EventDate.StartTime;
                                userdata.Requests.Add(tmp);
                                ReactData.User.Add(userdata);

                            }
                            else
                            {
                                var userdata = ReactData.User.Where(x => x.Name.ToLower() == request.Owner.ChatUserName.ToLower()).FirstOrDefault();
                                VoteRequest tmp = new VoteRequest();
                                tmp.AppointmentRequestID = request.ID;
                                tmp.UserName = request.Owner.ChatUserName;
                                tmp.State = request.State;
                                tmp.Date = request.EventDate.Date;
                                tmp.Time = request.EventDate.StartTime;
                                userdata.Requests.Add(tmp);
                            }
                        }
                    }
                }
                var json = Json(ReactData, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
                return json;
            }
            return new JsonResult("");

        }
        
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> InvitedUsers(int ID)
        {
            Models.Events.Event _calendar = _context.Events.Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == ID).FirstOrDefault();
            if (_calendar != null)
            {
                ChatUser[] Members = new ChatUser[_calendar.Members.Count()];
                for (int i = 0; i < _calendar.Members.Count(); i++)
                {
                    Members[i] = new ChatUser { Name = _calendar.Members[i].ChatUserModel.ChatUserName, key=i };
                }
                return Json(Members);
            }
            return null;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> Templates(int ID)
        {
            Models.Events.Event _calendar = _context.Events.Include(x => x.EventDateTemplates).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == ID).FirstOrDefault();
            if (_calendar != null)
            {
                return Json(_calendar.EventDateTemplates.Select(x => new {key = x.ID, Day = x.Day,Start = x.StartTime.ToString("HH:mm"),Stop = x.StopTime.ToString("HH:mm") }), new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore});
            }
            return null;

        }
        
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<bool> SetDayOfTemplate(string ID, int Day)
        {
            EventDateTemplate template = _context.EventDateTemplates.Where(x => x.ID == ID).FirstOrDefault();
            if(template != null)
            {
                template.Day = (Models.Events.Day)Enum.ToObject(typeof(Models.Events.Day), Day);
                _context.SaveChanges();
                return true;
            }
            return false;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<bool> SetStartOfTemplate(string ID, string Start)
        {
            EventDateTemplate template = _context.EventDateTemplates.Where(x => x.ID == ID).FirstOrDefault();
            if (template != null)
            {
                template.StartTime = DateTime.ParseExact(Start,"HH:mm",System.Globalization.CultureInfo.InvariantCulture);
                _context.SaveChanges();
                return true;
            }
            return false;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<bool> SetStopOfTemplate(string ID, string Stop)
        {
            EventDateTemplate template = _context.EventDateTemplates.Where(x => x.ID == ID).FirstOrDefault();
            if (template != null)
            {
                template.StopTime = DateTime.ParseExact(Stop, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                _context.SaveChanges();
                return true;
            }
            return false;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> AddTemplate(int ID)
        {
            Models.Events.Event _calendar = _context.Events.Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == ID).FirstOrDefault();
            if (_calendar != null)
            {
                EventDateTemplate item = new EventDateTemplate();
                item.Event = _calendar;
                _calendar.EventDateTemplates.Add(item);
                _context.EventDateTemplates.Add(item);
                _context.SaveChanges();
                return Json(new { key =item.ID, Day = item.Day, Start = item.StartTime, Stop = item.StopTime }, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
            }
            return null;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public void RemoveTemplate(string ID,int CalendarID)
        {
            var calendar = _context.Events.Where(x => x.Id == CalendarID).FirstOrDefault();
            var template = _context.EventDateTemplates.Where(x => x.ID == ID).FirstOrDefault();
            if(calendar != null && template != null)
            {
                calendar.EventDateTemplates.Remove(template);
                _context.Remove(template);
                _context.SaveChanges();
            }
        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetEvent(int ID)
        {
            Models.Events.Event _calendar = _context.Events.Where(x => x.Id == ID).FirstOrDefault();
            if(_calendar != null)
            {
                if(_calendar.Name == null)
                {
                    _calendar.Name = "";
                    _context.SaveChangesAsync();
                }
                return Json(_calendar);
            }
            return RedirectToAction("OverViewData");
            
        }

        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> CreateEvent(int? ID)
        {
            if (ID == null)
            {
                ChatUserModel admin = await _userManager.GetUserAsync(this.User);
                Models.Events.Event newCalendar = new Models.Events.Event();
                if (admin.AdministratedCalendars == null)
                {
                    admin.AdministratedCalendars = new List<Models.Events.Event>();
                }
                admin.AdministratedCalendars.Add(newCalendar);
                if (admin.Calendars == null)
                {
                    admin.Calendars = new List<Models.Events.ManyMany.ChatUserModel_Event>();
                }
                Models.Events.ManyMany.ChatUserModel_Event JoinTable = new Models.Events.ManyMany.ChatUserModel_Event();
                JoinTable.Calendar = newCalendar;
                JoinTable.ChatUserModel = admin;
                admin.Calendars.Add(JoinTable);
                newCalendar.Members.Add(JoinTable);
                _context.Events.Add(newCalendar);
                _context.ChatUserModel_Event.Add(JoinTable);
                _context.SaveChanges();
                return View(newCalendar.Id);
            }
            //Create Calendar here stuff
            return View(ID);
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> EditCalendar(int ID)
        {
            return RedirectToAction("CreateEvent", new { ID = ID});
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> VoteOnCalendar(int ID)
        {
            return View(ID);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Data;
using BobDeathmic.Models;
using BobDeathmic.Models.EventDateFinder;
using BobDeathmic.Models.EventDateFinder.ManyMany;
using BobDeathmic.ReactDataClasses.EventDateFinder.OverView;
using BobDeathmic.ReactDataClasses.EventDateFinder.Vote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BobDeathmic.Controllers
{
    public class EventDateFinderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private UserManager<ChatUserModel> _userManager;
        public EventDateFinderController(ApplicationDbContext context, IConfiguration configuration, UserManager<ChatUserModel> userManager)
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
            Console.WriteLine("OverViewData");
            OverView data = new OverView();
            data.AddCalendarLink = this.Url.Action("CreateCalendar");
            ChatUserModel user = await _userManager.GetUserAsync(this.User);
            data.Calendars = getRelevantCalendars(user);
            return Json(data);
        }

        private List<ReactDataClasses.EventDateFinder.OverView.Calendar> getRelevantCalendars(ChatUserModel user)
        {
            
            List<ReactDataClasses.EventDateFinder.OverView.Calendar> Calendars = new List<ReactDataClasses.EventDateFinder.OverView.Calendar>();
            foreach(Models.EventDateFinder.Calendar calendar in _context.EventCalendar.Include(x => x.Admin).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Admin.Id == user.Id || x.isMember(user)))
            {
                //TODO make a custom CalendarMember object to facilitate better security (no need to lay open all data)
                Calendars.Add(new ReactDataClasses.EventDateFinder.OverView.Calendar { Id = calendar.Id,key = calendar.Id, EditLink = this.Url.Action("EditCalendar", "EventDateFinder", new { ID = calendar.Id }), Name = calendar.Name,VoteLink = this.Url.Action("VoteOnCalendar", "EventDateFinder", new { ID = calendar.Id})});
                
            }
            return Calendars;
        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task UpdateCalendarTitle(string ID,string Title)
        {
            Console.WriteLine("UpdateCalendarTitle");
            if (Int32.TryParse(ID, out int _ID))
            {
                var Calendar = _context.EventCalendar.Where(x => x.Id == _ID).FirstOrDefault();
                if (Calendar != null)
                {
                    Calendar.Name = Title;
                    await _context.SaveChangesAsync();
                }
            }
            
            
        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task AddInvitedUser(string ID, string ChatUser)
        {
            Console.WriteLine("AddInvitedUser");
            if (Int32.TryParse(ID, out int _ID))
            {
                var Calendar = _context.EventCalendar.Where(x => x.Id == _ID).FirstOrDefault();
                if (Calendar != null)
                {
                    if(Calendar.Members.Where(x => x.ChatUserModel.ChatUserName == ChatUser).Count() == 0)
                    {
                        ChatUserModel user = _context.ChatUserModels.Where(x => x.ChatUserName.ToLower() == ChatUser.ToLower()).FirstOrDefault();
                        if(user != null)
                        {
                            ChatUserModel_Calendar newrelation = new ChatUserModel_Calendar();
                            newrelation.Calendar = Calendar;
                            newrelation.ChatUserModel = user;
                            if(Calendar.Members == null)
                            {
                                Calendar.Members = new List<ChatUserModel_Calendar>();
                            }
                            Calendar.Members.Add(newrelation);
                            if(user.Calendars == null)
                            {
                                user.Calendars = new List<ChatUserModel_Calendar>();
                            }
                            user.Calendars.Add(newrelation);
                            _context.ChatUserModel_Calendar.Add(newrelation);
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
            Console.WriteLine("UpdateRequestState");
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
            Console.WriteLine("RemoveInvitedUser");
            if (Int32.TryParse(ID, out int _ID))
            {
                var RelationToDelete = _context.ChatUserModel_Calendar.Include(x => x.ChatUserModel).Where(x => x.CalendarID == _ID && x.ChatUserModel.ChatUserName == ChatUser).FirstOrDefault();
                if (RelationToDelete != null)
                {
                    _context.ChatUserModel_Calendar.Remove(RelationToDelete);
                    _context.SaveChanges();
                }
            }
        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> InvitableUsers(int ID)
        {
            Console.WriteLine("InvitableUsers");
            Models.EventDateFinder.Calendar _calendar = _context.EventCalendar.Include(x => x.Members).Where(x => x.Id == ID).FirstOrDefault();
            if (_calendar != null)
            {
                //TODO get real Filtered Members
                List<ChatUserModel_Calendar> relation = _context.ChatUserModel_Calendar.Include(x=>x.ChatUserModel).Where(x => x.Calendar == _calendar).ToList();
                List<ChatUserModel> ToFilterMembers = new List<ChatUserModel>();
                foreach(ChatUserModel_Calendar temp in relation)
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
            Console.WriteLine("GetEventDates");
            List<EventDate> EventDates = _context.EventDates.Include(x => x.Calendar).Include(x => x.Teilnahmen).ThenInclude(x => x.Owner).Where(x => x.CalendarId == ID).OrderBy(x => x.Date).ThenBy(x => x.StartTime).Take(6).ToList();
            Console.WriteLine("GetEventDates Step 1");
            if (EventDates.Count() >0)
            {
                VoteReactData ReactData = new VoteReactData();
                ReactData.Header = new List<EventDateHeader>();
                ReactData.User = new List<VoteChatUser>();
                ChatUserModel user = await _userManager.GetUserAsync(this.User);
                Console.WriteLine("GetEventDates Step 2");
                foreach (EventDate EventDate in EventDates)
                {
                    if(ReactData.Header.Where(x => x.Date == EventDate.Date.ToString("dd.MM.yy") && x.Time == EventDate.StartTime.ToString("HH:mm") + " - " + EventDate.StopTime.ToString("HH:mm")).Count() == 0)
                    {
                        Console.WriteLine("GetEventDates Step 2.1");
                        ReactData.Header.Add(new EventDateHeader { Date = EventDate.Date.ToString("dd.MM.yy"), Time = EventDate.StartTime.ToString("HH:mm") + " - " + EventDate.StopTime.ToString("HH:mm") });
                        Console.WriteLine("GetEventDates Step 2.2");
                        foreach (AppointmentRequest request in EventDate.Teilnahmen.OrderBy(x => x.EventDate.Date).ThenBy(x => x.EventDate.StartTime))
                        {
                            Console.WriteLine("GetEventDates Step 2.3");
                            if (ReactData.User.Where(x => x.Name.ToLower() == request.Owner.ChatUserName.ToLower()).Count() == 0)
                            {
                                Console.WriteLine("GetEventDates Step 2.4.1");
                                var userdata = new VoteChatUser { key = request.Owner.ChatUserName, Name = request.Owner.ChatUserName };
                                Console.WriteLine("GetEventDates Step 2.4.1.1");
                                userdata.canEdit = request.Owner == user;
                                userdata.Requests = new List<VoteRequest>();
                                Console.WriteLine("GetEventDates Step 2.4.1.2");
                                VoteRequest tmp = new VoteRequest();
                                tmp.AppointmentRequestID = request.ID;
                                tmp.UserName = request.Owner.ChatUserName;
                                tmp.State = request.State;
                                tmp.Date = request.EventDate.Date;
                                tmp.Time = request.EventDate.StartTime;
                                Console.WriteLine("GetEventDates Step 2.4.1.3");
                                userdata.Requests.Add(tmp);
                                Console.WriteLine("GetEventDates Step 2.4.1.4");
                                ReactData.User.Add(userdata);
                                Console.WriteLine("GetEventDates Step 2.4.1.5");

                            }
                            else
                            {
                                Console.WriteLine("GetEventDates Step 2.4.2");
                                var userdata = ReactData.User.Where(x => x.Name.ToLower() == request.Owner.ChatUserName.ToLower()).FirstOrDefault();
                                Console.WriteLine("GetEventDates Step 2.4.2.1");
                                VoteRequest tmp = new VoteRequest();
                                tmp.AppointmentRequestID = request.ID;
                                tmp.UserName = request.Owner.ChatUserName;
                                tmp.State = request.State;
                                tmp.Date = request.EventDate.Date;
                                tmp.Time = request.EventDate.StartTime;
                                Console.WriteLine("GetEventDates Step 2.4.2.2");
                                userdata.Requests.Add(tmp);
                                Console.WriteLine("GetEventDates Step 2.4.2.3");
                            }
                        }
                    }
                }
                Console.WriteLine("GetEventDates Step 3.1");
                var json = Json(ReactData, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
                Console.WriteLine("GetEventDates Step 3.2");
                return json;
            }
            return new JsonResult("");

        }
        
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> InvitedUsers(int ID)
        {
            Console.WriteLine("InvitedUsers");
            Models.EventDateFinder.Calendar _calendar = _context.EventCalendar.Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == ID).FirstOrDefault();
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
            Console.WriteLine("Templates");
            Models.EventDateFinder.Calendar _calendar = _context.EventCalendar.Include(x => x.EventDateTemplates).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == ID).FirstOrDefault();
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
            Console.WriteLine("SetDayOfTemplate");
            EventDateTemplate template = _context.EventDateTemplates.Where(x => x.ID == ID).FirstOrDefault();
            if(template != null)
            {
                template.Day = (Models.EventDateFinder.Day)Enum.ToObject(typeof(Models.EventDateFinder.Day), Day);
                _context.SaveChanges();
                return true;
            }
            return false;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<bool> SetStartOfTemplate(string ID, string Start)
        {
            Console.WriteLine("SetStartOfTemplate");
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
            Console.WriteLine("SetStopOfTemplate");
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
            Console.WriteLine("AddTemplate");
            Models.EventDateFinder.Calendar _calendar = _context.EventCalendar.Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == ID).FirstOrDefault();
            if (_calendar != null)
            {
                EventDateTemplate item = new EventDateTemplate();
                item.Calendar = _calendar;
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
            Console.WriteLine("RemoveTemplate");
            var calendar = _context.EventCalendar.Where(x => x.Id == CalendarID).FirstOrDefault();
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
        public async Task<IActionResult> GetCalendar(int ID)
        {
            Console.WriteLine("GetCalendar");
            Models.EventDateFinder.Calendar _calendar = _context.EventCalendar.Where(x => x.Id == ID).FirstOrDefault();
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
        public async Task<IActionResult> CreateCalendar(int? ID)
        {
            Console.WriteLine("CreateCalendar");
            if (ID == null)
            {
                ChatUserModel admin = await _userManager.GetUserAsync(this.User);
                Models.EventDateFinder.Calendar newCalendar = new Models.EventDateFinder.Calendar();
                if (admin.AdministratedCalendars == null)
                {
                    admin.AdministratedCalendars = new List<Models.EventDateFinder.Calendar>();
                }
                admin.AdministratedCalendars.Add(newCalendar);
                if (admin.Calendars == null)
                {
                    admin.Calendars = new List<Models.EventDateFinder.ManyMany.ChatUserModel_Calendar>();
                }
                Models.EventDateFinder.ManyMany.ChatUserModel_Calendar JoinTable = new Models.EventDateFinder.ManyMany.ChatUserModel_Calendar();
                JoinTable.Calendar = newCalendar;
                JoinTable.ChatUserModel = admin;
                admin.Calendars.Add(JoinTable);
                newCalendar.Members.Add(JoinTable);
                _context.EventCalendar.Add(newCalendar);
                _context.ChatUserModel_Calendar.Add(JoinTable);
                _context.SaveChanges();
                return View(newCalendar.Id);
            }
            //Create Calendar here stuff
            return View(ID);
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> EditCalendar(int ID)
        {
            Console.WriteLine("EditCalendar");
            //Edit Calendar here stuff
            return RedirectToAction("CreateCalendar",new { ID = ID});
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> VoteOnCalendar(int ID)
        {
            Console.WriteLine("VoteOnCalendar");
            //Create Calendar here stuff
            return View(ID);
        }
    }
}
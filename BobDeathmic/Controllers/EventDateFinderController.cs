using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Data;
using BobDeathmic.Models;
using BobDeathmic.Models.EventDateFinder;
using BobDeathmic.Models.EventDateFinder.ManyMany;
using BobDeathmic.ReactDataClasses.EventDateFinder.OverView;
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
                Calendars.Add(new ReactDataClasses.EventDateFinder.OverView.Calendar { Id = calendar.Id,key = calendar.Id, EditLink = this.Url.Action("EditCalendar", "EventDateFinder", new { ID = calendar.Id }), Name = calendar.Name,VoteLink = this.Url.Action("EventDateFinder", "VoteOnCalendar", new { ID = calendar.Id})});
                
            }
            return Calendars;
        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task UpdateCalendarTitle(string ID,string Title)
        {
            if(Int32.TryParse(ID, out int _ID))
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
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> InvitableUsers(int ID)
        {
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
                ChatUser[] Members = new ChatUser[FilteredMembers.Count()];
                for (int i = 0; i < FilteredMembers.Count(); i++)
                {
                    Members[i] = new ChatUser { Name = FilteredMembers[i].ChatUserName };
                }
                return Json(Members);
            }
            return null;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> InvitedUsers(int ID)
        {
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
            Models.EventDateFinder.Calendar _calendar = _context.EventCalendar.Include(x => x.AppointmentRequestTemplate).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == ID).FirstOrDefault();
            if (_calendar != null)
            {
                return Json(_calendar.AppointmentRequestTemplate.Select(x => new {key = x.ID, Day = x.Day,Start = x.StartTime.ToString("HH:mm"),Stop = x.StopTime.ToString("HH:mm") }), new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore});
            }
            return null;

        }
        
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<bool> SetDayOfTemplate(string ID, int Day)
        {
            EventDateTemplate template = _context.AppointmentRequestTemplates.Where(x => x.ID == ID).FirstOrDefault();
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
            EventDateTemplate template = _context.AppointmentRequestTemplates.Where(x => x.ID == ID).FirstOrDefault();
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
            EventDateTemplate template = _context.AppointmentRequestTemplates.Where(x => x.ID == ID).FirstOrDefault();
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
            Models.EventDateFinder.Calendar _calendar = _context.EventCalendar.Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Id == ID).FirstOrDefault();
            if (_calendar != null)
            {
                EventDateTemplate item = new EventDateTemplate();
                item.Calendar = _calendar;
                _calendar.AppointmentRequestTemplate.Add(item);
                _context.AppointmentRequestTemplates.Add(item);
                _context.SaveChanges();
                return Json(new { Day = item.Day, Start = item.StartTime, Stop = item.StopTime }, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
            }
            return null;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetCalendar(int ID)
        {
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
            if(ID == null)
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
            //Edit Calendar here stuff
            return RedirectToAction("CreateCalendar",new { ID = ID});
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> VoteOnCalendar(int ID)
        {
            //Create Calendar here stuff
            return View();
        }
    }
}
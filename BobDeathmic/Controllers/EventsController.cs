﻿using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.EventCalendar;
using BobDeathmic.Data.DBModels.EventCalendar.manymany;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Data.JSONModels;
using BobDeathmic.Models;
using BobDeathmic.Models.Events;
using BobDeathmic.ViewModels.ReactDataClasses.EventDateFinder.OverView;
using BobDeathmic.ViewModels.ReactDataClasses.EventDateFinder.Vote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private UserManager<ChatUserModel> _userManager;
        private IHostingEnvironment env;
        public EventsController(ApplicationDbContext context, IConfiguration configuration, UserManager<ChatUserModel> userManager, IHostingEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            this.env = env;
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public IActionResult Index()
        {
            return View("Index");
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
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public IActionResult DownloadAPK()
        {
            DirectoryInfo info = new DirectoryInfo(Path.Combine(new string[] { env.WebRootPath, "Downloads", "APK" }));
            FileInfo[] files = info.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();
            int i = 0;
            FileInfo DownloadFile = null;
            foreach (FileInfo file in files)
            {
                if(i == 0)
                {
                    DownloadFile = file;
                }
                i++;
                if(i > 3)
                {
                    //keep last 3 versions
                    file.Delete();
                }
            }
            if(DownloadFile == null)
            {
                return Index();
            }
            return PhysicalFile(DownloadFile.FullName, MimeTypes.GetMimeType(DownloadFile.FullName), Path.GetFileName(DownloadFile.FullName));
        }
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public String ISUpdateAvailable(int majorRevision, int minorRevision)
        {
            DirectoryInfo info = new DirectoryInfo(Path.Combine(new string[] { env.WebRootPath, "Downloads", "APK" }));
            FileInfo[] files = info.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();
            FileInfo DownloadFile = files.FirstOrDefault();
            int Major_Revision = 0;
            int Minor_Revision = 0;
            if (DownloadFile != null)
            {
                Tuple<int, int> VersionsFromFile = getVersionFromString(DownloadFile.FullName);
                if (VersionsFromFile.Item1 > majorRevision)
                {
                    return JsonConvert.SerializeObject(new MobileResponse() { Response = "true" });
                }
                if (VersionsFromFile.Item1 == majorRevision && VersionsFromFile.Item2 > minorRevision)
                {
                    return JsonConvert.SerializeObject(new MobileResponse() { Response = "true" });
                }
            }
            return JsonConvert.SerializeObject(new MobileResponse() { Response = "false" });
        }
        private Tuple<int,int> getVersionFromString(string version)
        {
            try
            {
                string[] versions = version.Split("__")[1].Split(".")[0].Split("_");

                return new Tuple<int, int>(int.Parse(versions[0]), int.Parse(versions[1]));
            }
            catch( ArgumentNullException ex)
            {
                return new Tuple<int, int>(0,0);
            }
            catch (FormatException ex)
            {
                return new Tuple<int, int>(0, 0);
            }
            catch (OverflowException ex)
            {
                return new Tuple<int, int>(0, 0);
            }

        }

        private List<Calendar> getRelevantCalendars(ChatUserModel user)
        {

            List<Calendar> Calendars = new List<Calendar>();
            foreach (Models.Events.Event calendar in _context.Events.Include(x => x.Admin).Include(x => x.Members).ThenInclude(x => x.ChatUserModel).Where(x => x.Admin.Id == user.Id || x.Members.Where(y => y.ChatUserModelID == user.Id).Count() > 0))
            {
                //TODO make a custom CalendarMember object to facilitate better security (no need to lay open all data)
                if (calendar.Admin == user)
                {
                    Calendars.Add(new Calendar { Id = calendar.Id, key = calendar.Id, DeleteLink = this.Url.Action("Delete", "Events", new { ID = calendar.Id }), EditLink = this.Url.Action("EditCalendar", "Events", new { ID = calendar.Id }), Name = calendar.Name, VoteLink = this.Url.Action("VoteOnCalendar", "Events", new { ID = calendar.Id }) });
                }
                else
                {
                    Calendars.Add(new Calendar { Id = calendar.Id, key = calendar.Id, DeleteLink = "", EditLink = "", Name = calendar.Name, VoteLink = this.Url.Action("VoteOnCalendar", "Events", new { ID = calendar.Id }) });
                }
            }
            return Calendars;
        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task UpdateEventTitle(string ID, string Title)
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
                    if (Calendar.Members.Where(x => x.ChatUserModel.ChatUserName == ChatUser).Count() == 0)
                    {
                        ChatUserModel user = _context.ChatUserModels.Where(x => x.ChatUserName.ToLower() == ChatUser.ToLower()).FirstOrDefault();
                        if (user != null)
                        {
                            ChatUserModel_Event newrelation = new ChatUserModel_Event();
                            newrelation.Calendar = Calendar;
                            newrelation.ChatUserModel = user;
                            if (Calendar.Members == null)
                            {
                                Calendar.Members = new List<ChatUserModel_Event>();
                            }
                            Calendar.Members.Add(newrelation);
                            if (user.Calendars == null)
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
        public async Task<int> UpdateRequestState(string requestID, string state,string comment)
        {
            var Request = _context.AppointmentRequests.Where(x => x.ID == requestID).FirstOrDefault();
            if (Request != null)
            {
                Request.State = (AppointmentRequestState)Enum.Parse(typeof(AppointmentRequestState), state);
                Request.Comment = comment;
                return _context.SaveChanges();
            }
            return 0;
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
                List<ChatUserModel_Event> relation = _context.ChatUserModel_Event.Include(x => x.ChatUserModel).Where(x => x.Calendar == _calendar).ToList();
                List<ChatUserModel> ToFilterMembers = new List<ChatUserModel>();
                foreach (ChatUserModel_Event temp in relation)
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
            List<EventDate> EventDates = _context.EventDates.Include(x => x.Event).Include(x => x.Teilnahmen).ThenInclude(x => x.Owner).Where(x => x.Event.Id == ID).OrderBy(x => x.Date).ThenBy(x => x.StartTime).ToList();
            if (EventDates.Count() > 0)
            {
                VoteReactData ReactData = new VoteReactData();
                ReactData.Header = new List<EventDateData>();
                ChatUserModel user = await _userManager.GetUserAsync(this.User);
                foreach (EventDate EventDate in EventDates)
                {
                    if (ReactData.Header.Where(x => x.Date == EventDate.Date.ToString("dd.MM.yy") && x.Time == EventDate.StartTime.ToString("HH:mm") + " - " + EventDate.StopTime.ToString("HH:mm")).Count() == 0)
                    {
                        EventDateData newData = new EventDateData { Requests = new List<VoteRequest>(), Date = EventDate.Date.ToString("dd.MM.yy"), Time = EventDate.StartTime.ToString("HH:mm") + " - " + EventDate.StopTime.ToString("HH:mm") };
                        //
                        foreach (AppointmentRequest request in EventDate.Teilnahmen.OrderBy(x => x.EventDate.Date).ThenBy(x => x.EventDate.StartTime).ThenBy(x => x.Owner.ChatUserName))
                        {
                            VoteRequest tmp = new VoteRequest();
                            tmp.AppointmentRequestID = request.ID;
                            tmp.UserName = request.Owner.ChatUserName;
                            tmp.State = request.State;
                            tmp.Date = request.EventDate.Date.ToString("dd.MM.yy");
                            tmp.Time = request.EventDate.StartTime.ToString("HH:mm") + "-" + request.EventDate.StopTime.ToString("HH:mm");
                            tmp.canEdit = request.Owner == user;
                            tmp.Comment = request.Comment;
                            newData.Requests.Add(tmp);
                        }
                        ReactData.Header.Add(newData);
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
                    Members[i] = new ChatUser { Name = _calendar.Members[i].ChatUserModel.ChatUserName, key = i };
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
                return Json(_calendar.EventDateTemplates.Select(x => new { key = x.ID, Day = x.Day, Start = x.StartTime.ToString("HH:mm"), Stop = x.StopTime.ToString("HH:mm") }), new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
            }
            return null;

        }

        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<bool> SetDayOfTemplate(string ID, int Day)
        {
            EventDateTemplate template = _context.EventDateTemplates.Where(x => x.ID == ID).FirstOrDefault();
            if (template != null)
            {
                template.Day = (Day)Enum.ToObject(typeof(Day), Day);
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
                template.StartTime = DateTime.ParseExact(Start, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
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
                return Json(new { key = item.ID, Day = item.Day, Start = item.StartTime, Stop = item.StopTime }, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
            }
            return null;

        }
        [Authorize(Roles = "User,Dev,Admin")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public void RemoveTemplate(string ID, int CalendarID)
        {
            var calendar = _context.Events.Where(x => x.Id == CalendarID).FirstOrDefault();
            var template = _context.EventDateTemplates.Where(x => x.ID == ID).FirstOrDefault();
            if (calendar != null && template != null)
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
            if (_calendar != null)
            {
                if (_calendar.Name == null)
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
                    admin.Calendars = new List<ChatUserModel_Event>();
                }
                ChatUserModel_Event JoinTable = new ChatUserModel_Event();
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
            return RedirectToAction("CreateEvent", new { ID = ID });
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> VoteOnCalendar(int ID)
        {
            return View(ID);
        }
    }
}
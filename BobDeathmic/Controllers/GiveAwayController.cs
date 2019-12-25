using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.GiveAway;
using BobDeathmic.Data.DBModels.GiveAway.manymany;
using BobDeathmic.Data.DBModels.Relay;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.ViewModels.GiveAway;
using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Controllers
{
    public class GiveAwayController : Controller
    {

        private readonly Data.ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ChatUserModel> _manager;
        private readonly IEventBus _eventBus;
        private IMemoryCache _cache;
        private Random random;

        [TempData]
        public string StatusMessage { get; set; }

        public GiveAwayController(IServiceScopeFactory scopeFactory, Data.ApplicationDbContext context, IServiceProvider serviceProvider, IConfiguration configuration, UserManager<ChatUserModel> manager, IEventBus eventBus, IMemoryCache memoryCache)

        {
            _context = context;
            _serviceProvider = serviceProvider;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _manager = manager;
            _eventBus = eventBus;
            random = new Random();
            _cache = memoryCache;
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Index()
        {
            var user = await _manager.GetUserAsync(HttpContext.User);
            user = _context.ChatUserModels.Where(x => x.Id == user.Id).Include(x => x.OwnedItems).FirstOrDefault();
            return View("GiveAwayList", user);
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Winnings()
        {
            var user = await _manager.GetUserAsync(HttpContext.User);
            user = _context.ChatUserModels.Where(x => x.Id == user.Id).Include(x => x.ReceivedItems).ThenInclude(x => x.Owner).FirstOrDefault();
            return View("Winnings", user);
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Create()
        {

            return View();
        }

        // POST: Streams2/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Create([Bind("GiveAwayItemId,Title,Key,SteamID,Link,Views,Owner,Receiver")] GiveAwayItem item)
        {
            if (ModelState.IsValid)
            {
                //TODO: Attribute Verification aka if links are links and etc
                var user = await _manager.GetUserAsync(HttpContext.User);
                user = _context.ChatUserModels.Where(x => x.Id == user.Id).Include(x => x.OwnedItems).FirstOrDefault();
                item.Owner = user;
                _context.Add(item);
                await _context.SaveChangesAsync();
            }
            else
            {
                return View(item);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet()]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _manager.GetUserAsync(HttpContext.User);
            user = _context.ChatUserModels.Where(x => x.Id == user.Id).Include(x => x.OwnedItems).FirstOrDefault();
            var item = _context.GiveAwayItems.Where(x => x.Id == id.ToString() && x.Owner == user).FirstOrDefault();
            if (item != null)
            {
                return View(item);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Streams2/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Edit([Bind("Id,Title,Key,SteamID,Link,Views,Owner,Receiver")] GiveAwayItem item)
        {
            if (ModelState.IsValid)
            {
                var storedItem = _context.GiveAwayItems.Where(x => x.Id == item.Id).FirstOrDefault();
                if(storedItem != null)
                {
                    storedItem.Title = item.Title;
                    storedItem.Key = item.Key;
                    storedItem.SteamID = item.SteamID;
                    storedItem.Link = item.Link;
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                return View(item);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stream = await _context.GiveAwayItems
                .FirstOrDefaultAsync(m => m.Id == id.ToString());
            if (stream == null)
            {
                return NotFound();
            }

            return View(stream);
        }

        // POST: Streams2/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var giveawayitem = await _context.GiveAwayItems.FindAsync(id);
            _context.GiveAwayItems.Remove(giveawayitem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Dev")]
        public async Task<IActionResult> Admin()
        {
            return View();
        }
        private List<string> getApplicants(GiveAwayItem item)
        {
            List<string> result = new List<string>();
            foreach (var applicant in item.Applicants)
            {
                result.Add(_context.ChatUserModels.Where(u => u.Id == applicant.UserID).FirstOrDefault().ChatUserName);
            }
            return result;
        }
        private async Task SetNextGiveAwayItem()
        {
            await ResetCurrentItem();
            var GiveAwayItems = _context.GiveAwayItems.MinBy(g => g.Views).Where(x => !x.current && x.ReceiverID == null);
            if (GiveAwayItems.Count() > 0)
            {
                var item = GiveAwayItems.ElementAt(random.Next(0, GiveAwayItems.Count() - 1));
                item.current = true;
                item.Views++;
                _context.SaveChanges();
            }
        }
        private async Task ResetCurrentItem()
        {
            var item = _context.GiveAwayItems.Where(x => x.current).FirstOrDefault();
            if (item != null)
            {
                item.current = false;
                await _context.SaveChangesAsync();
            }
        }
        private GiveAwayItem GetCurrentGiveAwayItem()
        {
            var GiveAwayItems = _context.GiveAwayItems.Where(x => x.current).Include(x => x.Applicants);
            if (GiveAwayItems.Count() > 0)
            {
                return GiveAwayItems.FirstOrDefault();
            }
            return null;
        }
        private List<String> getChatChannels()
        {
            List<String> Channels = new List<string>();
            var val = "";
            _cache.TryGetValue("Channel", out val);
            if (val == "")
            {
                _cache.Set("Channel", _context.RelayChannels.FirstOrDefault().Name);
            }
            foreach (RelayChannels channel in _context.RelayChannels)
            {
                if (channel.Name == val && Channels.Count() > 0)
                {
                    var temp = Channels[0];
                    Channels[0] = channel.Name;
                    Channels.Add(temp);
                }
                else
                {
                    Channels.Add(channel.Name);
                }
            }
            return Channels;
        }
        [HttpGet, ActionName("InitialAdminData")]
        [Authorize(Roles = "Admin,Dev")]
        public async Task<string> InitialAdminData()
        {
            GiveAwayAdminViewModel model = new GiveAwayAdminViewModel();
            if (GetCurrentGiveAwayItem() == null)
            {
                await SetNextGiveAwayItem();
            }
            model.Item = GetCurrentGiveAwayItem().Title;
            model.Link = GetCurrentGiveAwayItem().Link;
            model.Channels = getChatChannels();
            if (model.Item != null)
            {
                model.Applicants = getApplicants(GetCurrentGiveAwayItem());
            }
            return JsonConvert.SerializeObject(model);
        }
        [HttpGet, ActionName("NextItem")]
        [Authorize(Roles = "Admin,Dev")]
        public async Task<string> NextItem(string channel)
        {
            //Do Stuff
            await SetNextGiveAwayItem();
            _cache.Set("Channel", channel);
            GiveAwayAdminViewModel model = new GiveAwayAdminViewModel();
            model.Item = GetCurrentGiveAwayItem().Title;
            model.Link = GetCurrentGiveAwayItem().Link;
            if (model.Item != null)
            {
                model.Applicants = getApplicants(GetCurrentGiveAwayItem());
            }
            if (_context.GiveAwayItems.Where(x => x.current).FirstOrDefault() != null)
            {
                _eventBus.TriggerEvent(EventType.CommandResponseReceived, new CommandResponseArgs { Channel = channel, MessageType = Eventbus.MessageType.ChannelMessage, Message = $"Zur Verlosung steht {_context.GiveAwayItems.Where(x => x.current).FirstOrDefault()?.Title} bitte mit !Gapply teilnehmen" });
            }
            return JsonConvert.SerializeObject(model);

        }

        [HttpGet, ActionName("Raffle")]
        [Authorize(Roles = "Admin,Dev")]
        public async Task<string> Raffle(string channel)
        {
            //Do Stuff
            List<string> winners = doRaffle(channel);
            return JsonConvert.SerializeObject(winners);
        }
        [HttpGet, ActionName("UpdateParticipantList")]
        [Authorize(Roles = "Admin,Dev")]
        public async Task<string> UpdateParticipantList()
        {
            return JsonConvert.SerializeObject(getApplicants(GetCurrentGiveAwayItem()));
        }
        private List<string> doRaffle(string channel)
        {
            List<string> winners = new List<string>();
            bool repeatRaffle = false;
            using (var scope = _scopeFactory.CreateScope())
            {
                var tmpcontext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var currentitem = tmpcontext.GiveAwayItems.Include(x => x.Applicants).ThenInclude(y => y.User).ThenInclude(x => x.ReceivedItems).Where(x => x.current).FirstOrDefault();
                List<ChatUserModel> Applicants = null;
                ChatUserModel tmpwinner = null;
                if (currentitem != null && currentitem.Applicants.Count() > 0)
                {
                    int min = getLeastGiveAwayCount(currentitem.Applicants);
                    var elligableUsers = currentitem.Applicants.Where(x => x.User.ReceivedItems.Count() == min);

                    int winnerindex = random.Next(elligableUsers.Count());
                    var winner = elligableUsers.ToArray()[winnerindex];
                    winners.Add(winner.User.ChatUserName);
                    currentitem.Receiver = winner.User;
                    tmpwinner = winner.User;
                    currentitem.ReceiverID = winner.UserID;
                    tmpcontext.SaveChanges();
                    _eventBus.TriggerEvent(EventType.CommandResponseReceived, new CommandResponseArgs { Channel = channel, MessageType = Eventbus.MessageType.ChannelMessage, Message = $"Gewonnen hat {winner.User.ChatUserName}" });
                }
                if (currentitem.Applicants.Count() > 1 && tmpcontext.GiveAwayItems.Include(x => x.Applicants).ThenInclude(y => y.User).Where(x => x.Title == currentitem.Title && x.Id != currentitem.Id && x.ReceiverID == null).Count() > 0)
                {
                    var newitem = tmpcontext.GiveAwayItems.Include(x => x.Applicants).ThenInclude(y => y.User).Where(x => x.Title == currentitem.Title && x.ReceiverID == null && x.current == false).FirstOrDefault();
                    foreach (var user in currentitem.Applicants.Where(x => x.UserID != tmpwinner.Id).ToList())
                    {
                        var m_n_relation = new User_GiveAwayItem(user.User, newitem);
                        newitem.Applicants.Add(m_n_relation);
                        user.User.AppliedTo.Add(m_n_relation);
                    }
                    currentitem.current = false;
                    newitem.current = true;
                    tmpcontext.SaveChanges();
                    repeatRaffle = true;
                }
            }
            if (repeatRaffle)
            {
                winners.AddRange(doRaffle(channel));
            }
            return winners;
        }
        private int getLeastGiveAwayCount(List<User_GiveAwayItem> items)
        {
            int value = 99999;
            foreach (var item in items)
            {
                if (value > item.User.ReceivedItems.Count())
                {
                    value = item.User.ReceivedItems.Count();
                }
            }
            if (value == 99999)
            {
                return 0;
            }
            return value;
        }

    }
}
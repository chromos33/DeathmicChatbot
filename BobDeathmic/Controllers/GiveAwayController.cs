using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Args;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Models.Discord;
using BobDeathmic.Models.GiveAwayModels;
using BobDeathmic.Models.ViewModels;
using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MoreLinq;

namespace BobDeathmic.Controllers
{
    public class GiveAwayController : Controller
    {

        private readonly Data.ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ChatUserModel> _manager;
        private readonly IEventBus _eventBus;
        private IMemoryCache _cache;
        private Random random;

        [TempData]
        public string StatusMessage { get; set; }

        public GiveAwayController(Data.ApplicationDbContext context, IServiceProvider serviceProvider, IConfiguration configuration, UserManager<ChatUserModel> manager, IEventBus eventBus, IMemoryCache memoryCache)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _manager = manager;
            _eventBus = eventBus;
            random = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / (DateTime.Now.Hour+1));
            _cache = memoryCache;
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Index()
        {
            var user = await _manager.GetUserAsync(HttpContext.User);
            user = _context.ChatUserModels.Where(x => x.Id == user.Id).Include( x => x.OwnedItems).FirstOrDefault();
            return View("GiveAwayList",user);
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Winnings()
        {
            var user = await _manager.GetUserAsync(HttpContext.User);
            user = _context.ChatUserModels.Where(x => x.Id == user.Id).Include(x => x.ReceivedItems).FirstOrDefault();
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
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _manager.GetUserAsync(HttpContext.User);
            user = _context.ChatUserModels.Where(x => x.Id == user.Id).Include(x => x.OwnedItems).FirstOrDefault();
            var item = _context.GiveAwayItems.Where(x => x.Id == id.ToString() && x.Owner == user).FirstOrDefault();
            if(item != null)
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
        public async Task<IActionResult> Edit([Bind("GiveAwayItemId,Title,Key,SteamID,Link,Views,Owner,Receiver")] GiveAwayItem item)
        {
            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
            }
            else
            {
                return View(item);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Delete(int? id)
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var giveawayitem = await _context.GiveAwayItems.FindAsync(id);
            _context.GiveAwayItems.Remove(giveawayitem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Dev")]
        public async Task<IActionResult> Admin()
        {
            GiveAwayAdminViewModel model = new GiveAwayAdminViewModel();
            model.Item = GetCurrentGiveAwayItem();
            model.Channels = getChatChannels();
            if (model.Item != null)
            {
                model.Applicants = getApplicants(model.Item);
            }
            return View(model);
        }
        private List<ChatUserModel> getApplicants(GiveAwayItem item)
        {
            List<ChatUserModel> result = new List<ChatUserModel>();
            foreach(var applicant in  item.Applicants)
            {
                result.Add(_context.ChatUserModels.Where(u => u.Id == applicant.UserID).FirstOrDefault());
            }
            return result;
        }
        private async Task SetNextGiveAwayItem()
        {
            await ResetCurrentItem();
            var GiveAwayItems = _context.GiveAwayItems.MinBy(g => g.Views).Where(x => !x.current && x.Receiver == null);
            if(GiveAwayItems.Count() > 0)
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
            if(item != null)
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
                if(channel.Name == val && Channels.Count() > 0)
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
        [Authorize(Roles = "Admin,Dev")]
        public async Task<IActionResult> NextItem(string channel)
        {
            //Do Stuff
            await SetNextGiveAwayItem();
            _cache.Set("Channel", channel);
            _eventBus.TriggerEvent(EventType.GiveAwayMessage,new GiveAwayEventArgs { channel = channel});
            return RedirectToAction(nameof(Admin));
        }
        [Authorize(Roles = "Admin,Dev")]
        public async Task<IActionResult> Raffle(string channel)
        {
            //Do Stuff
            var currentitem = _context.GiveAwayItems.Include(x => x.Applicants).ThenInclude(y => y.User).Where(x => x.current).FirstOrDefault();
            if(currentitem != null && currentitem.Applicants.Count() > 0)
            {
                int test = random.Next(currentitem.Applicants.Count());
                var winner = currentitem.Applicants[test];
                currentitem.Receiver = winner.User;
                currentitem.ReceiverID = winner.UserID;
                _context.SaveChanges();
                _eventBus.TriggerEvent(EventType.GiveAwayMessage, new GiveAwayEventArgs { winner = winner.User, channel = channel });
            }
            return RedirectToAction(nameof(Admin));
        }

    }
}
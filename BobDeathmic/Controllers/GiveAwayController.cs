using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Models;
using BobDeathmic.Models.GiveAwayModels;
using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BobDeathmic.Controllers
{
    public class GiveAwayController : Controller
    {

        private readonly Data.ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ChatUserModel> _manager;
        private Random random;

        [TempData]
        public string StatusMessage { get; set; }

        public GiveAwayController(Data.ApplicationDbContext context, IServiceProvider serviceProvider, IConfiguration configuration, UserManager<ChatUserModel> manager)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _manager = manager;
            random = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / DateTime.Now.Hour);
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Index()
        {
            var user = await _manager.GetUserAsync(HttpContext.User);
            user = _context.ChatUserModels.Where(x => x.Id == user.Id).Include( x => x.OwnedItems).FirstOrDefault();
            return View("GiveAwayList",user);
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
    }
}
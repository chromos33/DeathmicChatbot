using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Controllers
{
    public class CharacterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private UserManager<ChatUserModel> _userManager;
        private readonly IEventBus _eventBus;
        private Random random;
        public CharacterController(IEventBus eventBus, ApplicationDbContext context, IConfiguration configuration, UserManager<ChatUserModel> userManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _eventBus = eventBus;
            random = new Random();
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public IActionResult Index()
        {
            return View(Users());
        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> RollGold(string GM, int numdies, int die, int multiplikator)
        {
            int gold = Roll(numdies, die) * multiplikator;
            ChatUserModel user = await _userManager.GetUserAsync(this.User);
            if (GM != user.ChatUserName)
            {
                _eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, new MessageArgs { RecipientName = GM, Message = $"{user.ChatUserName} hat {gold} gold für den Charakter gewürfelt." });
            }
            _eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, new MessageArgs { RecipientName = user.ChatUserName, Message = $"Du hast {gold} gold für den Charakter gewürfelt." });
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> RollStats(string GM)
        {
            string stats = "";
            int sum = 0;
            for (int i = 0; i < 6; i++)
            {
                int result = Roll(4, 6, 1);
                sum += result;
                stats += result.ToString() + $" ({getBoni(result)})";
                if (i != 5)
                {
                    stats += ",";
                }
            }
            ChatUserModel user = await _userManager.GetUserAsync(this.User);
            if (GM != user.ChatUserName)
            {
                _eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, new MessageArgs { RecipientName = GM, Message = $"{user.ChatUserName} hat {stats} Stats ({sum}) für den Charakter gewürfelt " });
            }
            _eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, new MessageArgs { RecipientName = user.ChatUserName, Message = $"Du hast {stats} Stats ({sum}) für den Charakter gewürfelt." });

            return RedirectToAction("Index");
        }
        private string getBoni(int stat)
        {
            int boni = 0;
            int preparedStat = stat - 10;
            if (preparedStat < 0)
            {
                //Negative Boni
                boni = (int)Math.Round(Math.Floor((double)preparedStat / 2), 0);
                return boni.ToString();
            }
            else
            {
                //Positive Boni
                boni = preparedStat / 2;
                return "+" + boni.ToString();
            }
        }
        private int Roll(int numdies, int die)
        {
            int total = 0;
            for (int i = 0; i < numdies; i++)
            {
                //die+1 cause max value ist exclusive of that value
                total += random.Next(1, die + 1);
            }
            return total;
        }
        private int Roll(int numdies, int die, int removelowestdies)
        {
            List<int> rolls = new List<int>(6);
            for (int i = 0; i < numdies; i++)
            {
                //die+1 cause max value ist exclusive of that value
                rolls.Add(random.Next(1, die + 1));
            }
            for (int i = 0; i < removelowestdies; i++)
            {
                rolls.Remove(rolls.Min());
            }
            return rolls.Sum();

        }
        private List<ChatUserModel> Users()
        {
            return _context.ChatUserModels.ToList();
        }
    }
}
using BobDeathmic.Data;
using BobDeathmic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BobDeathmic.Models;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.ViewModels.User;
using BobDeathmic.ViewModels.ReactDataClasses.Table;
using BobDeathmic.ViewModels.ReactDataClasses.Table.Columns;
using BobDeathmic.ViewModels.ReactDataClasses.Other;
using Newtonsoft.Json;

namespace BobDeathmic.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ChatUserModel> _userManager;
        private readonly SignInManager<ChatUserModel> _signInManager;
        public UserController(ApplicationDbContext context, UserManager<ChatUserModel> userManager, SignInManager<ChatUserModel> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [TempData]
        public string StatusMessage { get; set; }
        [Authorize(Roles = "User,Dev,Admin")]
        public IActionResult Index()
        {
            return View();
        }
        /*
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Subscriptions()
        {
            ChatUserModel usermodel = await _userManager.GetUserAsync(this.User);
            List<StreamSubscription> streamsubs = _context.StreamSubscriptions.Include(ss => ss.User).Include(ss => ss.Stream).Where(ss => ss.User == usermodel).ToList();
            List<Stream> FilteredStreams = new List<Stream>();
            foreach (var stream in _context.StreamModels)
            {
                bool add = true;

                if (streamsubs != null && streamsubs.Where(ss => ss.Stream.StreamName == stream.StreamName && ss.Stream.Type == stream.Type).Count() > 0)
                {
                    add = false;
                }
                if (add)
                {
                    FilteredStreams.Add(stream);
                }
            }
            AddSubscriptionViewModel model = new AddSubscriptionViewModel();
            if (FilteredStreams.Count() > 0)
            {
                model.SubscribableStreams = FilteredStreams;
            }
            model.Subscriptions = streamsubs;
            return View(model);
        }
        */
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Subscriptions()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<String> SubscriptionsData()
        {
            ChatUserModel usermodel = await _userManager.GetUserAsync(this.User);
            List<StreamSubscription> streamsubs = _context.StreamSubscriptions.Include(ss => ss.User).Include(ss => ss.Stream).Where(ss => ss.User == usermodel).ToList();
            Table table = new Table();
            Row row = new Row(false,true);
            row.AddColumn(new TextColumn(0, "StreamName",true));
            row.AddColumn(new TextColumn(1, "Sub Status"));
            table.AddRow(row);
            //TODO Second Request for Streams not yet added

            foreach (var streamsub in streamsubs)
            {
                Row newrow = new Row();
                newrow.AddColumn(new TextColumn(0, streamsub.Stream.StreamName));
                if (streamsub.Subscribed == SubscriptionState.Subscribed)
                {
                    newrow.AddColumn(new StreamSubColumn(1, true, streamsub.ID));
                }
                else
                {
                    newrow.AddColumn(new StreamSubColumn(1, false, streamsub.ID));
                }
                table.AddRow(newrow);
            }
            return table.getJson();
        }
        
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<String> SubscribableStreamsData()
        {
            ChatUserModel usermodel = await _userManager.GetUserAsync(this.User);
            List<StreamSubscription> streamsubs = _context.StreamSubscriptions.Include(ss => ss.User).Include(ss => ss.Stream).Where(ss => ss.User == usermodel).ToList();
            List<SubscribableStream> FilteredStreams = new List<SubscribableStream>();
            foreach (var stream in _context.StreamModels)
            {
                bool add = true;

                if (streamsubs != null && streamsubs.Where(ss => ss.Stream.StreamName == stream.StreamName && ss.Stream.Type == stream.Type).Count() > 0)
                {
                    add = false;
                }
                if (add)
                {
                    FilteredStreams.Add(new SubscribableStream(stream.StreamName,stream.ID));
                }
            }
            return JsonConvert.SerializeObject(FilteredStreams);
        }
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<bool> AddSubscription(int streamid)
        {
            var stream = _context.StreamModels.AsQueryable().Where(s => s.ID == streamid).FirstOrDefault();
            ChatUserModel user = await _userManager.GetUserAsync(this.User);
            if (stream != null)
            {
                if (user.StreamSubscriptions == null)
                {
                    user.StreamSubscriptions = new List<StreamSubscription>();
                }
                StreamSubscription newsub = new StreamSubscription();
                newsub.Stream = stream;
                newsub.User = user;
                newsub.Subscribed = SubscriptionState.Subscribed;
                _context.StreamSubscriptions.Add(newsub);

                user.StreamSubscriptions.Add(newsub);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;

        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<string> ChangeSubscription(int id)
        {
            if (id == null)
            {
                return "error";
                //return NotFound();
            }

            StreamSubscription sub = _context.StreamSubscriptions.AsQueryable().Where(ss => ss.ID == id).FirstOrDefault();

            DateTime start = DateTime.Now;
            string @return = "error";
            if (sub != null)
            {
                switch (sub.Subscribed)
                {
                    case SubscriptionState.Subscribed:
                        sub.Subscribed = SubscriptionState.Unsubscribed;
                        @return = "false";
                        break;
                    case SubscriptionState.Unsubscribed:
                        sub.Subscribed = SubscriptionState.Subscribed;
                        @return = "true";
                        break;
                }
                await _context.SaveChangesAsync();
            }
            return @return;
            //return RedirectToAction(nameof(Subscriptions));
        }


        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> DeleteUser()
        {
            return View();
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> DeleteUser(int? id)
        {
            var user = await _userManager.GetUserAsync(this.User);
            await _userManager.DeleteAsync(user);
            return Redirect(nameof(MainController.Index));
        }

        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.GetUserAsync(this.User);
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    StatusMessage = "Passwort wurde geändert.";
                    return RedirectToAction(nameof(ChangePassword));
                }
                StatusMessage = "Passwort wurde nicht geändert.";
                return RedirectToAction(nameof(ChangePassword));
            }
            return RedirectToAction(nameof(ChangePassword));
        }
        #region Helpers
        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        #endregion
    }
}
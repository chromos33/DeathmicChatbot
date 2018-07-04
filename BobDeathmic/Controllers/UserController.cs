using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BobDeathmic.Models;
using BobDeathmic.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Subscriptions()
        {
            ChatUserModel usermodel = await _userManager.GetUserAsync(this.User);
            List<StreamSubscription> streamsubs = _context.StreamSubscriptions.Include(ss => ss.User).Include(ss => ss.Stream).Where(ss => ss.User == usermodel).ToList();
            List<Stream> FilteredStreams = new List<Stream>();
            foreach (var stream in _context.StreamModels)
            {
                bool add = true;
                
                if(streamsubs != null && streamsubs.Where(ss => ss.Stream.StreamName == stream.StreamName).Count() > 0)
                {
                    add = false;
                }
                if(add)
                {
                    FilteredStreams.Add(stream);
                }
            }
            if(FilteredStreams.Count() > 0)
            {
                ViewData["SubscribeableStream"] = FilteredStreams;
            }
            ViewData["Subscriptions"] = streamsubs;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubscription(Models.User.AddSubscriptionViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var stream = _context.StreamModels.Where(s => s.StreamName.ToLower() == model.StreamNameForSubscription.ToLower()).FirstOrDefault();
            ChatUserModel user = await _userManager.GetUserAsync(this.User);
            if (stream != null)
            {
                if(user.StreamSubscriptions == null)
                {
                    user.StreamSubscriptions = new List<StreamSubscription>();
                }
                StreamSubscription newsub = new StreamSubscription();
                newsub.Stream = stream;
                newsub.User = user;
                newsub.Subscribed = Models.Enum.SubscriptionState.Subscribed;
                _context.StreamSubscriptions.Add(newsub);
                
                user.StreamSubscriptions.Add(newsub);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Subscriptions));

        }
        public async Task<bool?> ChangeSubscription(int? id)
        {
            
            if(id == null)
            {
                return null;
                //return NotFound();
            }
            
            StreamSubscription sub = _context.StreamSubscriptions.Where(ss => ss.ID == id).FirstOrDefault();
            
            DateTime start = DateTime.Now;
            bool @return = false;
            if (sub != null)
            {
                switch(sub.Subscribed)
                {
                    case Models.Enum.SubscriptionState.Subscribed:
                        sub.Subscribed = Models.Enum.SubscriptionState.Unsubscribed;
                        @return = false;
                        break;
                    case Models.Enum.SubscriptionState.Unsubscribed:
                        sub.Subscribed = Models.Enum.SubscriptionState.Subscribed;
                        @return = true;
                        break;
                }
                await _context.SaveChangesAsync();
            }
            double duration = DateTime.Now.Subtract(start).TotalMilliseconds;
            return @return;
            //return RedirectToAction(nameof(Subscriptions));
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(Models.User.ChangePasswordViewModel model)
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
                    return RedirectToAction(nameof(ChangePassword), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(ChangePassword), new { Message = ManageMessageId.Error });
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
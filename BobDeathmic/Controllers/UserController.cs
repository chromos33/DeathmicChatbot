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
            ChatUserModel user = await _userManager.GetUserAsync(this.User);
            List<Stream> FilteredStreams = new List<Stream>();
            foreach (var stream in _context.StreamModels)
            {
                bool add = true;
                if(user.StreamSubscriptions == null)
                {
                    user.StreamSubscriptions = new List<StreamSubscription>();
                    await _context.SaveChangesAsync();
                }
                if(user.StreamSubscriptions.Where(ss => ss.Stream.StreamName == stream.StreamName).Count() > 0)
                {
                    add = false;
                }
                FilteredStreams.Add(stream);
            }
            ViewData["SubscribeableStream"] = FilteredStreams;
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
            //TODO: Subscription

            //Placeholder
            return View("Subscriptions");

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
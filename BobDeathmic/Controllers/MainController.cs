using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Services;
using BobDeathmic.ViewModels;
using BobDeathmic.ViewModels.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static BobDeathmic.Controllers.UserController;

namespace BobDeathmic.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Subscriptions", "User");
            }
            //return View();
            return RedirectToAction("Login");
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private readonly UserManager<ChatUserModel> _userManager;
        private readonly SignInManager<ChatUserModel> _signInManager;
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly ApplicationDbContext _dbcontext;

        public MainController(
            UserManager<ChatUserModel> userManager,
            SignInManager<ChatUserModel> signInManager,
            ILogger<MainController> logger,
            IEventBus eventBus,
            ApplicationDbContext dbcontext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _eventBus = eventBus;
            _dbcontext = dbcontext;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    if (returnUrl != null)
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Subscriptions", "User");
                    }

                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(MainController.Index), "Main");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestPasswort(RequestPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = _dbcontext.ChatUserModels.Where(cu => cu.ChatUserName.ToLower() == model.UserName.ToLower() || cu.UserName.ToLower() == model.UserName.ToLower()).FirstOrDefault();

            if (user != null)
            {
                string password = GeneratePassword();
                //var result = await _userManager.ResetPasswordAsync(user, await _userManager.GeneratePasswordResetTokenAsync(user), password);
                var result2 = await _userManager.RemovePasswordAsync(user);
                if (result2.Succeeded)
                {
                    result2 = await _userManager.AddPasswordAsync(user, password);
                    if (result2.Succeeded)
                    {
                        _eventBus.TriggerEvent(EventType.PasswordRequestReceived, new Args.PasswordRequestArgs { UserName = user.ChatUserName, TempPassword = password });
                    }

                }
            }
            return RedirectToAction(nameof(Login));
        }
        private string GeneratePassword()
        {
            Random random = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / DateTime.Now.Hour);
            int passes = random.Next(1, 8);
            string password = "";
            for (int passcounter = 0; passcounter < passes; passcounter++)
            {
                string LoremIpsum = Helper.StaticHelper.GetLoremIpsum().Replace(" ", "").Replace(",", "").Replace(".", "");
                password += LoremIpsum.Substring(0, random.Next(0, LoremIpsum.Length));
                password += random.Next(5000);
            }
            int start = random.Next(password.Length);
            int end = random.Next(random.Next(password.Length - start - 1));
            password = password.Substring(start, end);
            if (password.Length < 8)
            {
                password += random.Next(5000);
            }
            password = GeneratePasswordHash(password);
            return password;
        }
        private string GeneratePasswordHash(string _password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: _password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));
            return password;
        }
        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(MainController.Index), "Main");
            }
        }
        #endregion
    }
}

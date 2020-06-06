using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Models;
using BobDeathmic.ViewModels.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace BobDeathmic.Controllers
{
    public class UserAdminController : Controller
    {
        private Data.ApplicationDbContext _context;
        private IServiceProvider _serviceProvider;
        private readonly UserManager<ChatUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserAdminController(Data.ApplicationDbContext context, IServiceProvider serviceProvider, UserManager<ChatUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            List<UserRolesViewModel> UserList = new List<UserRolesViewModel>();
            foreach (ChatUserModel user in _context.Users)
            {
                UserRolesViewModel newUserRolesModel = new UserRolesViewModel();
                newUserRolesModel.User = user;
                newUserRolesModel.Roles = await _userManager.GetRolesAsync(user);
                UserList.Add(newUserRolesModel);
            }
            ViewData["PossibleRoles"] = new String[] { "Admin", "User" };
            return View(UserList);
        }
        public async Task<bool> SaveUserRoles(string UserId, int isAdmin)
        {
            var user = _context.ChatUserModels.AsQueryable().Where(cum => cum.Id == UserId).FirstOrDefault();
            IdentityResult result = null;
            if (isAdmin == 1)
            {
                result = await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            return result.Succeeded;
        }
    }
}
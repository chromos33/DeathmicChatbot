using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Services.Helper;
using Microsoft.AspNetCore.Mvc;

namespace BobDeathmic.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View("Index", CommandBuilder.BuildCommands("twitch", true));
        }
    }
}
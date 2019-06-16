using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Services.Helper;
using BobDeathmic.Services.Helper.Commands;
using Microsoft.AspNetCore.Mvc;

namespace BobDeathmic.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View("Index", Services.Helper.CommandBuilder.BuildCommands("twitch", true));
        }
    }
}
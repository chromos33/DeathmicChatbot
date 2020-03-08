using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Services.Commands;
using BobDeathmic.Services.Helper;
using Microsoft.AspNetCore.Mvc;

namespace BobDeathmic.Controllers
{
    public class HelpController : Controller
    {
        private ICommandService commandService;
        public HelpController(ICommandService commandService)
        {
            this.commandService = commandService;
        }
        public IActionResult Index()
        {
            return View("Index", commandService.GetCommands(Data.Enums.ChatType.Twitch));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BobDeathmic.Controllers
{
    public class EventDateFinderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
using BobDeathmic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ReactDataClasses.EventDateFinder.OverView
{
    public class Calendar
    {
        public string Name { get; set; }
        public string EditLink { get; set; }
        public string VoteLink { get; set; }
        public ChatUser[] ChatUsers { get; set; }
        public string key { get; set; }

    }
}

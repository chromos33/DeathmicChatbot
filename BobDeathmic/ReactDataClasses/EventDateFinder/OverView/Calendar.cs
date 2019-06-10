using BobDeathmic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ReactDataClasses.Events.OverView
{
    public class Calendar
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EditLink { get; set; }
        public string VoteLink { get; set; }
        public string DeleteLink { get; set; }
        public ChatUser[] ChatUsers { get; set; }
        public int key { get; set; }

    }
}

using BobDeathmic.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic;

namespace BobDeathmic.ViewModels.ReactDataClasses.EventDateFinder.Vote
{
    public class EventDateData
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public List<VoteRequest> Requests { get; set; }
    }
}

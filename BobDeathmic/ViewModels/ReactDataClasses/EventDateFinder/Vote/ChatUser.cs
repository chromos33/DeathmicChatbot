using BobDeathmic.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic;

namespace BobDeathmic.ViewModels.ReactDataClasses.EventDateFinder.Vote
{
    public class VoteChatUser
    {
        public List<VoteRequest> Requests { get; set; }
        public bool canEdit { get; set; }
        public string Name { get; set; }
        public string key { get; set; }
    }
}

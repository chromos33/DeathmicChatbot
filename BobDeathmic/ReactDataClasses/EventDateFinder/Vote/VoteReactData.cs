using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ReactDataClasses.EventDateFinder.Vote
{
    public class VoteReactData
    {
        public List<VoteChatUser> User { get; set; }
        public List<EventDateHeader> Header { get; set; }
    }
}

using BobDeathmic.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ReactDataClasses.Events.Vote
{
    public class VoteRequest
    {
        public string UserName { get; set; }
        public string AppointmentRequestID { get; set; }
        public AppointmentRequestState State { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public string[] States {
            get
            {
                return new string[] { "NotYetVoted","Available","NotAvailable","IfNeedBe"};
            }
        }
        
    }
}

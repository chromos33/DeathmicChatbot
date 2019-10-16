using BobDeathmic.Data.DBModels.EventCalendar;
using BobDeathmic.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic;

namespace BobDeathmic.ViewModels.ReactDataClasses.EventDateFinder.Vote
{
    public class VoteRequest
    {
        public string UserName { get; set; }
        public string AppointmentRequestID { get; set; }
        public AppointmentRequestState State { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public bool canEdit { get; set; }

        public string[] States
        {
            get
            {
                return new string[] { "NotYetVoted", "Available", "NotAvailable", "IfNeedBe" };
            }
        }

    }
}

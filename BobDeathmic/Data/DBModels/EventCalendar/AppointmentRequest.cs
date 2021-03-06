﻿using BobDeathmic;
using BobDeathmic.Data.DBModels.EventCalendar;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Models;
using BobDeathmic.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Data.DBModels.EventCalendar
{
    //Missing counter relation to ChatUserModel
    public class AppointmentRequest
    {
        public string ID { get; set; }
        public EventDate EventDate { get; set; }
        public ChatUserModel Owner { get; set; }
        public AppointmentRequestState State { get; set; }
        public string Comment { get; set; }
        public AppointmentRequest()
        {
            State = 0;
        }
        public AppointmentRequest(ChatUserModel Owner, EventDate EventDate)
        {
            this.EventDate = EventDate;
            this.Owner = Owner;
            State = 0;
        }
    }
    public enum AppointmentRequestState
    {
        NotYetVoted = 0,
        Available = 1,
        NotAvailable = 2,
        IfNeedBe = 3

    }
}


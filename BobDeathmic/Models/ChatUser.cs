using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Models.EventDateFinder;
using BobDeathmic.Models.EventDateFinder.ManyMany;
using BobDeathmic.Models.GiveAway;
using BobDeathmic.Models.GiveAwayModels;
using Microsoft.AspNetCore.Identity;

namespace BobDeathmic.Models
{
    // Add profile data for application users by adding properties to the ChatUserModel class
    public class ChatUserModel : IdentityUser
    {

        public string ChatUserName { get; set; }
        public List<StreamSubscription> StreamSubscriptions { get; set; }
        public string InitialPassword { get; set; }
        public List<Stream> OwnedStreams { get; set; }

        public List<GiveAwayItem> OwnedItems { get; set; }

        public List<GiveAwayItem> ReceivedItems { get; set; }
        public List<User_GiveAwayItem> AppliedTo { get; set; }
        public List<ChatUserModel_Calendar> Calendars { get; set; }
        public List<AppointmentRequest> AppointmentRequests { get; set; }
        public bool IsSubscribed(string streamname)
        {
            if(StreamSubscriptions != null && StreamSubscriptions.Where(ss => ss.Stream != null && ss.Stream.StreamName.ToLower() == streamname.ToLower() && ss.Subscribed == Enum.SubscriptionState.Subscribed).FirstOrDefault() != null)
            {
                return true;
            }
            return false;
        }

    }
}

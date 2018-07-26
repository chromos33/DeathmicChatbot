using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

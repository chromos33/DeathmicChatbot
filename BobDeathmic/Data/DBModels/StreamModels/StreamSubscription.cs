using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Data.DBModels.StreamModels
{
    public class StreamSubscription
    {
        public int ID { get; set; }
        public Stream Stream { get; set; }
        public SubscriptionState Subscribed { get; set; }
        public ChatUserModel User { get; set; }
    }
}

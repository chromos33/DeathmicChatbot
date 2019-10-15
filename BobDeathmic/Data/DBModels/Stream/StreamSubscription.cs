using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models
{
    public class StreamSubscription
    {
        public int ID { get; set; }
        public Stream Stream { get; set; }
        public Enum.SubscriptionState Subscribed { get; set; }
        public ChatUserModel User { get; set; }
    }
}

using BobDeathmic.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Eventbus
{
    public class EventBusLocal : IEventBus
    {
        public event EventHandler<StreamEventArgs> StreamChanged;

        public void TriggerEvent(string @event,dynamic EventData)
        {
            switch(@event)
            {
                case "StreamChanged":
                    StreamChanged(this, EventData);
                    break;
            }
        }
    }
}

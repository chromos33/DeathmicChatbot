using BobDeathmic.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Eventbus
{
    public interface IEventBus
    {
        event EventHandler<StreamEventArgs> StreamChanged;
        void TriggerEvent(string @event, dynamic EventData);
    }
}

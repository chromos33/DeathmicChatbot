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
        public event EventHandler<DiscordMessageArgs> DiscordMessageReceived;
        public event EventHandler<TwitchMessageArgs> TwitchMessageReceived;
        public event EventHandler<PasswordRequestArgs> PasswordRequestReceived;

        public void TriggerEvent(EventType @event,dynamic EventData)
        {
            switch(@event)
            {
                case EventType.StreamChanged:
                    StreamChanged(this, EventData);
                    break;
                case EventType.DiscordMessageReceived:
                    DiscordMessageReceived(this, EventData);
                    break;
                case EventType.TwitchMessageReceived:
                    TwitchMessageReceived(this, EventData);
                    break;
                case EventType.PasswordRequestReceived:
                    PasswordRequestReceived(this, EventData);
                    break;
            }
        }
    }
    public enum EventType
    {
        StreamChanged = 1,
        DiscordMessageReceived = 2,
        TwitchMessageReceived = 3,
        PasswordRequestReceived = 4,
    }
}

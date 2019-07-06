using BobDeathmic.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Eventbus
{
    public class EventBusLocal : IEventBus
    {
        public event EventHandler<StreamTitleChangeArgs> StreamTitleChangeRequested;
        public event EventHandler<StreamTitleChangeArgs> StreamTitleChanged;
        public event EventHandler<MessageArgs> DiscordMessageSendRequested;
        public event EventHandler<PasswordRequestArgs> PasswordRequestReceived;
        public event EventHandler<TwitchMessageArgs> TwitchMessageReceived;
        public event EventHandler<RelayMessageArgs> RelayMessageReceived;
        public event EventHandler<StrawPollRequestEventArgs> StrawPollRequested;







        
        


        

        public void TriggerEvent(EventType @event, dynamic EventData)
        {
            switch (@event)
            {
                case EventType.RelayMessageReceived:
                    RelayMessageReceived(this, EventData);
                    break;
                case EventType.TwitchMessageReceived:
                    TwitchMessageReceived(this, EventData);
                    break;
                case EventType.PasswordRequestReceived:
                    PasswordRequestReceived(this, EventData);
                    break;
                case EventType.StreamTitleChangeRequested:
                    StreamTitleChangeRequested(this, EventData);
                    break;
                case EventType.StreamTitleChanged:
                    StreamTitleChanged(this, EventData);
                    break;
                case EventType.StrawPollRequested:
                    StrawPollRequested(this, EventData);
                    break;
                case EventType.DiscordMessageSendRequested:
                    DiscordMessageSendRequested(this, EventData);
                    break;
            }
        }
    }
    public enum EventType
    {
        RelayMessageReceived = 2,
        TwitchMessageReceived = 3,
        PasswordRequestReceived = 4,
        StreamTitleChangeRequested = 5,
        StreamTitleChanged = 6,
        StrawPollRequested = 9,
        DiscordMessageSendRequested = 10
    }
}

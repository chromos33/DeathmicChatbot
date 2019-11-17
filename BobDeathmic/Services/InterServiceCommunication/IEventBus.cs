using BobDeathmic.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Eventbus
{
    public interface IEventBus
    {
        event EventHandler<RelayMessageArgs> RelayMessageReceived;
        event EventHandler<TwitchMessageArgs> TwitchMessageReceived;
        event EventHandler<PasswordRequestArgs> PasswordRequestReceived;
        event EventHandler<StreamTitleChangeArgs> StreamTitleChangeRequested;
        event EventHandler<StreamTitleChangeArgs> StreamTitleChanged;
        event EventHandler<StrawPollRequestEventArgs> StrawPollRequested;
        event EventHandler<MessageArgs> DiscordMessageSendRequested;
        event EventHandler<CommandResponseArgs> CommandOutputReceived;
        void TriggerEvent(EventType @event, dynamic EventData);
    }
}

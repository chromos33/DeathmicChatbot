﻿using BobDeathmic.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Eventbus
{
    public interface IEventBus
    {
        event EventHandler<StreamEventArgs> StreamChanged;
        event EventHandler<DiscordMessageArgs> DiscordMessageReceived;
        event EventHandler<TwitchMessageArgs> TwitchMessageReceived;
        event EventHandler<PasswordRequestArgs> PasswordRequestReceived;
        void TriggerEvent(EventType @event, dynamic EventData);
    }
}
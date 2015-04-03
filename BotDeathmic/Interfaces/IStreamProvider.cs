using System;
using System.Collections.Generic;
using DeathmicChatbot.StreamInfo;
using DeathmicChatbot.StreamInfo.Twitch;
//using DeathmicChatbot.StreamInfo.Hitbox;

namespace DeathmicChatbot.Interfaces
{
    public interface IStreamProvider
    {
        event EventHandler<StreamEventArgs> StreamStarted;
        event EventHandler<StreamEventArgs> StreamStopped;
        bool AddStream(string stream);
        void RemoveStream(string stream);
        void CheckStreams();
        List<string> GetStreamInfoArray();
        string GetLink();
        
    }
}

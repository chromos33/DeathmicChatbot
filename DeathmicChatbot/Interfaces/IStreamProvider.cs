using System;
using System.Collections.Generic;
using DeathmicChatbot.StreamInfo;
//using DeathmicChatbot.StreamInfo.Hitbox;

namespace DeathmicChatbot.Interfaces
{
    public interface IStreamProvider
    {
        event EventHandler<StreamEventArgs> StreamStarted;
        event EventHandler<StreamEventArgs> StreamStopped;
        event EventHandler<StreamEventArgs> StreamGlobalNotification;
        bool AddStream(string stream);
        void RemoveStream(string stream);
        void CheckStreams();
        List<string> GetStreamInfoArray();
        string GetLink();
        void StartTimer();
        
    }
}

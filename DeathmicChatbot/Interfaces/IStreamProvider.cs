#region Using

using System;
using System.Collections.Generic;

#endregion


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
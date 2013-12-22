#region Using

using System;
using System.Collections.Generic;

#endregion


namespace DeathmicChatbot
{
    public interface IStreamProvider
    {
        event EventHandler<StreamEventArgs> StreamStarted;
        event EventHandler<StreamEventArgs> StreamStopped;
        bool AddStream(string stream);
        void RemoveStream(string stream);
        void CheckStreams();
        IEnumerable<string> GetStreamInfoArray();
        string GetLink();
    }
}
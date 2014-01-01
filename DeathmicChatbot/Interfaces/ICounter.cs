#region Using

using System;

#endregion


namespace DeathmicChatbot.Interfaces
{
    internal interface ICounter
    {
        event EventHandler<CounterEventArgs> CountRequested;
        event EventHandler<CounterEventArgs> ResetRequested;
        event EventHandler<CounterEventArgs> StatRequested;
        void Count(string sName);
        void CounterReset(string sName);
        void CounterStats(string sName);
    }
}
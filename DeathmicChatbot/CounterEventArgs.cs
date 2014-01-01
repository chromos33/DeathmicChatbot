#region Using

using System;

#endregion


namespace DeathmicChatbot
{
    public class CounterEventArgs : EventArgs
    {
        public CounterEventArgs(string sMessage) { Message = sMessage; }

        public string Message { get; private set; }
    }
}
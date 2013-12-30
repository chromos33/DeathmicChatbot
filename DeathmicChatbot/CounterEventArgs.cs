#region Using

using System;

#endregion


namespace DeathmicChatbot
{
    internal class CounterEventArgs : EventArgs
    {
        public CounterEventArgs(string sMessage) { Message = sMessage; }

        public string Message { get; private set; }
    }
}
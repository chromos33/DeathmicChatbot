#region Using

using System.Diagnostics;

#endregion


namespace DeathmicChatbot.Interfaces
{
    public interface ILogManagerProvider
    {
        void WriteToLog(string mode, string message, StackTrace trace = null);
    }
}
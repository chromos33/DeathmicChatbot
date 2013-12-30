#region Using

using System.Diagnostics;

#endregion


namespace DeathmicChatbot
{
    public interface ILogManagerProvider
    {
        void WriteToLog(string mode, string message, StackTrace trace = null);
    }
}
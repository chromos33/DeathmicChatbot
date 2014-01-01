#region Using

using System.Diagnostics;
using DeathmicChatbot.Interfaces;

#endregion


namespace DeathmicChatbot
{
    internal class LogManagerProvider : ILogManagerProvider
    {
        private readonly LogManager _log;
        public LogManagerProvider(LogManager log) { _log = log; }

        #region ILogManagerProvider Members

        public void WriteToLog(string mode,
                               string message,
                               StackTrace trace = null) { _log.WriteToLog(mode, message, trace); }

        #endregion
    }
}
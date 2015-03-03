#region Using

using System;
using System.Diagnostics;
using System.IO;

#endregion


namespace DeathmicChatbot
{
    public class LogManager
    {
        private readonly String _path;

        public LogManager(string path) { _path = path; }

        public void WriteToLog(string level,
                               string text,
                               StackTrace trace = null)
        {
            if (trace == null)
                trace = new StackTrace();

            var source = trace.GetFrame(1).GetMethod().ToString();

            var log = File.AppendText(_path);

            var logtext = String.Format("[{0:s}] [{1}] [{2}] {3}",
                                        DateTime.Now,
                                        source,
                                        level,
                                        text);

            log.WriteLine(logtext);

            log.Flush();
            log.Close();
        }
    }
}
using System;
using System.IO;
using System.Diagnostics;

namespace DeathmicChatbot
{
    internal class LogManager
    {
        private String path;

        public LogManager(string path)
        {
            this.path = path;
        }

        public void WriteToLog(string level, string text, StackTrace trace = null)
        {
            if (trace == null)
            {
                trace = new StackTrace();
            }

            string source = trace.GetFrame(1).GetMethod().ToString();

            StreamWriter log = File.AppendText(path);

            string logtext = String.Format("[{0:s}] [{1}] [{2}] {3}", DateTime.Now, source, level, text);

            log.WriteLine(logtext);

            log.Flush();
            log.Close();
        }
    }
}
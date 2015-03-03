#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#endregion


namespace DeathmicChatbot
{
    internal static class JoinLogger
    {
        private static XMLProvider xmlprovider;
        private const string LOGGING_DIRECTORY_NAME = "join_logging";
        private const string LOGGING_OPS_FILE = "logging_ops.txt";

        public static void LogJoin(string sNick, MessageQueue messageQueue)
        {
            MakeSureLoggingDirectoryExists();
            MakeSureLoggingFileForUserExists(sNick);
            WriteJoinToLogFile(sNick);
            WhisperJoinStatsToLoggingOps(sNick, messageQueue);
        }

        private static void WhisperJoinStatsToLoggingOps(string sNick,
                                                         MessageQueue
                                                             messageQueue)
        {
            if (sNick != System.Configuration.ConfigurationManager.AppSettings["Name"] || sNick != "BotDeathmic")
            {
                if (!File.Exists(LOGGING_OPS_FILE))
                    File.Create(LOGGING_OPS_FILE).Close();

                var streamReader = new StreamReader(LOGGING_OPS_FILE);
                var loggingOps = new List<string>();
                while (!streamReader.EndOfStream)
                {
                    var sLine = streamReader.ReadLine();
                    if (!string.IsNullOrEmpty(sLine) && !loggingOps.Contains(sLine))
                        loggingOps.Add(sLine);
                }
                streamReader.Close();

                foreach (var loggingOp in loggingOps)
                    messageQueue.PrivateNoticeEnqueue(loggingOp,
                                                      GetLastVisitData(sNick));
            }
        }

        private static string GetLastVisitData(string sNick)
        {
            if (xmlprovider == null){xmlprovider = new XMLProvider();}

            string[] userdata = xmlprovider.UserInfo(sNick).Split(',');
            String days_since_last_visit = DateTime.Now.Subtract(Convert.ToDateTime(userdata[1])).ToString("d' days 'h':'mm':'ss");
            string visitstring = "";
            switch(userdata[0])
            {
                case "1": visitstring = userdata[0] +" st"; break;
                case "2": visitstring = userdata[0] +" nd"; break;
                case "3": visitstring = userdata[0] +" rd"; break;
                default: visitstring = userdata[0] +" th"; break;
            }
            // Userdata[0] = VisitCount , Userdata[1] = LastVisit
            return string.Format("This is {0}'s {1} visit." + System.Environment.NewLine + "Their last visit was on {3} ({4} ago)", sNick, visitstring, userdata[1], days_since_last_visit);

            /*
            var streamReader = new StreamReader(GetLogFilePath(sNick));

            var joins = new List<string>();
            while (!streamReader.EndOfStream)
            {
                var sLine = streamReader.ReadLine();
                DateTime join;
                if (string.IsNullOrEmpty(sLine) ||
                    !DateTime.TryParse(sLine, out join))
                    continue;
                joins.Add(sLine);
            }
            streamReader.Close();

            var iVisitNumbers = joins.Count;

            var stringBuilder = new StringBuilder();

            string sNumberDescriptor;

            switch (iVisitNumbers)
            {
                case 1:
                    sNumberDescriptor = "st";
                    break;

                case 2:
                    sNumberDescriptor = "nd";
                    break;

                case 3:
                    sNumberDescriptor = "rd";
                    break;

                default:
                    sNumberDescriptor = "th";
                    break;
            }

            var sMessageVisitNumberDescription = string.Format("{0}{1}",
                                                               iVisitNumbers,
                                                               sNumberDescriptor);
            var sMessageVisitNumber = string.Format("This is {0}'s {1} visit.",
                                                    sNick,
                                                    sMessageVisitNumberDescription);
            stringBuilder.Append(sMessageVisitNumber);

            var sMessageLastVisitData = "";

            if (iVisitNumbers > 1)
            {
                var dtLastVisit = DateTime.Parse(joins[joins.Count - 2]);
                var tsLastVisit = DateTime.Now - dtLastVisit;

                sMessageLastVisitData =
                    string.Format(" Their last visit was on {0} ({1} ago).",
                                  joins[joins.Count - 2],
                                  tsLastVisit.ToString(tsLastVisit.Days == 0
                                                           ? "h':'mm':'ss"
                                                           : "d' days 'h':'mm':'ss"));
            }

            stringBuilder.Append(sMessageLastVisitData);

            return stringBuilder.ToString();*/
        }

        private static void WriteJoinToLogFile(string sNick)
        {
            var sLogFilePath = GetLogFilePath(sNick);
            var streamWriter = new StreamWriter(sLogFilePath, true);
            streamWriter.WriteLine(DateTime.Now.ToString());
            streamWriter.Close();
        }

        private static void MakeSureLoggingFileForUserExists(string sNick)
        {
            var sLogFilePath = GetLogFilePath(sNick);
            if (!File.Exists(sLogFilePath))
                File.Create(sLogFilePath).Close();
        }

		private static string GetLogFilePath(string sNick) { return System.IO.Path.Combine(LOGGING_DIRECTORY_NAME, sNick + ".txt"); }

        private static void MakeSureLoggingDirectoryExists() { Directory.CreateDirectory(LOGGING_DIRECTORY_NAME); }
    }
}
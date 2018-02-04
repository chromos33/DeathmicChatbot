using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BobCore
{
    static class Useful_Functions
    {
        public static String[] String_Array_Push(String[] _input, string item)
        {
            List<string> temp = _input.ToList();
            temp.Add(item);
            return temp.ToArray();
        }
        public static List<string> MessageParameters(string message, string trigger)
        {
            //TODO: Clear everything in front of ! to minimize false parameters
            string sMessage = message.ToLower();
            if (sMessage.Split('!').Count() > 1)
            {
                if (sMessage.Split('!')[0].Length > 0)
                {
                    sMessage = sMessage.Replace(sMessage.Split('!')[0], "");
                }
            }
            sMessage = sMessage.Replace(trigger.ToLower(), "");
            List<string> @params = new List<string>();
            if (sMessage.Contains('"'))
            {
                var result = from Match match in Regex.Matches(sMessage, "\"([^\"]*)\"")
                             select match.ToString();
                foreach (string item in result)
                {
                    @params.Add(item.Replace("\"", ""));
                    sMessage = sMessage.Replace(item, "");
                }
                foreach (string item in sMessage.Split(' ').ToList().Where(x => x.ToString() != "").ToList())
                {
                    @params.Add(item);
                }
            }
            else
            {
                if (sMessage == "" || !sMessage.StartsWith(" ", StringComparison.Ordinal))
                {
                    return new List<string>();
                }
                @params = sMessage.Split(' ').ToList().Where(x => x.ToString() != "").ToList();
            }

            return @params;
        }
        public static List<List<string>> MultiMessageParameters(string message,string trigger)
        {
            List<List<string>> Result = new List<List<string>>();
            string sMessage = message.ToLower();
            if (sMessage.Split('!').Count() > 1)
            {
                if (sMessage.Split('!')[0].Length > 0)
                {
                    sMessage = sMessage.Replace(sMessage.Split('!')[0], "");
                }
            }
            sMessage = sMessage.Replace(trigger.ToLower(), "");

            string[] test = sMessage.Split(",");
            bool first = true;
            foreach(string item in test)
            {
                List<string> unfiltered = Useful_Functions.MessageParameters(item, trigger);
                if(first)
                {
                    first = false;
                    unfiltered.RemoveAt(0);
                }
                Result.Add(unfiltered);
            }

            return Result;

        }
        public static bool IsFileReady(String sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    inputStream.Close();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetField(propertyName) != null;
        }

    }
}

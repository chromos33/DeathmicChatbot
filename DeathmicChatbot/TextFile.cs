#region Using

using System.Collections.Generic;
using System.IO;
using DeathmicChatbot.Interfaces;

#endregion


namespace DeathmicChatbot
{
    public class TextFile : ITextFile
    {
        public TextFile(string sFilePath) { FilePath = sFilePath; }

        private string FilePath { get; set; }

        #region ITextFile Members

        public List<string> ReadWholeFileInLines()
        {
            var lines = new List<string>();

            if (!File.Exists(FilePath))
                return lines;

            using (var reader = new StreamReader(FilePath))
            {
                while (!reader.EndOfStream)
                    lines.Add(reader.ReadLine());
            }

            return lines;
        }

        public void WriteLines(IEnumerable<string> lines, bool bAppend = false)
        {
            using (var writer = new StreamWriter(FilePath, bAppend))
            {
                foreach (var line in lines)
                    writer.WriteLine(line);
            }
        }

        #endregion
    }
}
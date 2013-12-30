#region Using

using System.Collections.Generic;

#endregion


namespace DeathmicChatbot
{
    public interface ITextFile
    {
        List<string> ReadWholeFileInLines();
        void WriteLines(IEnumerable<string> lines, bool bAppend = false);
    }
}
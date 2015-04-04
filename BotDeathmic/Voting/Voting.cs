#region Using

using System;
using System.Collections.Generic;

#endregion


namespace DeathmicChatbot
{
    public struct Voting
    {
        public DateTime _dtEndTime;
        public int _iIndex;
        public string _sQuestion;
        public List<string> _slAnswers;
        public string _userInfo;
        public Dictionary<string, string> _votes;
    }
}
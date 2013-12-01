#region Using

using System;
using System.Collections.Generic;
using Sharkbite.Irc;

#endregion


namespace DeathmicChatbot
{
    public struct Voting
    {
        public DateTime _dtEndTime;
        public int _iIndex;
        public string _sQuestion;
        public List<string> _slAnswers;
        public UserInfo _userInfo;
        public Dictionary<string, string> _votes;
    }

    public class VotingEventArgs : EventArgs
    {
        public VotingEventArgs(Voting voting, UserInfo userInfo)
        {
            Voting = voting;
            User = userInfo;
        }

        public Voting Voting { get; private set; }

        public UserInfo User { get; private set; }
    }
}
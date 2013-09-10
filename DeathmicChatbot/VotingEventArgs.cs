using Sharkbite.Irc;
using System;
using System.Collections.Generic;

namespace DeathmicChatbot
{
    public struct Voting
    {
        public int Index;
        public UserInfo User;
        public string Question;
        public List<string> Answers;
        public DateTime EndTime;
        public Dictionary<string, string> Votes;
    }

    public class VotingEventArgs : EventArgs
    {
        public Voting Voting { get; private set; }

        public UserInfo User { get; private set; }

        public VotingEventArgs(Voting argVoting, UserInfo argUser)
        {
            Voting = argVoting;
            User = argUser;
        }
    }
}
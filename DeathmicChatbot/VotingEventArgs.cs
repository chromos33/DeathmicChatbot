using Sharkbite.Irc;
using System;
using System.Collections.Generic;

namespace DeathmicChatbot
{
    public struct Voting
    {
        public int index;
        public UserInfo user;
        public string question;
        public List<string> answers;
        public DateTime endTime;
        public Dictionary<string, string> votes;
    }

    public class VotingEventArgs : EventArgs
    {
        public Voting voting { get; set; }

        public UserInfo user { get; set; }

        public VotingEventArgs(Voting argVoting, UserInfo argUser)
        {
            voting = argVoting;
            user = argUser;
        }
    }
}
#region Using

using System;

#endregion


namespace DeathmicChatbot
{
    public class VotingEventArgs : EventArgs
    {
        public VotingEventArgs(Voting voting, string userInfo)
        {
            Voting = voting;
            User = userInfo;
        }

        public Voting Voting { get; private set; }

        public string User { get; private set; }
    }
}
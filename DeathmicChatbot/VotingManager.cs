#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Sharkbite.Irc;

#endregion


namespace DeathmicChatbot
{
    public class VoteManager
    {
        private const int MAX_IDS = 5;
        private readonly Dictionary<int, Voting> _votings;
        public VoteManager() { _votings = new Dictionary<int, Voting>(); }

        public Dictionary<int, Voting> Votings { get { return new Dictionary<int, Voting>(_votings); } }

        public event EventHandler<VotingEventArgs> VotingStarted;
        public event EventHandler<VotingEventArgs> VotingEnded;
        public event EventHandler<VotingEventArgs> Voted;
        public event EventHandler<VotingEventArgs> VoteRemoved;

        /*Starts a voting
         *
         * Arguments:
         *  question: The question
         *
         *  answers: The possible answers
         *
         * Returns:
         *  The id of the added vote.
        */

        public void StartVoting(UserInfo user,
                                string question,
                                List<string> answers,
                                DateTime endTime)
        {
            var index = 0;
            while (_votings.ContainsKey(index) && index < MAX_IDS)
                index++;
            Voting newVote;
            newVote._iIndex = index;
            newVote._userInfo = user;
            newVote._sQuestion = question;
            newVote._slAnswers = answers;
            newVote._votes = new Dictionary<string, string>();
            newVote._dtEndTime = endTime;
            _votings[index] = newVote;
            VotingStarted(this, new VotingEventArgs(newVote, user));
        }

        /* Ends a voting.
         *
         * Arguments:
         *
         *  user: The user that called for the voting to end.
         *
         *  index: The index of the voting to end.
         */

        public void EndVoting(UserInfo user, int index)
        {
            if (!_votings.ContainsKey(index))
                throw new ArgumentOutOfRangeException("index");
            if (_votings[index]._userInfo.Nick.ToLower() != user.Nick.ToLower())
                throw new InvalidOperationException(
                    "User is not the same that started the vote");
            var endedVote = _votings[index];
            VotingEnded(this, new VotingEventArgs(endedVote, user));
            _votings.Remove(index);
        }

        /* Adds a vote for an answer
         *
         * Arguments
         *
         * user: The user that voted
         *
         * index: The index of the voting
         *
         * answer: The answer that was chosen
         */

        public void Vote(UserInfo user, int index, string answer)
        {
            if (!_votings.ContainsKey(index))
                throw new ArgumentOutOfRangeException("index");
            var voting = _votings[index];
            if (!voting._slAnswers.Contains(answer))
                throw new ArgumentOutOfRangeException("answer");
            voting._votes[user.Nick.ToLower()] = answer;
            Voted(this, new VotingEventArgs(voting, user));
        }

        /* Removes a vote
         *
         * Arguments:
         *
         * user: The unser that wants to remove his vote
         *
         * index: The index of the voting
         */

        public void RemoveVote(UserInfo user, int index)
        {
            if (!_votings.ContainsKey(index))
                throw new ArgumentOutOfRangeException("index");
            var voting = _votings[index];
            voting._votes.Remove(user.Nick.ToLower());
            VoteRemoved(this, new VotingEventArgs(voting, user));
        }

        /* Checks all vote if they have reached their end time.
         */

        public void CheckVotings()
        {
            var votings = new List<Voting>(_votings.Values);
            foreach (var voting in
                votings.Where(voting => voting._dtEndTime <= DateTime.Now))
                EndVoting(voting._userInfo, voting._iIndex);
        }
    }
}
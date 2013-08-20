using System.Collections.Generic;
using Sharkbite.Irc;
using System;

namespace DeathmicChatbot
{
    public class VoteManager
    {
        private static int MAX_IDS = 5;
        private Dictionary<int, Voting> _votings;

        public Dictionary<int, Voting> Votings
        {
            get { return new Dictionary<int, Voting>(_votings); }
        }

        public event EventHandler<VotingEventArgs> VotingStarted;
        public event EventHandler<VotingEventArgs> VotingEnded;
        public event EventHandler<VotingEventArgs> Voted;
        public event EventHandler<VotingEventArgs> VoteRemoved;

        public VoteManager()
        {
            _votings = new Dictionary<int, Voting>();
        }
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
        public int StartVoting(UserInfo user, string question, List<string> answers, DateTime endTime)
        {
            int index = 0;
            for (; _votings.ContainsKey(index) && index < MAX_IDS; ++index)
                continue;
            if (index >= MAX_IDS)
                throw new InvalidOperationException(
                    string.Format("Too many votings already running. Maximum is {0}", MAX_IDS));
            Voting newVote;
            newVote.index = index;
            newVote.user = user;
            newVote.question = question;
            newVote.answers = answers;
            newVote.votes = new Dictionary<string, string>();
            newVote.endTime = endTime;
            _votings[index] = newVote;
            VotingStarted(this, new VotingEventArgs(newVote, user));
            return index;
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
                throw new ArgumentOutOfRangeException("id");
            if (_votings[index].user.Nick.ToLower() != user.Nick.ToLower())
                throw new InvalidOperationException("User is not the same that started the vote");
            Voting endedVote = _votings[index];
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
                throw new ArgumentOutOfRangeException("id");
            Voting voting = _votings[index];
            if (!voting.answers.Contains(answer))
                throw new ArgumentOutOfRangeException("answer");
            voting.votes[user.Nick.ToLower()] = answer;
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
                throw new ArgumentOutOfRangeException("id");
            Voting voting = _votings[index];
            voting.votes.Remove(user.Nick.ToLower());
            VoteRemoved(this, new VotingEventArgs(voting, user));
        }
        /* Checks all vote if they have reached their end time.
         */
        public void CheckVotings()
        {
            List<Voting> votings = new List<Voting>(_votings.Values);
            foreach (Voting voting in votings)
            {
                if (voting.endTime <= DateTime.Now)
                {
                    EndVoting(voting.user, voting.index);
                }
            }
        }
    }
}
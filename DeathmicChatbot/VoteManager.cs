#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sharkbite.Irc;

#endregion


namespace DeathmicChatbot
{
    public class VoteManager
    {
        private const int MAX_IDS = 5;
        private readonly Dictionary<int, Voting> _votings;
		private readonly LogManager _log;
		private readonly string nick;

		public VoteManager(CommandManager man, LogManager log, string nick_) {
			_votings = new Dictionary<int, Voting>();
			_log = log;
			nick = nick_;

			man.setPublicCommand("startvote", startVotingCommand);
			man.setPublicCommand("vote", voteCommand);
			man.setPrivateCommand("removevote", removeVoteCommand);
			man.setPrivateCommand("rmvote", removeVoteCommand);
			man.setPublicCommand("endvote", endVotingCommand);
			man.setPublicCommand("listvote", listVotingsCommand);
			man.setPublicCommand("lsvote", listVotingsCommand);
		}

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

		private void startVotingCommand(MessageContext ctx, string text, string commandArgs)
		{
			var args = commandArgs != null
				? commandArgs.Split('|')
				: new string[0];
			if (args.Length < 3)
			{
				ctx.replyPrivate(string.Format(
					"Please use the following format: {0}startvote <time>|<question>|<answer1,answer2,...>",
					CommandManager.ACTIVATOR));
				return;
			}
			var timeString = args[0];
			var timeRegex = new Regex(@"^(\d+d)?(\d+h)?(\d+m)?(\d+s)?$");
			var timeMatch = timeRegex.Match(timeString);
			if (!timeMatch.Success)
			{
				ctx.replyPrivate("Time needs to be in the following format: [<num>d][<num>h][<num>m][<num>s]");
				ctx.replyPrivate("Examples: 10m30s\n5h\n1d\n1d6h");
				return;
			}
			var span = new TimeSpan();
			TimeSpan tmpSpan;
			if (TimeSpan.TryParseExact(timeMatch.Groups[1].Value,
				"d'd'",
				null,
				out tmpSpan))
				span += tmpSpan;
			if (TimeSpan.TryParseExact(timeMatch.Groups[2].Value,
				"h'h'",
				null,
				out tmpSpan))
				span += tmpSpan;

			if (TimeSpan.TryParseExact(timeMatch.Groups[3].Value,
				"m'm'",
				null,
				out tmpSpan))
				span += tmpSpan;
			if (TimeSpan.TryParseExact(timeMatch.Groups[4].Value,
				"s's'",
				null,
				out tmpSpan))
				span += tmpSpan;

			var question = args[1];
			var answers = new List<string>(args[2].Split(','));

			var endTime = DateTime.Now + span;
			try
			{
				StartVoting(ctx.getSenderInfo(), question, answers, endTime);
				_log.WriteToLog("Information", String.Format(
					"{0} started a voting: {1}. End Date is: {2}", ctx.getSenderNick(), question, endTime));
			}
			catch (InvalidOperationException e)
			{
				ctx.replyPrivate(e.Message);
				_log.WriteToLog("Error", String.Format(
					"{0} tried starting a voting: {1}. But: {2}", ctx.getSenderNick(), question, e.Message));
			}
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

		private void endVotingCommand(MessageContext ctx, string text, string commandArgs)
		{
			int index;
			if (commandArgs == null || !int.TryParse(commandArgs, out index))
			{
				ctx.replyPrivate(string.Format(
					"The format for ending a vote is: {0}endvote <id>", CommandManager.ACTIVATOR));
				return;
			}
			try
			{
				EndVoting(ctx.getSenderInfo(), index - 1);
			}
			catch (ArgumentOutOfRangeException e)
			{
				if (e.ParamName == "id")
				{
					ctx.replyPrivate(string.Format("There is no voting with the id {0}", index));
				}
				else

					throw;
			}
			catch (InvalidOperationException e)
			{
				ctx.replyPrivate(e.Message);
			}
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

		private void voteCommand(MessageContext ctx, string text, string commandArgs)
		{
			var args = commandArgs != null
				? commandArgs.Split(' ')
				: new string[0];
			if (args.Length < 2)
			{
				ctx.reply(string.Format("Format: /msg {0} vote <id> <answer>", nick));
				ctx.reply(string.Format("You can check the running votings with /msg {0} listvotings", nick));
				return;
			}
			int index;
			var answer = args[1];
			if (!int.TryParse(args[0], out index))
			{
				ctx.reply("id must be a number");
				return;
			}
			try
			{
				Vote(ctx.getSenderInfo(), index - 1, answer);
			}
			catch (ArgumentOutOfRangeException e)
			{
				switch (e.ParamName)
				{
				case "id":
					ctx.reply(string.Format("There is no voting with the id {0}", index));
					break;
				case "answer":
					ctx.reply(string.Format("The voting {0} has no answer {1}", index, answer));
					break;
				default:
					throw;
				}
			}
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

		private void removeVoteCommand(MessageContext ctx,
			string text,
			string commandArgs)
		{
			int index;
			if (commandArgs == null || !int.TryParse(commandArgs, out index))
			{
				ctx.reply(string.Format("The format for removing your vote is: /msg {0} removevote <id>", nick));
				return;
			}
			try
			{
				RemoveVote(ctx.getSenderInfo(), index - 1);
			}
			catch (ArgumentOutOfRangeException e)
			{
				if (e.ParamName == "id")
				{
					ctx.reply(string.Format("There is no voting with the id {0}", index));
				}
				else

					throw;
			}
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

		private void listVotingsCommand(MessageContext ctx, string text, string commandArgs)
		{
			if (Votings.Count == 0)
			{
				ctx.reply("There are currently no votings running");
			}
			foreach (var voting in Votings.Values)
			{
				ctx.reply(string.Format("{0} - {1}", voting._iIndex + 1, voting._sQuestion));
				ctx.reply("Answers:");
				foreach (var answer in voting._slAnswers)
					ctx.reply(string.Format("    {0}", answer));
				ctx.reply(string.Format("Voting runs until {0}", voting._dtEndTime));
			}
		}
    }
}
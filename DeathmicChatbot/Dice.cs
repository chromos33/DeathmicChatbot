using System;
using DeathmicChatbot;

namespace DeathmicChatbot
{
	public class Dice
	{
		public Dice (CommandManager cmds)
		{
			cmds.setPublicCommand("roll", Roll);
		}
		private static void Roll(MessageContext ctx,
			string text,
			string commandArgs)
		{
			var regex = new Regex(@"(^\d+)[wWdD](\d+$)");
			if (!regex.IsMatch(commandArgs))
			{
				ctx.reply(String.Format(
						"Error: Invalid roll request: {0}.",
						commandArgs));
			}
			else
			{
				var match = regex.Match(commandArgs);

				UInt64 numberOfDice;
				UInt64 sidesOfDice;

				try
				{
					sidesOfDice = Convert.ToUInt64(match.Groups[2].Value);
					numberOfDice = Convert.ToUInt64(match.Groups[1].Value);
				}
				catch (OverflowException)
				{
					ctx.reply("Error: Result could make the server explode. Get real, you maniac.");
					return;
				}

				if (numberOfDice == 0 || sidesOfDice == 0)
				{
					ctx.reply(string.Format("Error: Can't roll 0 dice, or dice with 0 sides."));
					return;
				}

				if (sidesOfDice >= Int32.MaxValue)
				{
					ctx.reply(string.Format(
							"Error: Due to submolecular limitations, a die can't have more than {0} sides.",
							Int32.MaxValue - 1));
					return;
				}

				UInt64 sum = 0;

				var random = new Random();

				var max = numberOfDice * sidesOfDice;
				if (max / numberOfDice != sidesOfDice)
				{
					ctx.reply("Error: Result could make the server explode. Get real, you maniac.");
					return;
				}

				if (numberOfDice > 100000000)
				{
					ctx.reply("Seriously? ... I'll try. But don't expect the result too soon. It's gonna take me a while.");
				}

				for (UInt64 i = 0; i < numberOfDice; i++)
					sum += (ulong) random.Next(1, Convert.ToInt32(sidesOfDice) + 1);

				ctx.reply(String.Format("{0}: {1}",
						commandArgs,
						sum));
			}
		}
	}
}


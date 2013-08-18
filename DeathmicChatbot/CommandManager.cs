using System.Collections.Generic;
using Sharkbite.Irc;

namespace DeathmicChatbot
{
	public class CommandManager
	{
		private const string ACTIVATOR = "!";
		private readonly Dictionary<string, PublicCommand> _public_commands;
		private readonly Dictionary<string, PrivateCommand> _private_commands;

		public delegate void PublicCommand(UserInfo user,string channel,string text,string commandArgs);

		public delegate void PrivateCommand(UserInfo user,string text,string commandArgs);

		public CommandManager()
		{
			_public_commands = new Dictionary<string, PublicCommand>();
			_private_commands = new Dictionary<string, PrivateCommand>();
		}
		/* Sets a public command
		 * 
		 * Args:
		 * 
		 * name - Name of the command
		 * callback - Delegate to call
		 * overwrite - If true an already existing command with that name will be overwritten.
		 * 
		 * Returns:
		 * 
		 * true if the command was set, false if there was already an command with that name.
		 */
		public bool SetCommand(string name, PublicCommand callback, bool overwrite = false)
		{
			if (!_public_commands.ContainsKey(name) || overwrite)
			{
				_public_commands[name] = callback;
				return true;
			}
			return false;
		}
		/* Sets a private command
		 * 
		 * Args:
		 * 
		 * name - Name of the command
		 * callback - Delegate to call
		 * overwrite - If true an already existing command with that name will be overwritten.
		 * 
		 * Returns:
		 * 
		 * true if the command was set, false if there was already an command with that name.
		 */
		public bool SetCommand(string name, PrivateCommand callback, bool overwrite = false)
		{
			if (!_private_commands.ContainsKey(name) || overwrite)
			{
				_private_commands[name] = callback;
				return true;
			}
			return false;
		}
		/* Unsets a command
		 * 
		 * Args:
		 * 
		 * name - Name of the command
		 * 
		 * Returns:
		 * 
		 * true if the command was unset, false if there was no such command.
 		 */
		public bool UnsetCommand(string name)
		{
			if (!_public_commands.ContainsKey(name))
			{
				_public_commands.Remove(name);
				return true;
			}
			return false;
		}
		/* Checks a string for a public command and executes if found
		 * 
		 * Args:
		 * 
		 * user: The user that wrote the message
		 * text: The message that is being checked
		 * 
		 * Returns:
		 * 
		 * true if a command was found and executed, false if the activator was not present
		 * or no command with that name was found.
		 */
		public bool CheckCommand(UserInfo user, string channel, string text, 
		                          bool isPrivate=false)
		{
			if (!text.StartsWith(ACTIVATOR) && !isPrivate)
				return false;
			string commandString = "";
			if (text.StartsWith(ACTIVATOR))
			{
				commandString = text.Remove(0, 1);
			}
			else
			{
				commandString = text;
			}
			int commandEnd = commandString.IndexOf(' ');
			string command;
			string commandArgs;

			if (commandEnd != -1)
			{
				command = commandString.Remove(commandEnd);
				commandArgs = commandString.Remove(0, commandEnd + 1);
			}
			else
			{
				command = commandString;
				commandArgs = null;
			}

			if (_public_commands.ContainsKey(command))
			{
				_public_commands[command](user, channel, text, commandArgs);
				return true;
			}
			else if (isPrivate && _private_commands.ContainsKey(command))
			{
				_private_commands[command](user, text, commandArgs);
				return true;
			}
			return false;
		}
	}
}
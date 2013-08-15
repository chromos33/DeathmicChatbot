using System.Collections.Generic;
using Sharkbite.Irc;

namespace DeathmicChatbot
{
	public class CommandManager
	{
		private static string activator = "!";
		private Dictionary<string, Command> commands;
		
		public delegate void Command(UserInfo user, string channel, string text, string command_args);
		
		public CommandManager()
		{
			this.commands = new Dictionary<string, CommandManager.Command>();
		}
		
		/* Sets a command
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
		public bool setCommand(string name, Command callback, bool overwrite=false)
		{
			if (!this.commands.ContainsKey(name) || overwrite)
			{
				this.commands[name] = callback;
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
		public bool unsetCommand(string name)
		{
			if (!this.commands.ContainsKey(name))
			{
				this.commands.Remove(name);
				return true;
			}
			return false;
		}
		
		/* Checks a string for a command and executes if found
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
		public bool checkCommand(UserInfo user, string channel, string text)
		{
			if (text.StartsWith(activator))
			{
				string command_string = text.Remove(0, 1);
				int command_end = command_string.IndexOf(' ');
				string command;
				string command_args;
				if (command_end != -1)
				{
					command = command_string.Remove(command_end);
					command_args = command_string.Remove(0, command_end + 1);
				}
				else
				{
					command = command_string;
					command_args = null;
				}
				
				if (this.commands.ContainsKey(command))
				{
					this.commands[command](user, channel, text, command_args);
				}
			}
			return false;
		}
	}
}

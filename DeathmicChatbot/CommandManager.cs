using System.Collections.Generic;
using Sharkbite.Irc;

namespace DeathmicChatbot
{
    public class CommandManager
    {
        private const string ACTIVATOR = "!";
        private readonly Dictionary<string, Command> _commands;

        public delegate void Command(UserInfo user, string channel, string text, string commandArgs);

        public CommandManager()
        {
            _commands = new Dictionary<string, Command>();
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

        public bool SetCommand(string name, Command callback, bool overwrite = false)
        {
            if (!_commands.ContainsKey(name) || overwrite)
            {
                _commands[name] = callback;
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
            if (!_commands.ContainsKey(name))
            {
                _commands.Remove(name);
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

        public bool CheckCommand(UserInfo user, string channel, string text)
        {
            if (text.StartsWith(ACTIVATOR))
            {
                string commandString = text.Remove(0, 1);
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

                if (_commands.ContainsKey(command))
                {
                    _commands[command](user, channel, text, commandArgs);
                }
            }
            return false;
        }
    }
}
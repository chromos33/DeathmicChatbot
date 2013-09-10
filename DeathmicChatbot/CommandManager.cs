using System.Collections.Generic;
using Sharkbite.Irc;

namespace DeathmicChatbot
{
    public class CommandManager
    {
        public const string ACTIVATOR = "!";
        private readonly Dictionary<string, PublicCommand> _publicCommands;
        private readonly Dictionary<string, PrivateCommand> _privateCommands;

        public delegate void PublicCommand(UserInfo user, string channel, string text, string commandArgs);

        public delegate void PrivateCommand(UserInfo user, string text, string commandArgs);

        public CommandManager()
        {
            _publicCommands = new Dictionary<string, PublicCommand>();
            _privateCommands = new Dictionary<string, PrivateCommand>();
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
            if (!_publicCommands.ContainsKey(name) || overwrite)
            {
                _publicCommands[name] = callback;
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
            if (!_privateCommands.ContainsKey(name) || overwrite)
            {
                _privateCommands[name] = callback;
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
            if (!_publicCommands.ContainsKey(name))
            {
                _publicCommands.Remove(name);
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
                                 bool isPrivate = false)
        {
            if (!text.StartsWith(ACTIVATOR) && !isPrivate)
                return false;
            var commandString = text.StartsWith(ACTIVATOR) ? text.Remove(0, 1) : text;
            var commandEnd = commandString.IndexOf(' ');
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

            if (_publicCommands.ContainsKey(command))
            {
                _publicCommands[command](user, channel, text, commandArgs);
                return true;
            }

            if (isPrivate && _privateCommands.ContainsKey(command))
            {
                _privateCommands[command](user, text, commandArgs);
                return true;
            }

            return false;
        }
    }
}
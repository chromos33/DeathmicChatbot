#region Using

using System;
using System.Collections.Generic;
using Sharkbite.Irc;

#endregion


namespace DeathmicChatbot
{
    public class CommandManager
    {
        #region Delegates

        public delegate void PrivateCommand(
            UserInfo user, string text, string commandArgs);

        public delegate void PublicCommand(
            UserInfo user, string channel, string text, string commandArgs);

        #endregion

        public const string ACTIVATOR = "!";
        private readonly Dictionary<string, PrivateCommand> _privateCommands;
        private readonly Dictionary<string, PublicCommand> _publicCommands;

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

        public void SetCommand(string name,
                               PublicCommand callback,
                               bool overwrite = false)
        {
            if (!_publicCommands.ContainsKey(name) || overwrite)
                _publicCommands[name] = callback;
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

        public void SetCommand(string name,
                               PrivateCommand callback,
                               bool overwrite = false)
        {
            if (!_privateCommands.ContainsKey(name) || overwrite)
                _privateCommands[name] = callback;
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

        public bool CheckCommand(UserInfo user,
                                 string channel,
                                 string text,
                                 bool isPrivate = false)
        {
            if (!text.StartsWith(ACTIVATOR, StringComparison.Ordinal) &&
                !isPrivate)
                return false;

            var commandString = text.StartsWith(ACTIVATOR,
                                                StringComparison.Ordinal)
                                    ? text.Remove(0, 1)
                                    : text;

            var iCommandEndIndex = commandString.IndexOf(' ');
            var command = iCommandEndIndex != -1
                              ? commandString.Remove(iCommandEndIndex)
                              : commandString;
            var commandArgs = iCommandEndIndex != -1
                                  ? commandString.Remove(0, iCommandEndIndex + 1)
                                  : null;

            if (ExecuteIfPublicCommand(user, channel, text, command, commandArgs))
                return true;

            if (ExecuteIfPrivateCommand(user,
                                        text,
                                        isPrivate,
                                        command,
                                        commandArgs))
                return true;

            return false;
        }

        private bool ExecuteIfPrivateCommand(UserInfo user,
                                             string text,
                                             bool isPrivate,
                                             string command,
                                             string commandArgs)
        {
            if (isPrivate && _privateCommands.ContainsKey(command))
            {
                _privateCommands[command](user, text, commandArgs);
                return true;
            }
            return false;
        }

        private bool ExecuteIfPublicCommand(UserInfo user,
                                            string channel,
                                            string text,
                                            string command,
                                            string commandArgs)
        {
            if (_publicCommands.ContainsKey(command))
            {
                _publicCommands[command](user, channel, text, commandArgs);
                return true;
            }
            return false;
        }
    }
}
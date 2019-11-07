using BobDeathmic.ChatCommands.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ChatCommands
{
    public class Help : IfCommand
    {
        public string Trigger { get { return "!help"; } }

        public string Description { get { return "Listet Commands auf"; } }

        public string Category { get { return "General"; } }

        public string Alias => "!help";

        private List<IfCommand> Commands;

        public bool ChatSupported(ChatType chat)
        {
            return true;
        }
        public Help(List<IfCommand> Commands)
        {
            this.Commands = Commands;
        }
        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            return CommandEventType.None;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            if (args["message"].ToLower().StartsWith(Trigger))
            {
                ChatType type = getChatType(args["type"]);
                return getHelp(type);
            }
            return string.Empty;
        }

        public async Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return string.Empty;
        }

        private ChatType getChatType(string input)
        {
            switch(input)
            {
                case "twitch":
                    return ChatType.Twitch;
                case "discord":
                    return ChatType.Discord;
            }
            return ChatType.NotImplemented;
        }

        public string getHelp(ChatType type)
        {
            string currentCategory = string.Empty;
            string sCommandMessage = "Viele Befehle haben einen help parameter (!Befehl help)" + Environment.NewLine;

            //Manual Implementation. Would otherwise need redundant rework of Commands all holding reference to UserManager ...
            sCommandMessage += Environment.NewLine;
            sCommandMessage += Environment.NewLine;
            sCommandMessage += $"[WebInterface]{Environment.NewLine}";
            sCommandMessage += "!WebInterfaceLink (!wil) : Gibt den Link zum Webinterface zurück" + Environment.NewLine;
            foreach (IfCommand command in Commands.Where(x => x.ChatSupported(type)))
            {
                if (currentCategory != command.Category)
                {
                    sCommandMessage += Environment.NewLine;
                    sCommandMessage += Environment.NewLine;
                    currentCategory = command.Category;
                    sCommandMessage += $"[{command.Category}]{Environment.NewLine}";
                }
                sCommandMessage += command.Trigger + " : " + command.Description + Environment.NewLine;
            }
            return sCommandMessage;
        }

        public ChatCommandOutput execute(Dictionary<string, string> message, IServiceScopeFactory scopefactory)
        {
            throw new NotImplementedException();
        }
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }

        public ChatCommandOutput execute(ChatCommandArguments args, IServiceScopeFactory scopefactory)
        {
            throw new NotImplementedException();
        }
    }
}

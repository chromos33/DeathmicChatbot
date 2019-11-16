using BobDeathmic.ChatCommands.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data.Enums;
using BobDeathmic.Eventbus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ChatCommands
{
    public class Help : ICommand
    {
        public string Trigger { get { return "!help"; } }

        public string Description { get { return "Listet Commands auf"; } }

        public string Category { get { return "General"; } }

        public string Alias => "!help";

        private List<ICommand> Commands;

        public bool ChatSupported(ChatType chat)
        {
            return true;
        }
        public Help(List<ICommand> Commands)
        {
            this.Commands = Commands;
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

            sCommandMessage += "Dev";
            sCommandMessage += Environment.NewLine;
            sCommandMessage += Environment.NewLine;
            sCommandMessage += $"[WebInterface]{Environment.NewLine}";
            sCommandMessage += "!WebInterfaceLink (!wil) : Gibt den Link zum Webinterface zurück" + Environment.NewLine;
            foreach (ICommand command in Commands.Where(x => x.ChatSupported(type)))
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
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopefactory)
        {
            ChatCommandOutput output = new ChatCommandOutput();
            output.ExecuteEvent = true;
            output.Type = Eventbus.EventType.CommandResponseReceived;
            output.EventData = new CommandResponseArgs(
                args.Type,
                getHelp(args.Type),
                MessageType.PrivateMessage,
                EventType.CommandResponseReceived,
                args.Sender,
                args.ChannelName
              );
            return output;
        }
    }
}

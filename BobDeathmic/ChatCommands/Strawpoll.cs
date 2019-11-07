using BobDeathmic.Args;
using BobDeathmic.ChatCommands.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data.Enums;
using BobDeathmic.Data.Enums.Stream;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BobDeathmic.ChatCommands
{
    public class Strawpoll : IfCommand
    {
        public string Trigger => "!strawpoll";
        public string Alias => "!straw";

        public string Description => "Erstellt einen Strawpoll (!strawpoll q='Frage' o='Option1|Option2' [m='true/false'])";

        public string Category => "stream";


        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(Alias))
            {
                return CommandEventType.Strawpoll;
            }
            return CommandEventType.None;
        }

        public ChatCommandOutput execute(ChatCommandArguments args, IServiceScopeFactory scopefactory)
        {
            return null;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }

        public async Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }
        private StreamTitleChangeArgs PrepareStreamTitleChange(string StreamName, string Message)
        {
            var arg = new StreamTitleChangeArgs();
            arg.StreamName = StreamName;
            arg.Type = StreamProviderTypes.Twitch;
            if (Message.StartsWith("!stream"))
            {
                var questionRegex = Regex.Match(Message, @"game=\'(.*?)\'");
                var GameRegex = Regex.Match(Message, @"game=\'(.*?)\'");
                string Game = "";
                if (GameRegex.Success)
                {
                    Game = GameRegex.Value;
                }
                var TitleRegex = Regex.Match(Message, @"title=\'(.*?)\'");
                string Title = "";
                if (TitleRegex.Success)
                {
                    Title = TitleRegex.Value;
                }
                arg.Game = Game;
                arg.Title = Title;
            }
            else
            {
                if (Message.StartsWith("!game"))
                {
                    arg.Game = Message.Replace("!game ", "");
                    arg.Title = "";
                }
                if (Message.StartsWith("!title"))
                {
                    arg.Title = Message.Replace("!title ", "");
                    arg.Game = "";
                }
            }
            return arg;
        }
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }
    }
}

using BobDeathmic.Args;
using BobDeathmic.ChatCommands.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data.Enums;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.JSONObjects;
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
            if(args.elevatedPermissions)
            {
                var strawpollargs = extractStrawpollData(args.Message);
                strawpollargs.StreamName = args.ChannelName;
                switch (args.Type)
                {
                    case ChatType.Discord:
                        throw new NotImplementedException("Discord has not yet been implemented for Strawpollcommand");
                        break;
                    case ChatType.Twitch:
                        strawpollargs.Type = StreamProviderTypes.Twitch;
                        break;
                    default:
                        return null;

                }

                return new ChatCommandOutput() {
                    Type = Eventbus.EventType.StrawPollRequested,
                    ExecuteEvent = true,
                    EventData = strawpollargs
                };
            }
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
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }
        private StrawPollRequestEventArgs extractStrawpollData(string message)
        {
            var questionRegex = Regex.Match(message, @"q=\'(.*?)\'");
            var optionsRegex = Regex.Match(message, @"o=\'(.*?)\'");
            var multiRegex = Regex.Match(message, @"m=\'(.*?)\'");
            StrawPollPostData values = null;
            string question = "";
            List<string> Options = new List<string>();
            bool multi = false;
            if (questionRegex.Success && optionsRegex.Success)
            {
                Options = optionsRegex.Value.Replace("o='", "").Replace("'", "").Split('|').ToList();
                question = questionRegex.Value.Replace("q='", "").Replace("'", "");
                if (multiRegex.Success)
                {
                    switch (multiRegex.Value)
                    {
                        case "true":
                        case "j":
                            multi = true;
                            break;
                    }
                }
            }
            else
            {
                // for the forgetfull
                string[] parameters = message.Replace("!strawpoll ", "").Split('|');
                question = parameters[0];
                for (var i = 1; i < parameters.Count(); i++)
                {
                    Options.Add(parameters[i]);
                }
            }
            StrawPollRequestEventArgs arg = new StrawPollRequestEventArgs();
            arg.Answers = Options.ToArray();
            arg.Question = question.Trim();
            arg.multiple = multi;
            return arg;
        }
    }
}

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
    public class TwitchStreamTitle : ICommand
    {
        public string Trigger => "!stream";
        public string Alias => "!cst";

        public string Description => "Ändert den StreamTitel (!stream game='Bla' title='Bla')";

        public string Category => "stream";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(Alias))
            {
                return CommandEventType.TwitchTitle;
            }
            return CommandEventType.None;
        }

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopefactory)
        {
            return null;
            /*
            if (message["message"].ToLower().StartsWith(Trigger) || message["message"].ToLower().StartsWith(alias))
            {
                return CommandEventType.TwitchTitle;
            }
            return CommandEventType.None;
            */
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
    }
    public class Game : ICommand
    {
        public string Trigger => "!game";
        public string Alias => "!game";

        public string Description => "Ändert das Stream Spiel";

        public string Category => "stream";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(Alias))
            {
                return CommandEventType.TwitchTitle;
            }
            return CommandEventType.None;
        }

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopefactory)
        {
            if (!args.elevatedPermissions)
            {
                return null;
            }
            return new ChatCommandOutput()
            {
                ExecuteEvent = true,
                Type = Eventbus.EventType.StreamTitleChangeRequested,
                EventData = TwitchStreamTitleHelper.PrepareStreamTitleChange(args.ChannelName, args.Message)
            };
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
    }
    public class Title : ICommand
    {
        public string Trigger => "!title";
        public string Alias => "!titel";

        public string Description => "Ändert den Stream Titel";

        public string Category => "stream";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(Alias))
            {
                return CommandEventType.TwitchTitle;
            }
            return CommandEventType.None;
        }

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopefactory)
        {
            if(!args.elevatedPermissions)
            {
                return null;
            }
            return new ChatCommandOutput()
            {
                ExecuteEvent = true,
                Type = Eventbus.EventType.StreamTitleChangeRequested,
                EventData = TwitchStreamTitleHelper.PrepareStreamTitleChange(args.ChannelName,args.Message)
            };
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
    }
    public static class TwitchStreamTitleHelper
    {
        public static StreamTitleChangeArgs PrepareStreamTitleChange(string StreamName, string Message)
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
    }
}

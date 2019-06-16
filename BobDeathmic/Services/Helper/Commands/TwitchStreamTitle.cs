using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Helper.Commands
{
    public class TwitchStreamTitle : IfCommand
    {
        public string Trigger => "!stream";
        public string alias => "!cst";

        public string Description => "Ändert den StreamTitel (!stream game='Bla' title='Bla')";

        public string Category => "stream";

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(alias))
            {
                return CommandEventType.TwitchTitle;
            }
            return CommandEventType.None;   
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }

        public async Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }
    }
    public class Game : IfCommand
    {
        public string Trigger => "!game";
        public string alias => "!game";

        public string Description => "Ändert das Stream Spiel";

        public string Category => "stream";

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(alias))
            {
                return CommandEventType.TwitchTitle;
            }
            return CommandEventType.None;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }

        public async Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }
    }
    public class Title : IfCommand
    {
        public string Trigger => "!title";
        public string alias => "!title";

        public string Description => "Ändert den Stream Titel";

        public string Category => "stream";

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(alias))
            {
                return CommandEventType.TwitchTitle;
            }
            return CommandEventType.None;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }

        public async Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }
    }
}

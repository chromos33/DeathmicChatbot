using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Helper.Commands
{
    public class Strawpoll : IfCommand
    {
        public string Trigger => "!strawpoll";
        public string alias => "!straw";

        public string Description => "Erstellt einen Strawpoll (!strawpoll q='Frage' o='Option1|Option2' [m='true/false'])";

        public string Category => "stream";

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(alias))
            {
                return CommandEventType.Strawpoll;
            }
            return CommandEventType.None;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args)
        {
            return "";
        }
    }
}

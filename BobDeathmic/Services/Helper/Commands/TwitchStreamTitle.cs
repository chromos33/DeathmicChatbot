using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Helper.Commands
{
    public class TwitchStreamTitle : IfCommand
    {
        public string Trigger => "!changestreamtitle";

        public string Description => "Ändert den StreamTitel (!changestreamtitle game='Bla' title='Bla')";

        public string Category => "stream";

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger))
            {
                return CommandEventType.TwitchTitle;
            }
            return CommandEventType.None;   
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args)
        {
            return "";
        }
    }
}

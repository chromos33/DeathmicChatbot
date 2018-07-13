using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Helper.Commands
{
    public class WebInterfaceLink : IfCommand
    {
        public string Trigger { get { return "!WebInterfaceLink"; } }

        public string Description { get { return "Gibt den Link zum Webinterface zurück"; } }

        public string Category { get { return "WebInterface"; } }

        public async Task<string> ExecuteCommandIfAplicable(Dictionary<string, string> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger))
            {

            }
            return "";
        }
    }
}

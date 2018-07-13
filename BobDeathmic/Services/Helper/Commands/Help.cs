using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Helper.Commands
{
    public class Help : IfCommand
    {
        public string Trigger { get { return "!devhelp"; } }

        public string Description { get { return "Listet Commands auf"; } }

        public string Category { get { return "General"; } }
        private string sCommandMessage { get; set; }

        public async Task<string> ExecuteCommandIfAplicable(Dictionary<String, String> args)
        {
            if (args["message"].ToLower().StartsWith(Trigger))
            {
                return sCommandMessage;
            }
            return "";
        }
        public void PopulateCommandList(string provider = "discord")
        {
            //Should ever only be called once (Per Help Command init)
            if (sCommandMessage == "" || sCommandMessage == null)
            {
                string currentCategory = "";
                sCommandMessage = "Viele Befehle haben einen help parameter (!befehl help)" + Environment.NewLine;

                //Manual Implementation. Would otherwise need redundant rework of Commands all holding reference to UserManager ...
                sCommandMessage += Environment.NewLine;
                sCommandMessage += Environment.NewLine;
                sCommandMessage += $"[WebInterface]{Environment.NewLine}";
                sCommandMessage += "!WebInterfaceLink : Gibt den Link zum Webinterface zurück" + Environment.NewLine;
                foreach (IfCommand tempcommand in Services.Helper.CommandBuilder.BuildCommands(provider, true))
                {
                    if (currentCategory != tempcommand.Category)
                    {
                        sCommandMessage += Environment.NewLine;
                        sCommandMessage += Environment.NewLine;
                        currentCategory = tempcommand.Category;
                        sCommandMessage += $"[{tempcommand.Category}]{Environment.NewLine}";
                    }
                    sCommandMessage += tempcommand.Trigger + " : " + tempcommand.Description + Environment.NewLine;
                }
            }
        }
    }
}

using BobCore.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord.WebSocket;
using Discord;
namespace BobCore.Commands
{
    class Help : IFCommand
    {
        //TODO: Refactor commands to return trigger directly in getter
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "List of Commands"; } }
        public string category { get { return "General"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return "!help"; } }
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;

        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                var results = from type in Assembly.GetAssembly(typeof(Commands.IFCommand)).GetTypes()
                              where typeof(Commands.IFCommand).IsAssignableFrom(type)
                              select type;
                string output = "";
                List<dynamic> Commands = new List<dynamic>();
                foreach (var type in results)
                {
                    if (type.Name != "IFCommand")
                    {
                        Type elementType = Type.GetType(type.Name);
                        Commands.Add(Activator.CreateInstance(type));
                    }
                }
                string currentCategory = "";
                output += "Viele Befehle haben einen help parameter (!befehl help) der die Paramter anzeigt sofern sie komplexer sind wie !addstream streamname oder nur !streamcheck" + Environment.NewLine;
                foreach (var command in Commands.OrderBy(x => x.category).ToList())
                {
                    if (Useful_Functions.HasProperty(command, "UserRestriction"))
                    {
                        var test = command.GetType().GetField("UserRestriction");
                        if (command.UserRestriction.Contains(username.ToLower()))
                        {
                            if (currentCategory != command.category)
                            {
                                output += Environment.NewLine;
                                output += Environment.NewLine;
                                currentCategory = command.category;
                                output += $"[{command.category}]{Environment.NewLine}";
                            }
                            output += command.sTrigger + " : " + command.description + Environment.NewLine;
                        }
                    }
                    else
                    {
                        if (currentCategory != command.category)
                        {
                            output += Environment.NewLine;
                            output += Environment.NewLine;
                            currentCategory = command.category;
                            output += $"[{command.category}]{Environment.NewLine}";
                        }
                        output += command.sTrigger + " : " + command.description + Environment.NewLine;
                    }
                }
                return output;
            }
            return "";
        }

        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                UserList = _DataList;
            }
            if (type == "DataClasses.Stream")
            {
                StreamList = _DataList;
            }
        }
    }
    class RemoveOldUsers : IFCommand
    {
        private static string[] Requirements = { "User", "Present", "Client" };
        public string description { get { return "User die nicht im auf dem Discord Server sidn werden gelöscht"; } }
        public List<string> UserRestriction = new List<string> { "chromos33", "deathmic" };
        public string category { get { return "General"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return "!removeoldusers"; } }

        public List<DataClasses.User> lUser;
        public Discord.WebSocket.DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                if (UserRestriction.Contains(username.ToLower()))
                {
                    List<DataClasses.User> newList = new List<DataClasses.User>();
                    foreach (var user in Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users)
                    {
                        var newuser = lUser.Where(x => x.Name.ToLower() == user.Username.ToLower()).FirstOrDefault();
                        if (newuser != null)
                        {
                            newList.Add(newuser);
                        }
                        else
                        {
                            user.SendMessageAsync("!addmyuser anwenden um deinen Nutzer bei mir zu registrieren");
                        }
                    }
                    lUser = newList;
                    Administrative.XMLFileHandler.writeFile(lUser, "Users");
                }
                else
                {
                    return "Forbidden";
                }

            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
}

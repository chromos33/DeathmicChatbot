using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BobCore.Commands
{
    class DebugUser : IFCommand
    {
        private string[] Requirements = { "Client" };
        public string description { get { return "Discord User Debug"; } }
        public string category { get { return "User"; } }
        public List<string> UserRestriction = new List<string> { "chromos33" };
        public bool @private { get { return true; } }
        public DiscordSocketClient Client;
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return "!debuguser"; } }
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;

        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                if (UserRestriction.Contains(username.ToLower()))
                {
                    List<string> Users = new List<string>();
                    foreach (var user in Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(u => u.Status != Discord.UserStatus.Offline))
                    {
                        Users.Add(user.Username);
                        
                    }
                }
            }
            return "";
        }

        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
}

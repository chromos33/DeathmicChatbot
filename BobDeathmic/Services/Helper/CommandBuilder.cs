using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Helper
{
    public class CommandBuilder
    {
        public static List<IfCommand> BuildCommands(string source, bool excludeHelp = false)
        {
            List<IfCommand> Commands = new List<IfCommand>();
            
            if (!excludeHelp && CommandBuilder.CommandActiveInSource(source, "Help"))
            {
                Commands.Help tmp = new Commands.Help();
                tmp.PopulateCommandList(source);
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "ChangeStreamTitle"))
            {
                BobDeathmic.Services.Helper.Commands.TwitchStreamTitle tmp = new Commands.TwitchStreamTitle();
                Commands.Add(tmp);
            }
            /*
            if (CommandBuilder.CommandActiveInSource(source, "uptime"))
            {
                Commands.Add(new Commands.Stream.UpTime());
            }
            */


            return Commands;
        }
        public static bool CommandActiveInSource(string source, string commandname)
        {
            var config = new ConfigurationBuilder()
                         .SetBasePath(Environment.CurrentDirectory)
                         .AddJsonFile("appsettings.json", true, true)
                         .Build();
            var ConfigObject = config.Get<AppConfig>();
            Command setting = null;
            if ((setting = ConfigObject.Commands.Where(x => x.Name.ToLower() == commandname.ToLower()).FirstOrDefault()) != null)
            {
                if (source == "twitch")
                {
                    return setting.Twitch;
                }
                if (source == "discord")
                {
                    return setting.Discord;
                }
            }
            return false;
        }
    }
}

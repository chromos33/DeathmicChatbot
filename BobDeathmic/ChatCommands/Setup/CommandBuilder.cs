using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.ChatCommands.Setup;

namespace BobDeathmic.ChatCommands.Setup
{
    public class CommandBuilder
    {
        public static List<IfCommand> BuildCommands(string source, bool excludeHelp = false)
        {
            List<IfCommand> Commands = new List<IfCommand>();

            if (!excludeHelp && CommandBuilder.CommandActiveInSource(source, "Help"))
            {
                Help tmp = new Help();
                tmp.PopulateCommandList(source);
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "stream"))
            {
                TwitchStreamTitle tmp = new TwitchStreamTitle();
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "game"))
            {
                Game tmp = new Game();
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "title"))
            {
                Title tmp = new Title();
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "strawpoll"))
            {
                Strawpoll tmp = new Strawpoll();
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "registerraffle"))
            {
                RandomChatUserRegisterCommand tmp = new RandomChatUserRegisterCommand();
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "next"))
            {
                PickNextChatUserForNameCommand tmp = new PickNextChatUserForNameCommand();
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "nextrand"))
            {
                PickNextRandChatUserForNameCommand tmp = new PickNextRandChatUserForNameCommand();
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "list"))
            {
                ListRandUsersInListCommand tmp = new ListRandUsersInListCommand();
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "skip"))
            {
                SkipLastRandUserCommand tmp = new SkipLastRandUserCommand();
                Commands.Add(tmp);
            }
            if (CommandBuilder.CommandActiveInSource(source, "quote"))
            {
                QuoteCommand tmp = new QuoteCommand();
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

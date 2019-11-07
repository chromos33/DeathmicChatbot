using BobDeathmic.ChatCommands.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Commands
{
    public interface ICommandService
    {
        List<IfCommand> GetCommands(ChatType type);
        Task handleCommand(ChatCommandArguments args, ChatType chatType, string sender);
    }
}

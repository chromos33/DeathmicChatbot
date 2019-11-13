using BobDeathmic.ChatCommands.Args;
using BobDeathmic.Data.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ChatCommands.Setup
{
    public interface IfCommand
    {
        string Trigger { get; }
        string Description { get; }
        string Category { get; }
        string Alias { get; }
        bool ChatSupported(ChatType chat);



        //Replace with execute functions
        Task<string> ExecuteCommandIfApplicable(Dictionary<String, String> args, IServiceScopeFactory scopeFactory);
        Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory);
        Task<CommandEventType> EventToBeTriggered(Dictionary<String, String> args);
        Task<ChatCommandOutput> execute(ChatCommandArguments args, IServiceScopeFactory scopefactory);
        bool isCommand(string message);
    }
}

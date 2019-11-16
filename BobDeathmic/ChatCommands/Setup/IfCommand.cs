using BobDeathmic.ChatCommands.Args;
using BobDeathmic.Data.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ChatCommands.Setup
{
    public interface ICommand
    {
        string Trigger { get; }
        string Description { get; }
        string Category { get; }
        string Alias { get; }
        bool ChatSupported(ChatType chat);
        Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopefactory);
        bool isCommand(string message);
    }
}

using BobDeathmic.Services.Helper.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Helper
{
    public interface IfCommand
    {
        string Trigger { get; }
        string Description { get; }
        string Category { get; }
        Task<string> ExecuteCommandIfApplicable(Dictionary<String, String> args, IServiceScopeFactory scopeFactory);
        Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory);

        Task<CommandEventType> EventToBeTriggered(Dictionary<String, String> args);
    }
}

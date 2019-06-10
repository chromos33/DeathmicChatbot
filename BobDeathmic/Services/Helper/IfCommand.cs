using BobDeathmic.Services.Helper.Commands;
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
        Task<string> ExecuteCommandIfApplicable(Dictionary<String, String> args);

        Task<CommandEventType> EventToBeTriggered(Dictionary<String, String> args);
    }
}

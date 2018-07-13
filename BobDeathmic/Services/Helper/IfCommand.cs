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
        Task<string> ExecuteCommandIfAplicable(Dictionary<String, String> args);
    }
}

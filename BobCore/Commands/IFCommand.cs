using BobCore.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobCore.Commands
{
    interface IFCommand
    {
        string sTrigger { get; }
        string[] SARequirements { get; }
        bool @private { get; }
        string category { get; }
        string description { get; }
        // Use Useful_Functions.MessageParameters(message, sTrigger) to get Parameters from message
        string CheckCommandAndExecuteIfApplicable(string message, string username, string channel);
        // TODO: Find a way you can do add Objects without relying on absolute Lists
        void addRequiredList(dynamic _DataList, string type);
    }
}

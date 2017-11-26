using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobCore.StreamFunctions
{
    interface StreamChecker
    {
        void CheckOnlineStreams();
        void Start();
        void Start(List<DataClasses.internalStream> Streams);
        event EventHandler<StreamEventArgs> StreamOnline;
        event EventHandler<StreamEventArgs> StreamOffline;
        void AddSecurityVaultData(DataClasses.SecurityVault _vault);
    }
}

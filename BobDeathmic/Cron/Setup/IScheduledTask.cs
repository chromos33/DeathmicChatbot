using BobDeathmic;
using BobDeathmic.Cron.Setup;
using BobDeathmic.Services;
using BobDeathmic.Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Cron.Setup
{
    public interface IScheduledTask
    {
        string Schedule { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}

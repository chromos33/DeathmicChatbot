using BobDeathmic.Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Tasks
{
    public class EventCalendarTask : IScheduledTask
    {
        public string Schedule => "11 20 * * *";

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm"));
        }
    }
}

using BobDeathmic.Args;
using BobDeathmic.Eventbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Services
{
    public class EventBusTester : BackgroundService
    {
        private IEventBus _eventBus;
        public EventBusTester(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.StreamChanged += StreamChanged;
        }

        private void StreamChanged(object sender, StreamEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }

        }
    }
}

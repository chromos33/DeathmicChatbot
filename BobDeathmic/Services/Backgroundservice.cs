﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using TwitchLib.Api;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BobDeathmic.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace BobDeathmic.Services
{
    public abstract class BackgroundService : IHostedService, IDisposable
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts =
                                                       new CancellationTokenSource();

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
        public bool Active;

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            Active = true;
            // Store the task we're executing
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            // If the task is completed then return it, 
            // this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            // Otherwise it's running
            Active = false;
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite,
                                                              cancellationToken));
            }

        }

        public virtual void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}
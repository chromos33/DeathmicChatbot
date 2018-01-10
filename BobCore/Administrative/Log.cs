using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BobCore.Administrative
{
    public class DebugLogger
    {
        private readonly ILogger<DebugLogger> _logger;
        public DebugLogger(ILogger<DebugLogger> logger)
        {
            _logger = logger;
        }
        public void DoNotice(string msg)
        {
            _logger.LogDebug(1, msg);
        }
        
    }
}

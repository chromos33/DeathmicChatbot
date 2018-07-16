using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BobDeathmic.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IEnumerable<Microsoft.Extensions.Hosting.IHostedService> _microservices;
        public ServicesController(IEnumerable<Microsoft.Extensions.Hosting.IHostedService> microservices)
        {
            _microservices = microservices;
        }
        public IActionResult Index()
        {
            return NotFound();
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> ServiceControls()
        {
            return View();
        }
        [Authorize(Roles = "Dev,Admin")]
        public async Task<string> StartDiscord()
        {
            try
            { 
                await _microservices.Where(ms=>ms.GetType() == typeof(DiscordService)).FirstOrDefault()?.StartAsync(new System.Threading.CancellationToken(false));
            }catch(Exception ex)
            {
                Console.WriteLine("test");
            }
            
            return "Started";
        }
        [Authorize(Roles = "Dev,Admin")]
        public async Task<string> StopDiscord()
        {
            try
            {
                await _microservices.Where(ms => ms.GetType() == typeof(DiscordService)).FirstOrDefault()?.StopAsync(new System.Threading.CancellationToken(true));
            }
            catch (Exception ex)
            {
                Console.WriteLine("test");
            }
            return "Stopped";
        }
        [Authorize(Roles = "Dev,Admin")]
        public async Task<string> RestartDiscord()
        {
            try
            {
                await _microservices.Where(ms => ms.GetType() == typeof(DiscordService)).FirstOrDefault()?.StopAsync(new System.Threading.CancellationToken(true));
                await _microservices.Where(ms => ms.GetType() == typeof(DiscordService)).FirstOrDefault()?.StartAsync(new System.Threading.CancellationToken(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine("test");
            }
            return "Restart";
        }
        [Authorize(Roles = "Dev,Admin")]
        public async Task<string> StopTwitchRelay()
        {
            try
            {
                var test = _microservices.Where(ms => ms.GetType() == typeof(TwitchRelayCenter)).FirstOrDefault();
                await test?.StopAsync(new System.Threading.CancellationToken(true));
            }
            catch (Exception ex)
            {
                Console.WriteLine("test");
            }
            return "Stopped";
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<string> RestartTwitchRelay()
        {
            try
            {
                await _microservices.Where(ms => ms.GetType() == typeof(TwitchRelayCenter)).FirstOrDefault()?.StopAsync(new System.Threading.CancellationToken(true));
                GC.Collect();
                await _microservices.Where(ms => ms.GetType() == typeof(TwitchRelayCenter)).FirstOrDefault()?.StartAsync(new System.Threading.CancellationToken(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine("test");
            }
            return "Restart";
        }
        
    }
}
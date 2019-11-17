using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.Eventbus;
using BobDeathmic.JSONObjects;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Services
{
    public class StrawPollService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEventBus _eventBus;
        private static readonly HttpClient client = new HttpClient();
        public StrawPollService(IEventBus eventBus, IServiceScopeFactory scopeFactory)
        {
            _eventBus = eventBus;
            _scopeFactory = scopeFactory;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _eventBus.StrawPollRequested += StrawPollRequested;
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async void StrawPollRequested(object sender, StrawPollRequestEventArgs e)
        {
            if (e.Question.Count() > 1)
            {
                var values = new StrawPollPostData { title = e.Question, options = e.Answers, multi = e.multiple };
                var response = await client.PostAsJsonAsync("https://www.strawpoll.me/api/v2/polls", values);

                var responseString = await response.Content.ReadAsStringAsync();
                var StrawPollData = JsonConvert.DeserializeObject<StrawPollResponseData>(responseString);
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    Stream stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower().Equals(e.StreamName.ToLower())).FirstOrDefault();
                    _eventBus.TriggerEvent(EventType.RelayMessageReceived, new Args.RelayMessageArgs() { SourceChannel = stream.DiscordRelayChannel, StreamType = StreamProviderTypes.Twitch, TargetChannel = stream.StreamName, Message = StrawPollData.Url() });
                }
            }
            else
            {
                //Error Message you must have at least 2 options
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    Stream stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower().Equals(e.StreamName.ToLower())).FirstOrDefault();
                    _eventBus.TriggerEvent(EventType.RelayMessageReceived, new Args.RelayMessageArgs() { SourceChannel = stream.DiscordRelayChannel, StreamType = StreamProviderTypes.Twitch, TargetChannel = stream.StreamName, Message = "You need at least 2 Options" });
                }
            }
        }
    }
}

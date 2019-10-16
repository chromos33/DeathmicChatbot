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


            var questionRegex = Regex.Match(e.Message, @"q=\'(.*?)\'");
            var optionsRegex = Regex.Match(e.Message, @"o=\'(.*?)\'");
            var multiRegex = Regex.Match(e.Message, @"m=\'(.*?)\'");
            StrawPollPostData values = null;
            string question = "";
            List<string> Options = new List<string>();
            bool multi = false;
            if (questionRegex.Success && optionsRegex.Success)
            {
                Options = optionsRegex.Value.Replace("o='", "").Replace("'", "").Split('|').ToList();
                question = questionRegex.Value.Replace("q='", "").Replace("'", "");
                if (multiRegex.Success)
                {
                    switch (multiRegex.Value)
                    {
                        case "true":
                        case "j":
                            multi = true;
                            break;
                    }
                }
            }
            else
            {
                // for the forgetfull
                string[] parameters = e.Message.Replace("!strawpoll ", "").Split('|');
                question = parameters[0];
                for (var i = 1; i < parameters.Count(); i++)
                {
                    Options.Add(parameters[i]);
                }
            }
            if (Options.Count() > 1)
            {
                values = new StrawPollPostData { title = question, options = Options.ToArray(), multi = multi };
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

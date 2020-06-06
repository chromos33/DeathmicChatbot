using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.Enums.Relay;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.Eventbus;
using Microsoft.EntityFrameworkCore;
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
    public class MixerChecker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IEventBus _eventBus;
        private System.Timers.Timer _timer;
        private bool _inProgress = false;
        public MixerChecker(IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed += (sender, args) => CheckOnlineStreams();
            _timer.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task CheckOnlineStreams()
        {
            if (!_inProgress)
            {
                _inProgress = true;
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        HttpClient client = new HttpClient();
                        string baseUrl = "https://mixer.com/api/v1/channels/";
                        IQueryable<Stream> GetStreams()
                        {
                            return _context.StreamModels.AsQueryable().Where(x => x.Type == StreamProviderTypes.Mixer).Include(x => x.StreamSubscriptions).ThenInclude(x => x.User);
                        }
                        foreach (Stream stream in GetStreams())
                        {
                            string RequestLink = baseUrl + stream.StreamName;
                            var response = await client.GetAsync(RequestLink);
                            var responsestring = await response.Content.ReadAsStringAsync();
                            Regex regex = new Regex("\"description\":.*\"languageId\"");
                            responsestring = regex.Replace(responsestring, "\"languageId\"");
                            JSONObjects.MixerChannelInfo streamInfo = JsonConvert.DeserializeObject<JSONObjects.MixerChannelInfo>(responsestring);
                            if (streamInfo.online)
                            {
                                if (stream.StreamState == StreamState.NotRunning)
                                {
                                    SetStreamOnline();
                                    StreamStarted();
                                    void SetStreamOnline()
                                    {
                                        stream.Started = DateTime.Now;
                                        if (streamInfo.type != null)
                                        {
                                            stream.Game = streamInfo.type.name;
                                        }
                                        else
                                        {
                                            stream.Game = "Undefined";
                                        }
                                        stream.StreamState = StreamState.Started;

                                        stream.Url = $"https://mixer.com/{stream.StreamName}";
                                    }
                                    async Task StreamStarted()
                                    {
                                        int longDelayCounter = 0;
                                        foreach (string username in stream.GetActiveSubscribers())
                                        {
                                            longDelayCounter++;
                                            if (longDelayCounter == 5)
                                            {
                                                longDelayCounter = 0;
                                                await Task.Delay(2000);
                                            }
                                            _eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, new MessageArgs() { Message = stream.StreamStartedMessage(stream.Game, stream.Url), RecipientName = username });
                                            await Task.Delay(100);
                                        }
                                    }
                                }
                                else
                                {
                                    stream.StreamState = StreamState.Running;
                                }
                            }
                            else
                            {
                                SetStreamOffline();
                                void SetStreamOffline()
                                {
                                    stream.StreamState = StreamState.NotRunning;
                                }
                                void StreamStopped()
                                {
                                    // Implement when/if relay Integrated
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                        _inProgress = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    _inProgress = false;
                }
            }


        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}

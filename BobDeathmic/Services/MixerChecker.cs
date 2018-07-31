using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models.Enum;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
                        foreach (Models.Stream stream in GetStreams())
                        {
                            string RequestLink = baseUrl + stream.StreamName;
                            var response = await client.GetAsync(RequestLink);
                            var responsestring = await response.Content.ReadAsStringAsync();
                            JSONObjects.MixerChannelInfo streamInfo = JsonConvert.DeserializeObject<JSONObjects.MixerChannelInfo>(responsestring);
                            if(streamInfo.online)
                            {
                                if(stream.StreamState == StreamState.NotRunning)
                                {
                                    SetStreamOnline();
                                    void SetStreamOnline()
                                    {
                                        stream.Started = DateTime.Now;
                                        stream.Game = streamInfo.type.name;
                                        stream.StreamState = StreamState.Started;
                                        stream.Url = $"https://mixer.com/{stream.StreamName}";
                                    }
                                    void StreamStarted()
                                    {
                                        StreamEventArgs args = new StreamEventArgs();
                                        args.Notification = stream.StreamStartedMessage();
                                        args.state = StreamState.Started;
                                        args.link = stream.Url;
                                        args.channel = stream.StreamName;
                                        args.StreamType = StreamProviderTypes.Mixer;
                                        //replace with real state when/if relay integrated
                                        args.relayactive = RelayState.NotActivated;
                                        _eventBus.TriggerEvent(EventType.StreamChanged, args);
                                    }
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
                        _context.SaveChangesAsync();
                        IQueryable<Models.Stream> GetStreams()
                        {
                            return _context.StreamModels.Where(x => x.Type == StreamProviderTypes.Mixer);
                        }
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

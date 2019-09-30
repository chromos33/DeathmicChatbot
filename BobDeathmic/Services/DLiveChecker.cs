using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Services
{
    public class DLiveChecker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private System.Timers.Timer _timer;
        private bool _inProgress;
        private IEventBus _eventBus;
        public DLiveChecker(IServiceScopeFactory scopeFactory, IEventBus eventBus)
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
        private async void CheckOnlineStreams()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    HttpClient client = new HttpClient();
                    string baseurl = "https://graphigo.prd.dlive.tv/";
                    IQueryable<Models.Stream> GetStreams()
                    {
                        return _context.StreamModels.Where(x => x.Type == StreamProviderTypes.DLive).Include(x => x.StreamSubscriptions).ThenInclude(x => x.User);
                    }
                    foreach (Models.Stream stream in GetStreams())
                    {
                        var content = new StringContent("{\"query\":\"{ userByDisplayName(displayname: \\\""+stream.StreamName+"\\\") {livestream{id}}}\"}", Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(baseurl, content);
                        var responsestring = await response.Content.ReadAsStringAsync();
                        JSONObjects.DLive.DLiveStreamOnlineData streamInfo = JsonConvert.DeserializeObject<JSONObjects.DLive.DLiveStreamOnlineData>(responsestring);
                        if(streamInfo.data.userByDisplayName.livestream != null)
                        {
                            SetStreamOnline();
                            StreamStarted();
                            void SetStreamOnline()
                            {
                                stream.Started = DateTime.Now;
                                if (streamInfo.data.userByDisplayName.livestream.category != null)
                                {
                                    stream.Game = streamInfo.data.userByDisplayName.livestream.category.title;
                                }
                                else
                                {
                                    stream.Game = "Undefined";
                                }
                                stream.StreamState = StreamState.Started;

                                stream.Url = $"https://dlive.tv/{stream.StreamName}";
                            }
                            async void StreamStarted()
                            {
                                if(stream.StreamState == StreamState.Started)
                                {
                                    foreach (string username in stream.GetActiveSubscribers())
                                    {
                                        _eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, new MessageArgs() { Message = stream.StreamStartedMessage(streamInfo.data.userByDisplayName.livestream.title, stream.Url), RecipientName = username });
                                        await Task.Delay(100);
                                    }
                                    stream.StreamState = StreamState.Running;
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
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }catch (Exception ex)
            {

            }
        }
    }
}

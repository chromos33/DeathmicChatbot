using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api;
using BobDeathmic.Services;
using BobDeathmic;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.Enums.Stream;
using TwitchLib.Api.Helix.Models.Streams;

namespace BobDeathmic.Services.Streams.Checker.Twitch
{
    public class TwitchChecker : BackgroundService
    {
        //private readonly IEventBus _eventbus;
        protected TwitchAPI api;
        private readonly IServiceScopeFactory _scopeFactory;
        private System.Timers.Timer _timer;
        private bool _inProgress;
        private IEventBus _eventBus;

        public IConfiguration Configuration { get; }
        public TwitchChecker(IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            api = new TwitchAPI();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                SecurityToken data = null;
                while (data == null)
                {
                    data = _context.SecurityTokens.Where(securitykey => securitykey.service == TokenType.Twitch).FirstOrDefault();
                    if (data == null)
                    {
                        //Just to prevent if from sleeping when it has data
                        await Task.Delay(10000, stoppingToken);
                    }

                }
                if (data != null)
                {
                    api.Settings.ClientId = data.ClientID;
                    api.Settings.AccessToken = data.token;
                }
            }
            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed += (sender, args) => CheckOnlineStreams();
            _timer.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
        public bool TriggerUpTime(Data.DBModels.StreamModels.Stream stream)
        {
            if (stream.UpTimeInterval > 0)
            {
                if (stream.Started > stream.LastUpTime)
                {
                    stream.LastUpTime = stream.Started;
                }
                var UpTime = DateTime.Now - stream.LastUpTime;
                if (UpTime.TotalMinutes > stream.UpTimeInterval)
                {
                    return true;
                }
            }
            return false;
        }
        public TimeSpan GetUpTime(Data.DBModels.StreamModels.Stream stream)
        {
            return DateTime.Now - stream.Started;
        }
        public async Task CheckOnlineStreams()
        {
            try
            {
                if (!_inProgress)
                {
                    _inProgress = true;
                    await FillClientIDs();
                    GetStreamsResponse StreamsData = await GetStreamData();
                    List<string> OnlineStreamIDs = StreamsData.Streams.Select(x => x.UserId).ToList();
                    await SetStreamsOffline(OnlineStreamIDs);
                    if (StreamsData.Streams.Count() > 0)
                    {
                        await SetStreamsOnline(OnlineStreamIDs, StreamsData);
                    }
                    _inProgress = false;
                }
            }
            catch (Exception ex)
            {
                _inProgress = false;
            }
            return;
        }
        private async Task FillClientIDs()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var Streams = _context.StreamModels;
                var StreamNameList = Streams.Where(x => string.IsNullOrEmpty(x.UserID)).Select(x => x.StreamName).ToList();
                if (StreamNameList.Any())
                {
                    var userdata = await api.Helix.Users.GetUsersAsync(logins: StreamNameList);

                    foreach (var user in userdata.Users)
                    {
                        Data.DBModels.StreamModels.Stream stream = Streams.Where(x => x.StreamName.ToLower() == user.Login.ToLower()).FirstOrDefault();
                        stream.UserID = user.Id;
                    }
                    _context.SaveChanges();

                }
            }
        }
        private async Task<GetStreamsResponse> GetStreamData()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var Streams = _context.StreamModels;
                    List<string> StreamIdList = Streams.Where(x => !string.IsNullOrEmpty(x.UserID) && x.Type == StreamProviderTypes.Twitch).Select(x => x.UserID).ToList();
                    if (StreamIdList.Any())
                    {
                        return await api.Helix.Streams.GetStreamsAsync(userIds: StreamIdList);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }
        string[] RandomDiscordRelayChannels = { "stream_1", "stream_2", "stream_3" };
        private async Task<bool> SetStreamsOffline(List<string> OnlineStreamIDs)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var Streams = _context.StreamModels.Where(s => !OnlineStreamIDs.Contains(s.UserID));
                
                foreach (Data.DBModels.StreamModels.Stream stream in Streams.Where(x => x.StreamState != StreamState.NotRunning && x.Type == StreamProviderTypes.Twitch))
                {
                    if(DateTime.Now.Subtract(stream.Started) > TimeSpan.FromSeconds(600))
                    {
                        Console.WriteLine("Stream offline");
                        stream.StreamState = StreamState.NotRunning;
                        if (RandomDiscordRelayChannels.Contains(stream.DiscordRelayChannel))
                        {
                            stream.DiscordRelayChannel = "An";
                        }
                    }
                }
                _context.SaveChanges();
            }
            return true;
        }
        private async Task<bool> SetStreamsOnline(List<string> OnlineStreamIDs, GetStreamsResponse StreamsData)
        {
            
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var Streams = _context.StreamModels.Where(s => OnlineStreamIDs.Contains(s.UserID) && s.Type == StreamProviderTypes.Twitch).Include(x => x.StreamSubscriptions).ThenInclude(y => y.User);
                Boolean write = false;
                foreach (Data.DBModels.StreamModels.Stream stream in Streams)
                {
                    TwitchLib.Api.Helix.Models.Streams.Stream streamdata = StreamsData.Streams.Single(sd => sd.UserId == stream.UserID);
                    if (stream.StreamState == StreamState.NotRunning)
                    {
                        stream.StreamState = StreamState.Running;
                        stream.Started = streamdata.StartedAt.ToLocalTime();
                        write = true;
                        if(stream.DiscordRelayChannel == "An")
                        {
                            stream.DiscordRelayChannel = getRandomRelayChannel();
                        }
                        await NotifyUsers(stream);
                    }

                }
                if (write)
                {
                    _context.SaveChanges();
                }
                
            }
            return true;
        }
        private async Task NotifyUsers(Data.DBModels.StreamModels.Stream stream)
        {
            int longDelayCounter = 0;
            Console.WriteLine("FUCK YOU TWITCH " + stream.StreamName);
            foreach (string username in stream.GetActiveSubscribers())
            {
                longDelayCounter++;
                if (longDelayCounter == 5)
                {
                    longDelayCounter = 0;
                    await Task.Delay(2000);
                }

                //_eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, new MessageArgs() { Message = stream.StreamStartedMessage(streamdata.Title, GetStreamUrl(stream)), RecipientName = username });
                await Task.Delay(100);
            }
        }
        private string getRandomRelayChannel()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                string[] occupiedchannels = _context.StreamModels.Where(x => RandomDiscordRelayChannels.Contains(x.DiscordRelayChannel)).Select(x => x.DiscordRelayChannel).ToArray();
                return RandomDiscordRelayChannels.Except(occupiedchannels).FirstOrDefault();
            }
        }
        private string GetStreamUrl(Data.DBModels.StreamModels.Stream stream)
        {
            if (string.IsNullOrEmpty(stream.Url))
            {
                stream.Url = "https://www.twitch.tv/" + stream.StreamName;
            }
            return stream.Url;
        }
    }
}

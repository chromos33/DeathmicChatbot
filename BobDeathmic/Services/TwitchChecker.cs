using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Models.Enum;
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
using TwitchLib.Api.Models.Helix.Streams.GetStreams;

namespace BobDeathmic.Services
{
    public class TwitchChecker : BackgroundService
    {
        //private readonly IEventBus _eventbus;
        protected TwitchAPI api;
        private readonly IServiceScopeFactory _scopeFactory;
        private System.Timers.Timer _timer;
        private bool _inProgress = false;
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
                Models.SecurityToken data = null;
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
                else
                {
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
        public bool TriggerUpTime(Models.Stream stream)
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
        public async Task CheckOnlineStreams()
        {
            try
            {
                if (!_inProgress)
                {
                    _inProgress = true;
                    List<Models.Stream> Streams = GetStreams();
                    await FillClientIDs(Streams);
                    GetStreamsResponse StreamsData = await GetStreamData(Streams);
                    List<string> OnlineStreamIDs = StreamsData.Streams.Select(x => x.UserId).ToList();
                    SetStreamsOffline(Streams.Where(x => !OnlineStreamIDs.Contains(x.UserID)).ToList());
                    SetStreamsOnline(Streams.Where(x => OnlineStreamIDs.Contains(x.UserID)).ToList(), StreamsData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _inProgress = false;
            }
            _inProgress = false;
            return;
        }
        private List<Models.Stream> GetStreams()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return _context.StreamModels.ToList();
            }
        }
        private async Task FillClientIDs(List<Models.Stream> Streams)
        {
            var StreamNameList = Streams.Where(x => x.UserID == null || x.UserID == "").Select(x => x.StreamName).ToList();
            if (StreamNameList.Count() > 0)
            {
                var userdata = await api.Users.helix.GetUsersAsync(logins: StreamNameList);
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    foreach (var user in userdata.Users)
                    {
                        Models.Stream stream = Streams.Where(x => x.StreamName.ToLower() == user.Login.ToLower()).FirstOrDefault();
                        stream.UserID = user.Id;

                        _context.Update(stream);
                    }
                    _context.SaveChanges();
                }
            }
        }
        private async Task<GetStreamsResponse> GetStreamData(List<Models.Stream> Streams)
        {
            List<string> StreamIdList = Streams.Where(x => x.UserID != null && x.UserID != "").Select(x => x.UserID).ToList();
            if (StreamIdList.Count() > 0)
            {
                return await api.Streams.helix.GetStreamsAsync(userIds: StreamIdList);
            }
            return null;
        }
        private async Task SetStreamsOffline(List<Models.Stream> Streams)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                foreach (Models.Stream stream in Streams.Where(x => x.StreamState != StreamState.NotRunning))
                {
                    stream.StreamState = StreamState.NotRunning;
                    _context.Update(stream);
                    StreamEventArgs args = new StreamEventArgs();
                    args.stream = "";
                    args.channel = stream.StreamName;
                    args.state = stream.StreamState;
                    args.link = "";
                    _eventBus.TriggerEvent(EventType.StreamChanged, args);
                }
                _context.SaveChanges();
            }
        }
        private async Task SetStreamsOnline(List<Models.Stream> Streams, GetStreamsResponse StreamsData)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                foreach (Models.Stream stream in Streams)
                {
                    //Bla do stuff
                    if(stream.StreamState == StreamState.NotRunning)
                    {
                        stream.StreamState = StreamState.Started;
                    }
                    else
                    {
                        stream.StreamState = StreamState.Running;
                    }
                    //Get StreamData for this stream and do stuff

                }
            }
        }
    }
}

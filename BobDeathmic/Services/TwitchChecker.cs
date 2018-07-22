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
            if(stream.UpTimeInterval > 0)
            {
                if(stream.Started > stream.LastUpTime)
                {
                    stream.LastUpTime = stream.Started;
                }
                var UpTime = DateTime.Now - stream.LastUpTime;
                if(UpTime.TotalMinutes > stream.UpTimeInterval)
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
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        _inProgress = true;
                        foreach (Models.Stream _stream in _context.StreamModels.ToList())
                        {
                            StreamEventArgs args = new StreamEventArgs();
                            args.StreamType = StreamProviderTypes.Twitch;
                            args.relayactive = _stream.RelayState;
                            if (_stream.UserID != null && _stream.UserID != "")
                            {
                                if (await api.Streams.v5.BroadcasterOnlineAsync(_stream.UserID))
                                {
                                    if (_stream.StreamState == StreamState.NotRunning)
                                    {
                                        var streamdata = await api.Streams.v5.GetStreamByUserAsync(_stream.UserID.ToString());
                                        string game = streamdata.Stream.Game;
                                        string channel = streamdata.Stream.Channel.Name;
                                        DateTime startdate = streamdata.Stream.CreatedAt;
                                        FillStreamArgs();
                                        void FillStreamArgs()
                                        {
                                            args.game = game;
                                            args.channel = channel;
                                            _stream.Game = game;
                                            _stream.Url = streamdata.Stream.Channel.Url;
                                            args.link = streamdata.Stream.Channel.Url;


                                            _stream.Started = DateTime.Now;
                                            args.Notification = _stream.StreamStartedMessage();
                                            args.stream = _stream.StreamName;
                                        }
                                    }
                                    else
                                    {
                                        args.game = _stream.Game;
                                        args.link = _stream.Url;
                                        args.channel = _stream.DiscordRelayChannel;
                                        args.stream = _stream.StreamName;
                                    }
                                    //Enable later on
                                    //SetupRelayChannel();
                                    await StartStreamOrCheckRelay();
                                    async Task<bool> StartStreamOrCheckRelay()
                                    {
                                        if (_stream.StreamState == StreamState.NotRunning)
                                        {
                                            args.state = StreamState.Started;
                                            _stream.StreamState = StreamState.Running;
                                            _context.StreamModels.Update(_stream);
                                            await _context.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            args.state = StreamState.Running;
                                            _context.StreamModels.Update(_stream);
                                            await _context.SaveChangesAsync();
                                        }
                                        return true;
                                    }
                                    if(TriggerUpTime(_stream))
                                    {
                                        args.PostUpTime = true;
                                        args.Uptime = DateTime.Now - _stream.Started;
                                        _stream.LastUpTime = DateTime.Now;
                                        await _context.SaveChangesAsync();
                                    }
                                    _eventBus.TriggerEvent(EventType.StreamChanged, args);

                                }
                                else
                                {
                                    if (_stream.StreamState == StreamState.Running)
                                    {
                                        args.channel = _stream.StreamName;
                                        args.game = "";
                                        args.state = StreamState.NotRunning;
                                        args.stream = "";
                                        args.link = "";
                                        _stream.StreamState = StreamState.NotRunning;
                                        _context.StreamModels.Update(_stream);
                                        await _context.SaveChangesAsync();
                                        _eventBus.TriggerEvent(EventType.StreamChanged, args);
                                    }
                                }
                            }
                        }
                    }
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
    }
}

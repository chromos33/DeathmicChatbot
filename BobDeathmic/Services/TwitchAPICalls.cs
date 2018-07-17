using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api;

namespace BobDeathmic.Services
{
    public class TwitchAPICalls : BackgroundService
    {

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEventBus _eventBus;
        public TwitchAPICalls(IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.StreamTitleChangeRequested += StreamTitleChangeRequested;
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async void StreamTitleChangeRequested(object sender, StreamTitleChangeArgs e)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Models.Stream stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower().Equals(e.StreamName.ToLower())).FirstOrDefault();
                if(stream != null)
                {
                    if(stream.ClientID != "" && stream.AccessToken != "")
                    {
                        TwitchAPI api = new TwitchAPI();
                        api.Settings.ClientId = stream.ClientID;
                        api.Settings.AccessToken = stream.AccessToken;
                        var GameRegex = Regex.Match(e.Message, @"(?<=game=')\w+(?=')");
                        string Game = "";
                        if (GameRegex.Success)
                        {
                            Game = GameRegex.Value;
                        }
                        var TitleRegex = Regex.Match(e.Message, @"(?<=title=\')\w+(?=\')");
                        string Title = "";
                        if (TitleRegex.Success)
                        {
                            Title = TitleRegex.Value;
                        }
                        if(Game != "" && Title != "")
                        {
                            var test = await api.Channels.v5.UpdateChannelAsync(channelId: stream.UserID, status:Title, game:Game);
                            Console.WriteLine("test");
                        }
                        if (Game != "" && Title == "")
                        {
                            var test = await api.Channels.v5.UpdateChannelAsync(channelId: stream.UserID, game: Game);
                            Console.WriteLine("test");
                        }
                        if (Game == "" && Title != "")
                        {
                            var test = await api.Channels.v5.UpdateChannelAsync(channelId: stream.UserID, status: Title);
                            Console.WriteLine("test");
                        }

                    }

                    
                }
            }
            return;
        }
    }
}

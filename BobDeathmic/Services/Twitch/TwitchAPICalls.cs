using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private readonly IConfiguration _configuration;
        public TwitchAPICalls(IServiceScopeFactory scopeFactory, IEventBus eventBus, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            _configuration = configuration;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.StreamTitleChangeRequested += StreamTitleChangeRequested;
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
        private async Task<string> RefreshToken(string streamname)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Models.Stream stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower().Equals(streamname) && sm.Type == Models.Enum.StreamProviderTypes.Twitch).FirstOrDefault();
                if (stream != null && stream.RefreshToken != null && stream.RefreshToken != "")
                {
                    var httpclient = new HttpClient();
                    string baseUrl = _configuration.GetValue<string>("WebServerWebAddress");
                    string url = $"https://id.twitch.tv/oauth2/token?grant_type=refresh_token&refresh_token={stream.RefreshToken}&client_id={stream.ClientID}&client_secret={stream.Secret}";
                    var response = await httpclient.PostAsync(url, new StringContent("", System.Text.Encoding.UTF8, "text/plain"));
                    var responsestring = await response.Content.ReadAsStringAsync();
                    JSONObjects.TwitchRefreshTokenData refresh = JsonConvert.DeserializeObject<JSONObjects.TwitchRefreshTokenData>(responsestring);
                    if (refresh.error == null)
                    {
                        stream.AccessToken = refresh.access_token;
                        stream.RefreshToken = refresh.refresh_token;
                        _context.SaveChanges();
                        return refresh.access_token;
                    }
                }
            }
            return "";
        }
        private async void StreamTitleChangeRequested(object sender, StreamTitleChangeArgs e)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Models.Stream stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower().Equals(e.StreamName.ToLower())).FirstOrDefault();
                if (stream != null && stream.ClientID != null && stream.ClientID != "" && stream.AccessToken != null && stream.AccessToken != "")
                {
                    TwitchAPI api = new TwitchAPI();
                    api.Settings.ClientId = stream.ClientID;
                    api.Settings.AccessToken = stream.AccessToken;
                    string message = "";
                    try
                    {
                        if (e.Game != "" && e.Title != "")
                        {
                            var test = await api.V5.Channels.UpdateChannelAsync(channelId: stream.UserID, status: e.Title, game: e.Game);
                            if (test.Game == e.Game && test.Status == e.Title)
                            {
                                message = "Stream Updated";
                            }
                            else
                            {
                                message = "Error while updating";
                            }

                        }
                        if (e.Game != "" && e.Title == "")
                        {
                            var test = await api.V5.Channels.UpdateChannelAsync(channelId: stream.UserID, game: e.Game);
                            //Does not actually work as request just returns the entered game irrelevant of actual change (always returns true)
                            if(test.Game == e.Game)
                            {
                                message = "Game Updated";
                            }
                            else
                            {
                                message = "Twitch game mismatch";
                            }
                        }
                        if (e.Game == "" && e.Title != "")
                        {
                            var test = await api.V5.Channels.UpdateChannelAsync(channelId: stream.UserID, status: e.Title);
                            if (test.Status == e.Title)
                            {
                                message = "Title Updated";
                            }
                            else
                            {
                                message = "Title not Updated";
                            }
                        }

                    }
                    catch (Exception ex) when (ex is TwitchLib.Api.Core.Exceptions.InvalidCredentialException || ex is TwitchLib.Api.Core.Exceptions.BadScopeException)
                    {

                        api.Settings.AccessToken = await RefreshToken(stream.StreamName);

                        if (api.Settings.AccessToken != "")
                        {
                            if (e.Game != "" && e.Title != "")
                            {
                                var test = await api.V5.Channels.UpdateChannelAsync(channelId: stream.UserID, status: e.Title, game: e.Game);
                            }
                            if (e.Game != "" && e.Title == "")
                            {
                                var test = await api.V5.Channels.UpdateChannelAsync(channelId: stream.UserID, game: e.Game);
                            }
                            if (e.Game == "" && e.Title != "")
                            {
                                var test = await api.V5.Channels.UpdateChannelAsync(channelId: stream.UserID, status: e.Title);
                            }
                        }
                    }
                    _eventBus.TriggerEvent(EventType.RelayMessageReceived, new Args.RelayMessageArgs() { SourceChannel = stream.DiscordRelayChannel, StreamType = Models.Enum.StreamProviderTypes.Twitch, TargetChannel = stream.StreamName, Message = message });
                }
                else
                {
                    _eventBus.TriggerEvent(EventType.RelayMessageReceived, new Args.RelayMessageArgs() { SourceChannel = stream.DiscordRelayChannel, StreamType = Models.Enum.StreamProviderTypes.Twitch, TargetChannel = stream.StreamName, Message = "Stream can't be updated no Authorization key detected." });
                }
            }
            return;
        }
    }
}

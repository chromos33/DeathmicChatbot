using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.Enums.Stream;
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
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;

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
                Stream stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower().Equals(streamname) && sm.Type == StreamProviderTypes.Twitch).FirstOrDefault();
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
                Stream stream = _context.StreamModels.Where(sm => sm.StreamName.ToLower().Equals(e.StreamName.ToLower())).FirstOrDefault();
                if (stream != null && !string.IsNullOrEmpty(stream.ClientID) && !string.IsNullOrEmpty(stream.AccessToken))
                {
                    TwitchAPI api = new TwitchAPI();
                    api.Settings.ClientId = stream.ClientID;
                    api.Settings.AccessToken = stream.AccessToken;
                    string message = "";
                    try
                    {
                        if (!string.IsNullOrEmpty(e.Game) && !string.IsNullOrEmpty(e.Title))
                        {
                            var gameId = (await api.Helix.Games.GetGamesAsync(gameNames: new List<string> { e.Game })).Games.FirstOrDefault()?.Id;
                            var updateRequest = new ModifyChannelInformationRequest
                            {
                                Title = e.Title,
                                GameId = gameId
                            };
                            await api.Helix.Channels.ModifyChannelInformationAsync(stream.UserID, updateRequest);
                            message = "Stream Updated";
                        }
                        else if (!string.IsNullOrEmpty(e.Game))
                        {
                            var gameId = (await api.Helix.Games.GetGamesAsync(gameNames: new List<string> { e.Game })).Games.FirstOrDefault()?.Id;
                            var updateRequest = new ModifyChannelInformationRequest
                            {
                                GameId = gameId
                            };
                            await api.Helix.Channels.ModifyChannelInformationAsync(stream.UserID, updateRequest);
                            message = "Game Updated";
                        }
                        else if (!string.IsNullOrEmpty(e.Title))
                        {
                            var updateRequest = new ModifyChannelInformationRequest
                            {
                                Title = e.Title
                            };
                            await api.Helix.Channels.ModifyChannelInformationAsync(stream.UserID, updateRequest);
                            message = "Title Updated";
                        }
                    }
                    catch (Exception ex) when (ex is TwitchLib.Api.Core.Exceptions.InvalidCredentialException || ex is TwitchLib.Api.Core.Exceptions.BadScopeException)
                    {
                        api.Settings.AccessToken = await RefreshToken(stream.StreamName);

                        if (!string.IsNullOrEmpty(api.Settings.AccessToken))
                        {
                            if (!string.IsNullOrEmpty(e.Game) && !string.IsNullOrEmpty(e.Title))
                            {
                                var gameId = (await api.Helix.Games.GetGamesAsync(gameNames: new List<string> { e.Game })).Games.FirstOrDefault()?.Id;
                                var updateRequest = new ModifyChannelInformationRequest
                                {
                                    Title = e.Title,
                                    GameId = gameId
                                };
                                await api.Helix.Channels.ModifyChannelInformationAsync(stream.UserID, updateRequest);
                            }
                            else if (!string.IsNullOrEmpty(e.Game))
                            {
                                var gameId = (await api.Helix.Games.GetGamesAsync(gameNames: new List<string> { e.Game })).Games.FirstOrDefault()?.Id;
                                var updateRequest = new ModifyChannelInformationRequest
                                {
                                    GameId = gameId
                                };
                                await api.Helix.Channels.ModifyChannelInformationAsync(stream.UserID, updateRequest);
                            }
                            else if (!string.IsNullOrEmpty(e.Title))
                            {
                                var updateRequest = new ModifyChannelInformationRequest
                                {
                                    Title = e.Title
                                };
                                await api.Helix.Channels.ModifyChannelInformationAsync(stream.UserID, updateRequest);
                            }
                        }
                    }
                    _eventBus.TriggerEvent(EventType.RelayMessageReceived, new Args.RelayMessageArgs() { SourceChannel = stream.DiscordRelayChannel, StreamType = StreamProviderTypes.Twitch, TargetChannel = stream.StreamName, Message = message });
                }
                else
                {
                    _eventBus.TriggerEvent(EventType.RelayMessageReceived, new Args.RelayMessageArgs() { SourceChannel = stream.DiscordRelayChannel, StreamType = StreamProviderTypes.Twitch, TargetChannel = stream.StreamName, Message = "Stream can't be updated no Authorization key detected." });
                }
            }
            return;
        }
    }
}

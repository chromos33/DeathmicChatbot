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
                if(stream != null && stream.ClientID != "" && stream.AccessToken != "")
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
                    try
                    {
                        if (Game != "" && Title != "")
                        {
                            await api.Channels.v5.UpdateChannelAsync(channelId: stream.UserID, status: Title, game: Game);
                            
                        }
                        if (Game != "" && Title == "")
                        {
                            var test = await api.Channels.v5.UpdateChannelAsync(channelId: stream.UserID, game: Game);
                        }
                        if (Game == "" && Title != "")
                        {
                            var test = await api.Channels.v5.UpdateChannelAsync(channelId: stream.UserID, status: Title);
                        }
                    }catch(Exception ex) when (ex is TwitchLib.Api.Exceptions.InvalidCredentialException || ex is TwitchLib.Api.Exceptions.BadScopeException)
                    {
                        
                        api.Settings.AccessToken = await RefreshToken(stream.StreamName);

                        if(api.Settings.AccessToken != "")
                        {
                            if (Game != "" && Title != "")
                            {
                                var test = await api.Channels.v5.UpdateChannelAsync(channelId: stream.UserID, status: Title, game: Game);
                            }
                            if (Game != "" && Title == "")
                            {
                                var test = await api.Channels.v5.UpdateChannelAsync(channelId: stream.UserID, game: Game);
                            }
                            if (Game == "" && Title != "")
                            {
                                var test = await api.Channels.v5.UpdateChannelAsync(channelId: stream.UserID, status: Title);
                            }
                        }
                    }
                    _eventBus.TriggerEvent(EventType.DiscordMessageReceived, new Args.DiscordMessageArgs() { Source = stream.DiscordRelayChannel, StreamType = Models.Enum.StreamProviderTypes.Twitch, Target = stream.StreamName, Message = "Stream Updated" });
                }
            }
            return;
        }
    }
}

using BobDeathmic.Args;
using BobDeathmic.Cron.Setup;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.EventCalendar;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Eventbus;
using BobDeathmic.Helper;
using BobDeathmic.Helper.EventCalendar;
using BobDeathmic.Models.Events;
using BobDeathmic.Services.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BobDeathmic.Cron
{
    public class TwitchTokenRefreshTask : IScheduledTask
    {
        public string Schedule => "0 */3 * * *";
        private readonly IServiceScopeFactory _scopeFactory;
        private IConfiguration _configuration;
        public TwitchTokenRefreshTask(IServiceScopeFactory scopeFactory, IConfiguration Configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = Configuration;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (_context != null)
                {
                    SecurityToken token = _context.SecurityTokens.Where(t => t.service == Data.Enums.Stream.TokenType.Twitch).FirstOrDefault();
                    if (token != null && token.RefreshToken != null && token.RefreshToken != "")
                    {
                        var httpclient = new HttpClient();
                        string baseUrl = _configuration.GetValue<string>("WebServerWebAddress");
                        string url = $"https://id.twitch.tv/oauth2/token?grant_type=refresh_token&refresh_token={token.RefreshToken}&client_id={token.ClientID}&client_secret={token.secret}";
                        var response = await httpclient.PostAsync(url, new StringContent("", System.Text.Encoding.UTF8, "text/plain"));
                        var responsestring = await response.Content.ReadAsStringAsync();
                        JSONObjects.TwitchRefreshTokenData refresh = JsonConvert.DeserializeObject<JSONObjects.TwitchRefreshTokenData>(responsestring);
                        if (refresh.error == null)
                        {
                            token.token = refresh.access_token;
                            token.RefreshToken = refresh.refresh_token;
                            _context.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}

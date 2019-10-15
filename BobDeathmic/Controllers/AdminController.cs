using BobDeathmic.Models;
using BobDeathmic.Models.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace BobDeathmic.Controllers
{
    public class AdminController : Controller
    {

        private readonly Data.ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private Random random;

        public AdminController(Data.ApplicationDbContext context, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            random = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / DateTime.Now.Hour);
        }
        [Authorize(Roles = "Dev,Admin")]
        public IActionResult Index()
        {
            return View();
        }
        private async Task<Boolean> SaveFileToLocalAsync(List<IFormFile> files, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await files.FirstOrDefault()?.CopyToAsync(stream);
            }
            return true;
        }

        [Authorize(Roles = "Dev")]
        public IActionResult SecurityTokens()
        {
            ViewData["TokenTypes"] = new List<TokenType> { TokenType.Twitch, TokenType.Discord, TokenType.Mixer };
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> AddSecurityToken([Bind("ClientID,secret,service")] Models.SecurityToken token)
        {
            switch (token.service)
            {
                case TokenType.Twitch:
                    return await AquireTwitchToken(token);
                case TokenType.Discord:
                    await SaveDiscordToken(token);
                    break;
            }
            return RedirectToAction(nameof(SecurityTokens));
        }
        public async Task SaveDiscordToken(SecurityToken token)
        {
            if (_context.SecurityTokens.Where(st => st.service == token.service).Count() == 0)
            {
                _context.SecurityTokens.Add(token);
            }
            else
            {
                var oldtoken = _context.SecurityTokens.Where(st => st.service == token.service).FirstOrDefault();
                oldtoken.token = token.ClientID;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> AquireTwitchToken(SecurityToken token)
        {
            string baseUrl = _configuration.GetValue<string>("WebServerWebAddress");
            if (_context.SecurityTokens.Where(st => st.service == token.service).Count() == 0)
            {
                _context.SecurityTokens.Add(token);
                await _context.SaveChangesAsync();
            }
            else
            {
                var oldtoken = _context.SecurityTokens.Where(st => st.service == token.service).FirstOrDefault();
                oldtoken = token;
                await _context.SaveChangesAsync();
            }
            string state = "as435aerfaw45w456";
            return Redirect($"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={token.ClientID}&redirect_uri={baseUrl}/Admin/TwitchReturnUrlAction&scope=channel_editor+chat_login+user:edit&state={state}");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TwitchReturnUrlAction(string code, string scope, string state)
        {
            var savedtoken = _context.SecurityTokens.Where(st => st.service == BobDeathmic.Models.Enum.TokenType.Twitch).FirstOrDefault();
            savedtoken.code = code;
            var client = new HttpClient();
            string baseUrl = _configuration.GetValue<string>("WebServerWebAddress");
            string url = $"https://id.twitch.tv/oauth2/token?client_id={savedtoken.ClientID}&client_secret={savedtoken.secret}&code={code}&grant_type=authorization_code&redirect_uri={baseUrl}/Admin/TwitchReturnUrlAction";
            var response = await client.PostAsync(url, new StringContent("", System.Text.Encoding.UTF8, "text/plain"));
            var responsestring = await response.Content.ReadAsStringAsync();
            JSONObjects.TwitchAuthToken authtoken = JsonConvert.DeserializeObject<JSONObjects.TwitchAuthToken>(responsestring);
            savedtoken.token = authtoken.access_token;
            savedtoken.RefreshToken = authtoken.refresh_token;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(SecurityTokens));
        }
    }
}
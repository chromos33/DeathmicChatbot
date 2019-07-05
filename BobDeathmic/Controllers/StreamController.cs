using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Models.StreamModels;
using BobDeathmic.Models.StreamViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BobDeathmic.Controllers
{
    public class StreamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEventBus _eventBus;

        public StreamController(ApplicationDbContext context, IConfiguration configuration, IEventBus eventBus)
        {
            _context = context;
            _configuration = configuration;
            _eventBus = eventBus;
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Status));
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Status()
        {
            Models.StreamViewModels.StreamListDataModel model = new Models.StreamViewModels.StreamListDataModel();
            model.StreamList = await _context.StreamModels.ToListAsync();
            model.StatusMessage = StatusMessage;
            return View(model);
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Verwaltung()
        {
            Models.StreamViewModels.StreamListDataModel model = new Models.StreamViewModels.StreamListDataModel();
            model.StreamList = await _context.StreamModels.ToListAsync();
            model.StatusMessage = StatusMessage;
            return View(model);
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stream = await _context.StreamModels.FindAsync(id);
            if (stream == null)
            {
                return NotFound();
            }
            StreamEditViewDataModel model = new StreamEditViewDataModel();
            model.stream = stream;
            model.StreamTypes = stream.EnumStreamTypes();
            model.RelayChannels = await _context.RelayChannels.ToListAsync();
            model.SelectedRelayChannel = stream.DiscordRelayChannel;
            model.StatusMessage = StatusMessage;
            return View(model);
        }

        // POST: Streams2/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,StreamName,Game,UserID,Url,Type,Started,Stopped,StreamState,DiscordRelayChannel,UpTimeInterval")] Stream stream)
        {
            if (id != stream.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stream);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StreamExists(stream.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Stream angepasst";
                return RedirectToAction(nameof(Edit));
            }
            StatusMessage = "Bitte nochmal versuchen eine Eingabe hat nicht gestimmt";
            return RedirectToAction(nameof(Edit));
        }

        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Create()
        {
            ViewData["StreamTypes"] = Stream.StaticEnumStreamTypes();
            var relaychannels = await _context.RelayChannels.ToListAsync();
            ViewData["RelayChannels"] = relaychannels;
            ViewData["SelectedRelayChannel"] = "Aus";
            return View();
        }

        // POST: Streams2/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Create([Bind("ID,StreamName,Game,UserID,Url,Type,AccessToken,Secret,ClientID,Started,Stopped,StreamState,DiscordRelayChannel,UpTimeInterval,LastUpTime")] Stream stream)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stream);
                await _context.SaveChangesAsync();
                handleCreated(stream);
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Verwaltung));
        }
        private async Task handleCreated(Stream stream)
        {
            string address = _configuration.GetValue<string>("WebServerWebAddress") + "/User/Subscriptions";
            MessageArgs args = new MessageArgs();
            args.Message = $"Der Stream {stream.StreamName} wurde hinzugefügt. Da kannst du für {address} ein Abonnement ausführen tun.";
            foreach (ChatUserModel user in _context.ChatUserModels)
            {
                args.RecipientName = user.ChatUserName;
                _eventBus.TriggerEvent(EventType.DiscordMessageSendRequested, args);
            }

        }
        [TempData]
        public string StatusMessage { get; set; }
        // GET: Streams2/Delete/5
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stream = await _context.StreamModels
                .FirstOrDefaultAsync(m => m.ID == id);
            if (stream == null)
            {
                return NotFound();
            }

            return View(stream);
        }

        // POST: Streams2/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stream = await _context.StreamModels.FindAsync(id);
            _context.StreamModels.Remove(stream);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Verwaltung));
        }

        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> TwitchOAuth(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stream = await _context.StreamModels.FindAsync(id);
            if (stream == null)
            {
                return NotFound();
            }
            BobDeathmic.Models.StreamViewModels.StreamOAuthDataModel model = new Models.StreamViewModels.StreamOAuthDataModel();
            model.Id = stream.ID.ToString();
            string baseurl = _configuration.GetSection("WebServerWebAddress").Value;
            model.RedirectLinkForTwitch = $"{baseurl}/Stream/TwitchReturnUrlAction";
            model.StatusMessage = StatusMessage;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> TwitchOAuth(int id, [Bind("Id,ClientId,Secret")] Models.StreamViewModels.StreamOAuthDataModel StreamOAuthData)
        {
            if (id != Int32.Parse(StreamOAuthData.Id))
            {
                return NotFound();
            }

            if (ModelState.IsValid && StreamOAuthData.ClientId != "" && StreamOAuthData.Secret != "")
            {
                var baseUrl = _configuration.GetSection("WebServerWebAddress").Value;
                string state = StreamOAuthData.Id + StreamOAuthData.Secret;
                Models.Stream stream = _context.StreamModels.Where(sm => sm.ID == Int32.Parse(StreamOAuthData.Id)).FirstOrDefault();
                if (stream != null)
                {
                    stream.Secret = StreamOAuthData.Secret;
                    stream.ClientID = StreamOAuthData.ClientId;
                }
                await _context.SaveChangesAsync();
                return Redirect($"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={StreamOAuthData.ClientId}&redirect_uri={baseUrl}/Stream/TwitchReturnUrlAction&scope=channel_editor+user:edit&state={state}");
            }
            StatusMessage = "ClientID und Secret entweder leer oder falsch";
            return View(nameof(TwitchOAuth));
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TwitchReturnUrlAction(string code, string scope, string state)
        {
            var client = new HttpClient();
            Models.Stream stream = _context.StreamModels.Where(sm => state.Contains(sm.Secret)).FirstOrDefault();
            if (stream != null)
            {
                var baseUrl = _configuration.GetSection("WebServerWebAddress").Value;
                string url = $"https://id.twitch.tv/oauth2/token?client_id={stream.ClientID}&client_secret={stream.Secret}&code={code}&grant_type=authorization_code&redirect_uri={baseUrl}/Stream/TwitchReturnUrlAction";

                var response = await client.PostAsync(url, new StringContent("", System.Text.Encoding.UTF8, "text/plain"));
                var responsestring = await response.Content.ReadAsStringAsync();

                JSONObjects.TwitchAuthToken authtoken = JsonConvert.DeserializeObject<JSONObjects.TwitchAuthToken>(responsestring);
                stream.AccessToken = authtoken.access_token;
                stream.RefreshToken = authtoken.refresh_token;

                await _context.SaveChangesAsync();
                StatusMessage = "Stream Oauth complete";
            }

            return RedirectToAction(nameof(Verwaltung));
        }
        private bool StreamExists(int id)
        {
            return _context.StreamModels.Any(e => e.ID == id);
        }
    }
}
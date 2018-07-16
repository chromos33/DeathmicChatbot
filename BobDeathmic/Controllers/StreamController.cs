using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BobDeathmic.Data;
using BobDeathmic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BobDeathmic.Controllers
{
    public class StreamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StreamController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Status));
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Status()
        {
            return View(await _context.StreamModels.ToListAsync());
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Verwaltung()
        {
            return View(await _context.StreamModels.ToListAsync());
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
            ViewData["StreamTypes"] = stream.EnumStreamTypes();
            var relaychannels = await _context.RelayChannels.ToListAsync();
            ViewData["RelayChannels"] = relaychannels;
            ViewData["SelectedRelayChannel"] = stream.DiscordRelayChannel;
            return View(stream);
        }

        // POST: Streams2/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,StreamName,Game,UserID,Url,Type,Started,Stopped,RelayState,StreamState,DiscordRelayChannel")] Stream stream)
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
                return RedirectToAction(nameof(Verwaltung));
            }
            return View(stream);
        }

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
        public async Task<IActionResult> TwitchOAuth(int id)
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
            string baseurl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            ViewData["RedirectLinkForTwitch"] = $"{baseurl}/Stream/TwitchReturnUrlAction";
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

            if (ModelState.IsValid)
            {
                string baseurl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                string state = StreamOAuthData.Id + StreamOAuthData.Secret;
                Models.Stream stream = _context.StreamModels.Where(sm => sm.ID == Int32.Parse(StreamOAuthData.Id)).FirstOrDefault();
                if(stream != null)
                {
                    stream.Secret = StreamOAuthData.Secret;
                    stream.ClientID = StreamOAuthData.ClientId;
                }
                await _context.SaveChangesAsync();
                return Redirect($"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={StreamOAuthData.ClientId}&redirect_uri={baseurl}/Stream/TwitchReturnUrlAction&scope=channel_editor+chat_login&state={state}");
            }
            return View(nameof(TwitchOAuth));
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TwitchReturnUrlAction(string code, string scope, string state)
        {
            var client = new HttpClient();
            Models.Stream stream = _context.StreamModels.Where(sm => state.Contains(sm.Secret)).FirstOrDefault();
            if(stream != null)
            {
                string url = $"https://id.twitch.tv/oauth2/token?client_id={stream.ClientID}&client_secret={stream.Secret}&code={code}&grant_type=authorization_code&redirect_uri=https://localhost:44347/Stream/TwitchReturnUrlAction";
                var response = await client.PostAsync(url, new StringContent("", System.Text.Encoding.UTF8, "text/plain"));
                var responsestring = await response.Content.ReadAsStringAsync();
                JSONObjects.TwitchAuthToken authtoken = JsonConvert.DeserializeObject<JSONObjects.TwitchAuthToken>(responsestring);
                stream.AccessToken = authtoken.access_token;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        private bool StreamExists(int id)
        {
            return _context.StreamModels.Any(e => e.ID == id);
        }
    }
}
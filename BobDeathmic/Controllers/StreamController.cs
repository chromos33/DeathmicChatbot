using BobDeathmic.Args;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.ViewModels.ReactDataClasses.Table;
using BobDeathmic.ViewModels.ReactDataClasses.Table.Columns;
using BobDeathmic.ViewModels.StreamModels;
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
            StreamListDataModel model = new StreamListDataModel();
            model.StreamList = await _context.StreamModels.ToListAsync();
            model.StatusMessage = StatusMessage;
            return View(model);
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> Verwaltung()
        {
            return View();
            StreamListDataModel model = new StreamListDataModel();
            model.StreamList = await _context.StreamModels.ToListAsync();
            model.StatusMessage = StatusMessage;
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<bool> SaveStreamEdit(int StreamID, string StreamName,StreamProviderTypes Type,int UpTime,int Quote,string Relay)
        {
            try
            {
                var stream = _context.StreamModels.Where(x => x.ID == StreamID).FirstOrDefault();
                if (stream != null)
                {
                    stream.StreamName = StreamName;
                    stream.Type = Type;
                    stream.UpTimeInterval = UpTime;
                    stream.QuoteInterval = Quote;
                    stream.DiscordRelayChannel = Relay;
                    _context.SaveChanges();
                    return true;
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
            
        }
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<String> GetStreamEditData(int streamID)
        {
            try
            {
                var stream = _context.StreamModels.Where(x => x.ID == streamID).FirstOrDefault();
                if (stream != null)
                {
                    var data = new BobDeathmic.ViewModels.ReactDataClasses.Other.StreamEditData(stream, _context.RelayChannels.Select(x => x.Name).ToList());
                    return data.ToJSON();
                }
                return "";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
           
        }
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<String> RelayChannels()
        {
            List<string> channels = new List<string>();
            channels.Add("Aus");
            channels.Add("An");
            channels.AddRange(_context.RelayChannels.Select(x => x.Name).ToList());
            return JsonConvert.SerializeObject(channels);
        }
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<String> StreamsData()
        {
            Table table = new Table();
            Row row = new Row(false, true);
            row.AddColumn(new TextColumn(0, "StreamName", true));
            row.AddColumn(new TextColumn(1, "Type"));
            row.AddColumn(new TextColumn(2, ""));
            row.AddColumn(new TextColumn(3, ""));
            row.AddColumn(new TextColumn(4, ""));
            table.AddRow(row);
            foreach (var stream in _context.StreamModels)
            {
                Row newrow = new Row();
                newrow.AddColumn(new TextColumn(0, stream.StreamName));
                newrow.AddColumn(new TextColumn(1, stream.Type.ToString()));
                newrow.AddColumn(new StreamEditColumn(2, "Edit",stream));
                if(stream.Type == StreamProviderTypes.Twitch)
                {
                    newrow.AddColumn(new LinkColumn(3, "Authorisieren", $"/Stream/TwitchOAuth/{stream.ID}"));
                }
                else
                {
                    newrow.AddColumn(new TextColumn(3, ""));
                }
                newrow.AddColumn(new StreamDeleteColumn(4, "Delete", stream));
                
                table.AddRow(newrow);
            }
            return table.getJson();
        }

        /*
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
        }*/

        // POST: Streams2/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<bool> Create(string StreamName, StreamProviderTypes Type, int UpTime, int Quote, string Relay)
        {
            
            try
            {
                Stream newstream = new Stream();
                newstream.StreamName = StreamName;
                newstream.Type = Type;
                newstream.UpTimeInterval = UpTime;
                newstream.QuoteInterval = Quote;
                newstream.DiscordRelayChannel = Relay;
                _context.StreamModels.Add(newstream);
                _context.SaveChanges();
                //handleCreated(newstream);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
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
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<bool> DeleteStream(int? streamID)
        {
            if (streamID == null)
            {
                return false;
            }

            var stream = await _context.StreamModels
                .FirstOrDefaultAsync(m => m.ID == streamID);
            if (stream == null)
            {
                return false;
            }
            _context.StreamModels.Remove(stream);
            _context.SaveChanges();
            return true;

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
            StreamOAuthDataModel model = new StreamOAuthDataModel();
            model.Id = stream.ID.ToString();
            string baseurl = _configuration.GetSection("WebServerWebAddress").Value;
            model.RedirectLinkForTwitch = $"{baseurl}/Stream/TwitchReturnUrlAction";
            model.StatusMessage = StatusMessage;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<IActionResult> TwitchOAuth(int id, [Bind("Id,ClientId,Secret")] StreamOAuthDataModel StreamOAuthData)
        {
            if (id != Int32.Parse(StreamOAuthData.Id))
            {
                return NotFound();
            }

            if (ModelState.IsValid && StreamOAuthData.ClientId != "" && StreamOAuthData.Secret != "")
            {
                var baseUrl = _configuration.GetSection("WebServerWebAddress").Value;
                string state = StreamOAuthData.Id + StreamOAuthData.Secret;
                Stream stream = _context.StreamModels.Where(sm => sm.ID == Int32.Parse(StreamOAuthData.Id)).FirstOrDefault();
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
            Stream stream = _context.StreamModels.Where(sm => state.Contains(sm.Secret)).FirstOrDefault();
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
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
            random = new Random(DateTime.Now.Second*DateTime.Now.Millisecond/DateTime.Now.Hour);
        }
        [Authorize(Roles = "Dev,Admin")]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Dev")]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost("UploadStreams")]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> UploadStreamsPost(List<IFormFile> files)
        {

            // full path to file in temp location
            var filePath = Path.GetTempFileName();
            await SaveFileToLocalAsync(files, filePath);
            List<Legacy.internalStream> StreamList = ImportStreamList(filePath);
            
            
            if(!_context.StreamModels.Any())
            {
                foreach (var item in StreamList)
                {
                    _context.StreamModels.Add(GenerateStreamForImport(item));
                }
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
        private async Task<Boolean> SaveFileToLocalAsync(List<IFormFile> files,string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await files.FirstOrDefault()?.CopyToAsync(stream);
            }
            return true;
        }
        private  List<Legacy.internalStream> ImportStreamList(string filePath)
        {
            FileStream fsFile = new FileStream(filePath, FileMode.Open);
            XmlReader xReader = XmlReader.Create(fsFile);
            System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Legacy.internalStream>));

            List<Legacy.internalStream> StreamList = (List<Legacy.internalStream>)xmlserializer.Deserialize(xReader);
            fsFile.Close();
            return StreamList;
        }
        private Models.Stream GenerateStreamForImport(Legacy.internalStream internalStream)
        {
            Models.Stream newStream = new Models.Stream();
            newStream.StreamName = internalStream.sChannel;
            if (internalStream.bRelayActive)
            {
                newStream.RelayState = Models.Enum.RelayState.NotRunning;
            }
            else
            {
                newStream.RelayState = Models.Enum.RelayState.NotActivated;
            }
            newStream.DiscordRelayChannel = internalStream.sTargetrelaychannel;
            newStream.StreamState = Models.Enum.StreamState.NotRunning;
            newStream.Game = internalStream.sGame;
            newStream.UserID = internalStream.sUserID;
            newStream.Type = Models.Enum.StreamProviderTypes.Twitch;
            newStream.Url = "www.twitch.tv/" + internalStream.sChannel;
            newStream.StreamName = internalStream.sChannel;
            return newStream;
        }

        [HttpPost("UploadUsers")]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> UploadUsersPost(List<IFormFile> files)
        {

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            await SaveFileToLocalAsync(files, filePath);
            List<Legacy.User> UserList = ImportUserList(filePath);
            if (ShouldImport())
            {
                await InitializeRoles();
                //TODO: remove filter
                foreach (var oldUserModel in UserList)
                {
                    
                    ChatUserModel newUser = GenerateUserForImport(oldUserModel);
                    ImportSubscriptions(oldUserModel, newUser);
                    string password = GeneratePassword(newUser);
                    var insertResult = await InsertUserAsync(password, newUser);
                    await AddUserRoleAsync("user", newUser);

                }

                _context.SaveChanges();
            }


            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
            
            return View("Import");
        }
        private List<Legacy.User> ImportUserList(string filePath)
        {
            FileStream fsFile = new FileStream(filePath, FileMode.Open);
            XmlReader xReader = XmlReader.Create(fsFile);
            System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Legacy.User>));

            List<Legacy.User> UserList = (List<Legacy.User>)xmlserializer.Deserialize(xReader);
            fsFile.Close();
            return UserList;
        }
        private bool ShouldImport()
        {
            return (_context.ChatUserModels.Count() <= 2) && _context.StreamModels.Any();
        }
        private ChatUserModel GenerateUserForImport(Legacy.User user)
        {
            Models.ChatUserModel newUser = new Models.ChatUserModel();
            string cleanedname = Regex.Replace(user.Name.Replace(" ", "").Replace("/", "").Replace("\\", ""), @"[\[\]\\\^\$\.\|\?\*\+\(\)\{\}%,;><!@#&\-\+]", "");

            newUser.UserName = cleanedname;
            newUser.ChatUserName = user.Name;
            return newUser;
        }
        private void ImportSubscriptions(Legacy.User oldUserModel, ChatUserModel newUserModel)
        {
            newUserModel.StreamSubscriptions = new List<Models.StreamSubscription>();
            foreach (Legacy.Stream iStream in oldUserModel.Streams)
            {
                if (iStream.name.Length > 0)
                {
                    Models.StreamSubscription newSubscription = new Models.StreamSubscription();
                    newSubscription.Subscribed = (iStream.subscribed) ? Models.Enum.SubscriptionState.Subscribed : Models.Enum.SubscriptionState.Unsubscribed;
     
                    Models.Stream connectionstream = _context.StreamModels.Where(stream => stream.StreamName.ToLower() == iStream.name.ToLower()).FirstOrDefault();
                    if (connectionstream != null)
                    {
                        newSubscription.Stream = connectionstream;
                    }
                    newSubscription.User = newUserModel;
                    newUserModel.StreamSubscriptions.Add(newSubscription);
                }
            }
        }
        private string GeneratePassword(ChatUserModel User)
        {
            string password = "";

            password += User.ChatUserName.Substring(random.Next(0, User.ChatUserName.Length - 1));
            foreach(Models.StreamSubscription subscription in User.StreamSubscriptions)
            {
                Models.Stream stream = subscription.Stream;
                password += stream.StreamName.Substring(random.Next(0, stream.StreamName.Length - 1));
            }
            password += random.Next();
            return HashString(password);
        }
        private string HashString(string unhashed)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: unhashed,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
        private async Task<IdentityResult> InsertUserAsync(string password,ChatUserModel User)
        {
            var UserManager = _serviceProvider.GetRequiredService<UserManager<Models.ChatUserModel>>();
            return await UserManager.CreateAsync(User, password);
        }
        private async Task InitializeRoles()
        {
            string[] Roles = { "Dev", "Admin", "User" };
            foreach(string role in Roles)
            {
                await CreateRoleAsync(role);
            }
        }
        private async Task<bool> CreateRoleAsync(string role)
        {
            var RoleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if(!await RoleManager.RoleExistsAsync(role))
            {
                IdentityResult result = await RoleManager.CreateAsync(new IdentityRole(role));
                return result.Succeeded;
            }
            return true;
        }
        private async Task<bool> AddUserRoleAsync(string role,ChatUserModel user)
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<Models.ChatUserModel>>();
            var result = await userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        [Authorize(Roles = "Dev")]
        public IActionResult SecurityTokens()
        {
            ViewData["TokenTypes"] = new List<TokenType> { TokenType.Twitch,TokenType.Discord,TokenType.Mixer };
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> AddSecurityToken([Bind("ClientID,secret,service")] Models.SecurityToken token)
        {
            switch(token.service)
            {
                case TokenType.Twitch:
                    return await AquireTwitchToken(token);
                case TokenType.Discord:
                    await SaveDiscordToken(token);
                    break;
            }
            return RedirectToAction(nameof(SecurityTokens));
        }
        public async Task<IActionResult> AquireTwitchToken(SecurityToken token)
        {
            string baseurl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            if (_context.SecurityTokens.Where(st => st.service == token.service).Count() == 0)
            {
                _context.SecurityTokens.Add(token);
                await _context.SaveChangesAsync();
            }
            return Redirect($"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={token.ClientID}&redirect_uri={baseurl}/Admin/TwitchReturnUrlAction&scope=chat_login viewing_activity_read openid&state=c3ab8aa609ea11e793ae92361f002671");
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
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TwitchReturnUrlAction(string code, string scope, string state)
        {
            var savedtoken = _context.SecurityTokens.Where(st => st.service == BobDeathmic.Models.Enum.TokenType.Twitch).FirstOrDefault();
            savedtoken.code = code;
            var client = new HttpClient();
            var baseUrl = _configuration.GetSection("WebAdress").Value;
            string url = $"https://id.twitch.tv/oauth2/token?client_id={savedtoken.ClientID}&client_secret={savedtoken.secret}&code={code}&grant_type=authorization_code&redirect_uri={baseUrl}/Admin/TwitchReturnUrlAction";
            var response = await client.PostAsync(url, new StringContent("", System.Text.Encoding.UTF8,"text/plain"));
            var responsestring = await response.Content.ReadAsStringAsync();
            JSONObjects.TwitchAuthToken authtoken = JsonConvert.DeserializeObject<JSONObjects.TwitchAuthToken>(responsestring);
            savedtoken.token = authtoken.access_token;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(SecurityTokens));
        }
    }
}
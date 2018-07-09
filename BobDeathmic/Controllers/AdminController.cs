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
using Microsoft.Extensions.DependencyInjection;

namespace BobDeathmic.Controllers
{
    public class AdminController : Controller
    {

        private Data.ApplicationDbContext _context;
        private IServiceProvider _serviceProvider;
        private Random random;

        public AdminController(Data.ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            random = new Random(DateTime.Now.Second*DateTime.Now.Millisecond/DateTime.Now.Hour);
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Dev")]
        public IActionResult Import()
        {
            return View();
        }
        [BindProperty]
        public Models.ChatUserModel ChatUserModel { get; set; }
        public async Task<IActionResult> Edit()
        {
            ChatUserModel = _context.ChatUserModels.Where(x => x.Id.ToString() == RouteData.Values["id"].ToString()).First();
            return View();
        }

        [HttpPost("UploadStreams")]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> UploadStreamsPost(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }
            FileStream fsFile = new FileStream(filePath, FileMode.Open);
            XmlReader xReader = XmlReader.Create(fsFile);
            System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Legacy.internalStream>));

            List<Legacy.internalStream> StreamList = (List<Legacy.internalStream>) xmlserializer.Deserialize(xReader);
            fsFile.Close();
            
            if(!_context.StreamModels.Any())
            {
                foreach (var item in StreamList)
                {
                    Models.Stream newStream = new Models.Stream();
                    newStream.StreamName = item.sChannel;
                    if (item.bRelayActive)
                    {
                        newStream.RelayState = Models.Enum.RelayState.NotRunning;
                    }
                    else
                    {
                        newStream.RelayState = Models.Enum.RelayState.NotActivated;
                    }
                    newStream.DiscordRelayChannel = item.sTargetrelaychannel;
                    newStream.StreamState = Models.Enum.StreamState.NotRunning;
                    newStream.Game = item.sGame;
                    newStream.UserID = item.sUserID;
                    newStream.Type = Models.Enum.StreamProviderTypes.Twitch;
                    newStream.Url = "www.twitch.tv/" + item.sChannel;
                    newStream.StreamName = item.sChannel;
                    _context.StreamModels.Add(newStream);
                }
                _context.SaveChanges();
            }


            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return RedirectToAction(nameof(Index));
        }
        [HttpPost("UploadUsers")]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> UploadUsersPost(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }
            FileStream fsFile = new FileStream(filePath, FileMode.Open);
            XmlReader xReader = XmlReader.Create(fsFile);
            System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Legacy.User>));

            List<Legacy.User> UserList = (List<Legacy.User>)xmlserializer.Deserialize(xReader);
            fsFile.Close();
            var UserManager = _serviceProvider.GetRequiredService<UserManager<Models.ChatUserModel>>();
            if ((_context.ChatUserModels.Count() <= 2) && _context.StreamModels.Any())
            {
                //TODO: remove filter
                foreach (var item in UserList.Where(x => x.Name.ToLower() == "chromos33"))
                {
                    string password = "";
                    Models.ChatUserModel newUser = new Models.ChatUserModel();
                    string cleanedname = Regex.Replace(item.Name.Replace(" ", "").Replace("/","").Replace("\\",""), @"[\[\]\\\^\$\.\|\?\*\+\(\)\{\}%,;><!@#&\-\+]", "");

                    newUser.UserName = cleanedname;
                    newUser.ChatUserName = item.Name;
                    password += item.Name.Substring(random.Next(0, item.Name.Length-1));
                    newUser.StreamSubscriptions = new List<Models.StreamSubscription>();
                    foreach(Legacy.Stream iStream in item.Streams)
                    {
                        password += iStream.name.Substring(random.Next(0, iStream.name.Length - 1));
                        Models.StreamSubscription newSubscription = new Models.StreamSubscription();
                        if (iStream.subscribed)
                        {
                            newSubscription.Subscribed = Models.Enum.SubscriptionState.Subscribed;
                        }
                        else
                        {
                            newSubscription.Subscribed = Models.Enum.SubscriptionState.Unsubscribed;
                        }
                        Models.Stream connectionstream = _context.StreamModels.Where(stream => stream.StreamName.ToLower() == iStream.name.ToLower()).FirstOrDefault();
                        if(connectionstream != null)
                        {
                            newSubscription.Stream = connectionstream;
                        }
                        newSubscription.User = newUser;
                        newUser.StreamSubscriptions.Add(newSubscription);
                    }
                    password += random.Next();
                    byte[] salt = new byte[128 / 8];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(salt);
                    }
                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: password,
                        salt: salt,
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8));


                    newUser.InitialPassword = hashed;
                    var result = await UserManager.CreateAsync(newUser, hashed);
                    //TODO remove filter
                    if(item.Name.ToLower() == "chromos33")
                    {
                        await CreateOrAddUserRoles("Dev", "chromos33");
                    }
                    await CreateOrAddUserRoles("User", newUser.UserName);
                }

                _context.SaveChanges();
            }


            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
            
            return View("Import");
        }
        private async Task CreateOrAddUserRoles(string role, string name)
        {
            try
            {
                var RoleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var UserManager = _serviceProvider.GetRequiredService<UserManager<Models.ChatUserModel>>();

                IdentityResult roleResult;
                //Adding Admin Role
                var roleCheck = await RoleManager.RoleExistsAsync(role);
                if (!roleCheck)
                {
                    //create the roles and seed them to the database
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(role));
                }
                //Assign Admin role to the main User here we have given our newly registered 
                //login id for Admin management
                Models.ChatUserModel user = await UserManager.FindByNameAsync(name);
                await UserManager.AddToRoleAsync(user, role);
            }catch(Exception)
            {
                Console.WriteLine(name);
                Console.WriteLine(role);
            }
            
        }

        [Authorize(Roles = "Dev")]
        public IActionResult SecurityTokens()
        {
            ViewData["TokenTypes"] = new List<TokenType> { TokenType.Twitch,TokenType.Discord,TokenType.Mixer };
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> AddSecurityToken([Bind("ClientID,service")] Models.SecurityToken token)
        {
            switch(token.service)
            {
                case TokenType.Twitch:
                    string baseurl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                    if(_context.SecurityTokens.Where(st => st.service == token.service).Count() == 0)
                    {
                        _context.SecurityTokens.Add(token);
                        await _context.SaveChangesAsync();
                    }
                    return Redirect($"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={token.ClientID}&redirect_uri={baseurl}/admin/TwitchReturnUrlAction&scope=viewing_activity_read+openid&state=c3ab8aa609ea11e793ae92361f002671");
                case TokenType.Discord:
                    if(_context.SecurityTokens.Where(st => st.service == token.service).Count() == 0)
                    {
                        _context.SecurityTokens.Add(token);
                    }
                    else
                    {
                       var oldtoken = _context.SecurityTokens.Where(st => st.service == token.service).FirstOrDefault();
                       oldtoken.token = token.ClientID;
                    }
                    await _context.SaveChangesAsync();
                    break;
            }
            return RedirectToAction(nameof(SecurityTokens));
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TwitchReturnUrlAction(string code)
        {
            var test = _context.SecurityTokens.Where(st => st.service == BobDeathmic.Models.Enum.TokenType.Twitch).FirstOrDefault();
            test.token = code;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(SecurityTokens));
        }
    }
}
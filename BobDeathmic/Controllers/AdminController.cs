﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
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
                    Models.StreamProvider newProvider = new Models.StreamProvider();
                    newProvider.Name = "Twitch";
                    newProvider.UserID = item.sUserID;
                    newProvider.UserName = item.sChannel;
                    newProvider.Stream = newStream;
                    if(newStream.StreamProvider == null)
                    {
                        newStream.StreamProvider = new List<Models.StreamProvider>();
                    }
                    newStream.StreamProvider.Add(newProvider);



                    _context.StreamProviders.Add(newProvider);
                    _context.StreamModels.Add(newStream);
                }
                _context.SaveChanges();
            }
                
            
            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = files.Count, size, filePath });
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
                    foreach(Legacy.LegacyStream iStream in item.Streams)
                    {
                        password += iStream.name.Substring(random.Next(0, iStream.name.Length - 1));
                        Models.StreamSubscription newSubscription = new Models.StreamSubscription();
                        if (iStream.subscribed)
                        {
                            newSubscription.Subscribed = Models.Enum.SubscriptionState.Subscribed;
                        }
                        else
                        {
                            newSubscription.Subscribed = Models.Enum.SubscriptionState.Unsubscribted;
                        }
                        Models.Stream connectionstream = _context.StreamModels.Where(stream => stream.StreamName.ToLower() == iStream.name.ToLower()).FirstOrDefault();
                        if(connectionstream != null)
                        {
                            newSubscription.Stream = connectionstream;
                        }
                        newSubscription.User = newUser;
                        newUser.StreamSubscriptions.Add(newSubscription);
                    }
                    SHA256 mySHA256 = SHA256Managed.Create();
                    password += random.Next();
                    //string hashedpw = mySHA256.ComputeHash(System.Text.Encoding.Unicode.GetBytes(password)).ToString();
                    string hashedpw = "kermit22";
                    var result = await UserManager.CreateAsync(newUser, hashedpw);
                    //TODO remove filter
                    if(item.Name.ToLower() == "chromos33" && false)
                    {
                        await CreateOrAddUserRoles("Dev", "chromos33");
                    }
                    await CreateOrAddUserRoles("User", newUser.UserName);
                }

                _context.SaveChanges();
            }


            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            ViewData["UserListe"] = _context.ChatUserModels.ToList();
            return View("UserList");
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
    }
}
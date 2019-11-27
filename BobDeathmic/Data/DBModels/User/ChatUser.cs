using BobDeathmic.Data.DBModels.EventCalendar;
using BobDeathmic.Data.DBModels.EventCalendar.manymany;
using BobDeathmic.Data.DBModels.GiveAway;
using BobDeathmic.Data.DBModels.GiveAway.manymany;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.Enums.Stream;
using BobDeathmic.Models.Events;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BobDeathmic.Data.DBModels.User
{
    // Add profile data for application users by adding properties to the ChatUserModel class
    public class ChatUserModel : IdentityUser
    {

        public string ChatUserName { get; set; }
        public List<StreamSubscription> StreamSubscriptions { get; set; }
        public string InitialPassword { get; set; }
        public List<Stream> OwnedStreams { get; set; }

        public List<GiveAwayItem> OwnedItems { get; set; }

        public List<GiveAwayItem> ReceivedItems { get; set; }
        public List<User_GiveAwayItem> AppliedTo { get; set; }

        public List<ChatUserModel_Event> Calendars { get; set; }
        public List<Event> AdministratedCalendars { get; set; }
        public List<AppointmentRequest> AppointmentRequests { get; set; }

        public ChatUserModel(): this(new List<Stream>())
        {}
        public ChatUserModel(string username) : this(username, new List<Stream>())
        {}
        public ChatUserModel(IEnumerable<Stream> Streams): this(String.Empty,Streams)
        {}
        public ChatUserModel(string username,IEnumerable<Stream> Streams)
        {
            ChatUserName = username;
            UserName = cleanedUserName(username);
            StreamSubscriptions = new List<StreamSubscription>();
            foreach (Stream stream in Streams)
            {
                StreamSubscriptions.Add(new StreamSubscription(stream, SubscriptionState.Subscribed));
            }
        }
        public void SetInitialPassword()
        {
            if(InitialPassword == "" || InitialPassword == null)
            {
                InitialPassword = GeneratePassword();
            }
        }
        public string GeneratePassword()
        {
            byte[] salt = new byte[128 / 8];
            byte[] pwd = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
                rng.GetBytes(pwd);
            }
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: pwd.ToString(),
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

            return hashed;
        }
        private string cleanedUserName(string username)
        {
            return Regex.Replace(username.Replace(" ", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty), @"[\[\]\\\^\$\.\|\?\'\*\+\(\)\{\}%,;><!@#&\-\+]", string.Empty);
        }

        public bool IsSubscribed(string streamname)
        {
            if(streamname == null || streamname == "")
            {
                throw new ArgumentException("StreamName must be not be null or empty");
            }
            if (StreamSubscriptions != null && StreamSubscriptions.Where(ss => ss.Stream != null && ss.Stream.StreamName.ToLower() == streamname.ToLower() && ss.Subscribed == SubscriptionState.Subscribed).FirstOrDefault() != null)
            {
                return true;
            }
            return false;
        }

    }
}

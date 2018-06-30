using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BobDeathmic.Models
{
    // Add profile data for application users by adding properties to the ChatUserModel class
    public class ChatUserModel : IdentityUser
    {
        public string ChatUserName { get; set; }
    }
}

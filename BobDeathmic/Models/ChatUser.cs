using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models
{
    public class ChatUser : IdentityUser
    {
        public string Name { get; set; }
    }
}

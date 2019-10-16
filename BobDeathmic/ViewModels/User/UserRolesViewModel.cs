using BobDeathmic.Data.DBModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.User
{
    public class UserRolesViewModel
    {
        public ChatUserModel User { get; set; }
        public IList<string> Roles { get; set; }
    }
}

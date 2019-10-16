using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Data.DBModels.User
{
    public class DiscordBan
    {
        public int Id { get; set; }
        public ulong DiscordID { get; set; }
    }
}

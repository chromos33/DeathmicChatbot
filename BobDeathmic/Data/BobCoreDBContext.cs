using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Models;

namespace BobDeathmic.Data
{
    public class BobCoreDBContext : DbContext
    {
        public BobCoreDBContext (DbContextOptions<BobCoreDBContext> options):base (options)
        {

        }
        public DbSet<BobDeathmic.Models.ExternalAuthData> AuthData { get; set; }
        public DbSet<Models.ChatUser> ChatUser { get; set; }
    }
}

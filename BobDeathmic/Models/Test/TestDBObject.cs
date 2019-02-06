using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.Test
{
    public class TestDBObject
    {
        [Key]
        public string Id { get; set; }

        public string OwnerID { get; set; }
        public ChatUserModel Owner { get; set; }
    }
}

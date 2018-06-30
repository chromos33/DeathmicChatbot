using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models
{
    public class ExternalAuthData
    {
        public int ID { get; set; }
        public string Token { get; set; }
        public string Login { get; set; }
        public string Platform { get; set; }
    }
}

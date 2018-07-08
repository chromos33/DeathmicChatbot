using BobDeathmic.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models
{
    public class SecurityToken
    {
        public int ID { get; set; }
        public string ClientID { get; set; }
        public string token { get; set; }
        public TokenType service { get; set; }
    }
}

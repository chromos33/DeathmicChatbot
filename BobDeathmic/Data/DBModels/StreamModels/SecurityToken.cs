using BobDeathmic.Data.Enums.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Data.DBModels.StreamModels
{
    public class SecurityToken
    {
        public int ID { get; set; }
        public string ClientID { get; set; }
        public string token { get; set; }
        public TokenType service { get; set; }
        public string code { get; set; }
        public string secret { get; set; }
        public string RefreshToken { get; set; }
    }
}

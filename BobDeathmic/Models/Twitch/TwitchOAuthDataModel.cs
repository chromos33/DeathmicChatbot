using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.Twitch
{
    public class TwitchOAuthDataModel
    {
        [Required]
        [DataType(DataType.Text)]
        public string client_id { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Helper
{
    public class AppConfig
    {
        public Command[] Commands { get; set; }
    }
    public class Command
    {
        public string Name { get; set; }
        public bool Twitch { get; set; }
        public bool Discord { get; set; }
    }
}

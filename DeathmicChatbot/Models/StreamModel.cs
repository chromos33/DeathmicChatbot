using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathmicChatbot.Models
{
    class StreamModel
    {
        public string Channel { get; set; }
        public string Message { get; set; }
        public string Game { get; set; }
        public DateTime starttime { get; set; }
    }
}

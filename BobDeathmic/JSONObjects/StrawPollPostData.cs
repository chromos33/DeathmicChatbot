using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.JSONObjects
{
    public class StrawPollPostData
    {
        public string title { get; set; }
        public string[] options { get; set; }
        public bool multi { get; set; }
    }
}

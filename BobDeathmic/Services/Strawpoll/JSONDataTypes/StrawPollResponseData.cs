using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.JSONObjects
{
    public class StrawPollResponseData
    {
        public double id { get; set; }
        public string title { get; set; }
        public string[] options { get; set; }
        public int[] votes { get; set; }
        public bool multi { get; set; }
        public string dupcheck { get; set; }
        public string captcha { get; set; }
        public string Url()
        {
            return "https://www.strawpoll.me/" + id;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models
{
    public class StreamProvider
    {
        public int ID { get; set; }
        public Stream Stream { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string UserID { get; set; }
        public string Url { get; set; }
    }
}

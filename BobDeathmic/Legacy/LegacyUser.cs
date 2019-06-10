using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Legacy
{
    public class User
    {
        public string Name { get; set; }
        public int VisitCounter;
        public bool bIsLoggingOp;
        public string password;
        public List<Stream> Streams { get; set; }
        public List<Alias> Aliase { get; set; }
        public bool bMessages { get; set; }
        public bool globalhourlyannouncement { get; set; }
        public bool bIsAdmin { get; set; }
    }

    public class Stream
    {
        public string name;
        public bool subscribed;
        public bool hourlyannouncement;
    }
    public class Alias
    {
        public string Name;
        public int VisitCounterBackup;
        public DateTime? LastVisitBackup;
    }
}

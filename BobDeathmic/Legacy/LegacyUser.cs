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
        public List<LegacyStream> Streams { get; set; }
        public List<LegacyAlias> Aliase { get; set; }
        public bool bMessages { get; set; }
        public bool globalhourlyannouncement { get; set; }
        public bool bIsAdmin { get; set; }
    }

    public class LegacyStream
    {
        public string name;
        public bool subscribed;
        public bool hourlyannouncement;
    }
    public class LegacyAlias
    {
        public string Name;
        public int VisitCounterBackup;
        public DateTime? LastVisitBackup;
    }
}

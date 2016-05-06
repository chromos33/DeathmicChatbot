using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathmicChatbot.DataFiles
{
    public class Alias
    {
        public string Name;
        public int VisitCounterBackup;
        public DateTime LastVisitBackup;
        public Alias(string name,int visitCounterBackup,DateTime lastVisitBackup)
        {
            Name = name;
            VisitCounterBackup = visitCounterBackup;
            LastVisitBackup = lastVisitBackup;
        }
        public Alias()
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Other
{
    public class SubscribableStream
    {
        public string Name { get; set; }
        public int StreamID { get; set; }

        public SubscribableStream(string name, int id)
        {
            Name = name;
            StreamID = id;
        }
    }
}

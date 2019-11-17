using BobDeathmic.Data.DBModels.StreamModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.StreamModels
{
    public class StreamListDataModel
    {
        public List<Stream> StreamList { get; set; }
        public string StatusMessage { get; set; }
    }
}

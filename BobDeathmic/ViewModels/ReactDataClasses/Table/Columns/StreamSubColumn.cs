using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table.Columns
{
    public class StreamSubColumn : Column
    {
        public string ReactComponentName { get { return "StreamSubColumn"; } }

        public int key { get; set; }
        public bool Status { get; set; }
        public int SubID { get; set; }
        public StreamSubColumn(int key, bool Status,int SubID)
        {
            this.key = key;
            this.Status = Status;
            this.canSort = false;
            this.SubID = SubID;
        }
    }
}

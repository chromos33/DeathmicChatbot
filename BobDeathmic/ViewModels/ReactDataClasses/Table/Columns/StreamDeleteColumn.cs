using BobDeathmic.Data.DBModels.Relay;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.Enums.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table.Columns
{

    public class StreamDeleteColumn : Column
    {
        public string ReactComponentName { get { return "StreamDeleteColumn"; } }

        public int key { get; set; }
        public string Text { get; set; }

        public int StreamID { get; set; }

        public StreamDeleteColumn(int key, string Text, Stream stream)
        {
            this.key = key;
            this.Text = Text;
            this.canSort = false;
            this.StreamID = stream.ID;
        }
    }
}

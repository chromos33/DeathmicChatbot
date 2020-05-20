using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table.Columns
{
    public class StreamCommandEditColumn : Column
    {

        public string ReactComponentName { get { return "StreamCommandEditColumn"; } }

        public string Text { get; set; }
        public int CommandID { get; set; }

        public StreamCommandEditColumn(int key, string Text,int CommandID)
        {
            this.Key = key;
            this.Text = Text;
            this.canSort = false;
            this.CommandID = CommandID;
        }
    }
}

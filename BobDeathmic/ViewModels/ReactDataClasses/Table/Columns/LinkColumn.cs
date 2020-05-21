using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table.Columns
{
    public class LinkColumn: Column
    {
        public string ReactComponentName { get { return "LinkColumn"; } }

        public int key { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        public LinkColumn(int key, string Text, string Link)
        {
            this.key = key;
            this.Text = Text;
            this.canSort = false;
            this.Link = Link;
        }
    }
}

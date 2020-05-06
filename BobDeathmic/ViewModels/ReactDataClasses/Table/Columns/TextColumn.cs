using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table.Columns
{
    public class TextColumn : Column
    {
        public string ReactComponentName { get { return "TextColumn"; } }

        public int key { get; set; }
        public string Text { get; set; }
        public TextColumn(int key, string Text, bool canSort = false)
        {
            this.key = key;
            this.Text = Text;
            this.canSort = canSort;
        }
    }
}

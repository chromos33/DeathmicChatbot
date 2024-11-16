using BobDeathmic.Data.DBModels.StreamModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table.Columns
{
    public class ObjectDeleteColumn: Column
    {
        public string ReactComponentName { get { return "ObjectDeleteColumn"; } }

        public int key { get; set; }
        public string Text { get; set; }

        public string DeleteLink { get; set; }
        public string DeleteText { get; set; }

        public ObjectDeleteColumn(int key, string Text, string DeleteLink,string DeleteText)
        {
            this.key = key;
            this.Text = Text;
            this.canSort = false;
            this.DeleteLink = DeleteLink;
            this.DeleteText = DeleteText;
        }
    }
}

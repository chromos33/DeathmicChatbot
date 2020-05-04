using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table
{
    public class Row
    {
        public List<Columns.Column> Columns { get;}

        public Row()
        {
            Columns = new List<Columns.Column>();
        }
        public void AddColumn(Columns.Column column)
        {
            Columns.Add(column);
        }
    }
}

using BobDeathmic.ViewModels.ReactDataClasses.Table.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table
{
    public class Row
    {
        public List<Columns.Column> Columns { get;}

        public string Filter { get; private set; }

        public bool canFilter { get; set; }

        public bool isStatic { get; set; }

        public Row(bool canFilter = true, bool isStatic = false)
        {
            Columns = new List<Columns.Column>();
            this.canFilter = canFilter;
            this.isStatic = isStatic;
        }
        public void AddColumn(Columns.Column column)
        {
            var test = column.GetType().ToString();

            if (canFilter && (Filter == "" || Filter == null) && column.GetType() == typeof(TextColumn))
            {
                Filter = ((TextColumn)column).Text;
            }
            Columns.Add(column);
            
        }
    }
}

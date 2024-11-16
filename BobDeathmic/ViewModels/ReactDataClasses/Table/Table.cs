using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table
{
    public class Table
    {
        public List<Row> Rows { get;}

        public Table()
        {
            Rows = new List<Row>();
        }
        public void AddRow(Row row)
        {
            Rows.Add(row);
        }

        public string getJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

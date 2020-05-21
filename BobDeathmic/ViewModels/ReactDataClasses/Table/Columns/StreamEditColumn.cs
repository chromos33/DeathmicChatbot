using BobDeathmic.Data.DBModels.Relay;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.Enums.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Table.Columns
{

    public class StreamEditColumn: Column
    {
        public string ReactComponentName { get { return "StreamEditColumn"; } }

        public int key { get; set; }
        public string Text { get; set; }

        public int StreamID { get; set; }

        public StreamEditColumn(int key, string Text, Stream stream)
        {
            this.key = key;
            this.Text = Text;
            this.canSort = false;
            this.StreamID = stream.ID;
        }
    }
    public class StreamEditData
    {
        public string StreamName { get; set; }

        public string[] StreamTypes { get; }
        public StreamProviderTypes Type { get; set; }

        public string RelayChannel { get; set; }

        public string[] RelayChannels { get; }

        public int UpTimeInterval { get; set; }

        public int QuoteInterval { get; set; }

        public StreamEditData(Stream stream,RelayChannels[] channels)
        {
            this.StreamTypes = stream.EnumStreamTypes().ToArray();
            this.Type = stream.Type;
            this.UpTimeInterval = stream.UpTimeInterval;
            this.QuoteInterval = stream.QuoteInterval;

        }
    }
}

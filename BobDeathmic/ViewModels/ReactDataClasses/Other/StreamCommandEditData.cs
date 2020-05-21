using BobDeathmic.Data.DBModels.StreamModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.ReactDataClasses.Other
{
    public class StreamCommandEditData
    {
        public string Name { get; set; }
        public string Response { get; set; }
        public string Mode { get; set; }
        public int AutoInterval { get; set; }

        public string StreamName { get; set; }
        public int StreamID { get; set; }
        public List<StreamData> Streams { get; set; }

        public StreamCommandEditData(StreamCommand command,IEnumerable<Stream> Streamlist)
        {
            Name = command.name;
            Response = command.response;
            Mode = command.Mode.ToString();
            AutoInterval = command.AutoInverval;
            StreamName = command.stream.StreamName;
            StreamID = command.stream.ID;
            Streams = new List<StreamData>();
            foreach(Stream stream in Streamlist)
            {
                Streams.Add(new StreamData(stream.StreamName,stream.ID));
            }
        }
        public StreamCommandEditData(IEnumerable<Stream> Streamlist)
        {
            Name = "";
            Response = "";
            Mode = "";
            AutoInterval = 0;
            StreamName = "";
            StreamID = 0;
            Streams = new List<StreamData>();
            foreach (Stream stream in Streamlist)
            {
                Streams.Add(new StreamData(stream.StreamName, stream.ID));
            }
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class StreamData
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public StreamData(string Name,int ID)
        {
            this.Name = Name;
            this.ID = ID;
        }
    }
}

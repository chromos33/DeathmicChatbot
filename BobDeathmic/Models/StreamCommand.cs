using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models
{
    public class StreamCommand
    {
        public int ID { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [MinLength(3)]
        public string name { get; set; }
        [Required]
        [MinLength(3)]
        public string response { get; set; }
        public StreamCommandMode Mode { get; set; }
        public int AutoInverval { get; set; }
        [Required]
        public int streamID { get; set; }
        public Stream stream { get; set; }
        private List<SelectListItem> SelectableStreams { get; set; }
        public List<SelectListItem> GetSelectableStreams()
        {
            return SelectableStreams;
        }
        public void SetSelectableStreams(List<SelectListItem> List)
        {
            SelectableStreams = List;
        }
    }
    public enum StreamCommandMode
    {
        Auto = 0,
        Manual = 1
    }
}

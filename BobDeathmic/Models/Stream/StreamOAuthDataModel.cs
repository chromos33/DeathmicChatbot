using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.StreamViewModels
{
    public class StreamOAuthDataModel
    {
        [Required]
        [DataType(DataType.Text)]
        public string Id { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string ClientId { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Secret { get; set; }
    }
}

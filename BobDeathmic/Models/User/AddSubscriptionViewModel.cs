using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Models.User
{
    public class AddSubscriptionViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Stream")]
        public string StreamNameForSubscription { get; set; }
    }
}

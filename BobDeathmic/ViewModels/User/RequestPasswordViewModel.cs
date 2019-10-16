using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.User
{
    public class RequestPasswordViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Nutzername")]
        public string UserName { get; set; }
    }
}

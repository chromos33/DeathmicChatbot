using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ViewModels.User
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Momentanes Passwort")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Das {0} muss mindestens zwischen {2} und {1} Zeichen lang sein.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Neues Passwort")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Passwort bestätigen")]
        [Compare("NewPassword", ErrorMessage = "Die Passwörter stimmen nicht überein.")]
        public string ConfirmPassword { get; set; }

        public string StatusMessage { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace www.ViewModels.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Le champ {0} est requis")]
        [EmailAddress]
        [Display(Name = "Adresse email")]
        public string Email { get; set; }
    }
}

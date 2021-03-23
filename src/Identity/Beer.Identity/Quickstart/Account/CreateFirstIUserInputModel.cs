
using Beer.Identity.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Quickstart.UI
{
    public class CreateFirstIUserInputModel 
    {
        [Required(ErrorMessage = "The {0} field is required")]
        [Display(Name = "Username")]
        public String Username { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [Display(Name = "Password")]
        public String Password { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [Display(Name = "Password Confirmation")]
        [Compare(nameof(Password), ErrorMessage =  "Password are not the same")]
        public String PasswordConfirmation { get; set; }

        [MinLength(3,ErrorMessage = "not less than {0} characters")]
        [MaxLength(200,ErrorMessage = "not more than {0} characters")]
        [Required(ErrorMessage = "The {0} field is required")]
        [Display(Name = "Display Name")]
        public String DisplayName { get; set; }

        [Display(Name = "Profile Picutre")]
        [Required(ErrorMessage = "The {0} field is required")]
        public String ProfilePictureUrl { get; set; }

        public String ReturnUrl { get; set; }
    }
}
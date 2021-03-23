// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Quickstart.UI
{
    public class LoginInputModel
    {
        [Required(ErrorMessage = "The {0} field is required")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [Display(Name = "Password")]
        public string Password { get; set; }
        
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}
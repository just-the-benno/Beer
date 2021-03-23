using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorApp.Pages.Users
{
    public class CreateNewUserViewModel
    {
        public String Loginname { get; set; }
        public String DisplayName { get; set; }
        public String Password { get; set; }
        public String PasswordConfirmation { get; set; }
        public String AvatarUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorApp.Services.Requests
{
    public class BeerUserRequests
    {
        public static class V1
        {
            public class ResetPasswordRequest
            {
                public String Password { get; set; }
            }

            public class CreateBeerUserRequest
            {
                public String Username { get; set; }
                public String Password { get; set; }
                public String DisplayName { get; set; }
                public String ProfilePictureUrl { get; set; }
            }
        }
    }
}

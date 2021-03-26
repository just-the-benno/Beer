using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.Service.API.Infrastrucutre
{
    public class JwtTokenAuthenticationOptions
    {
        public String AuthorityUrl { get; set; }
    }

    public class AppSettings
    {
        public JwtTokenAuthenticationOptions JwtTokenAuthenticationOptions { get; set; }
        public Dictionary<String, String> AppURIs { get; set; }
    }
}

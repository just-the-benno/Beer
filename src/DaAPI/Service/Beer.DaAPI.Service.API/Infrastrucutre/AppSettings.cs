using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Infrastrucutre
{
    public class JwtTokenAuthenticationOptions
    {
        public String AuthorityUrl { get; set; }
    }

    public class EventStoreSettings
    {
        public String Prefix { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
    }

    public class AppSettings
    {
        public JwtTokenAuthenticationOptions JwtTokenAuthenticationOptions { get; set; }
        public Dictionary<String,String> AppURIs { get; set; }
        public EventStoreSettings EventStoreSettings { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Configuration
{
    public class AppConfiguration
    {
        public OpenIdConnectConfiguration OpenIdConnectConfiguration { get; set; }
        public Dictionary<String, BeerAuthenticationClient> BeerAuthenticationClients { get; set; }
    }
}

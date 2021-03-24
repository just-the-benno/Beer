using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Configuration
{
    public class IdentityServerOptions
    {
        public String SigningCertificate { get; set; }
        public String ValidationCertificate { get; set; }
    }

    public class AppConfiguration
    {
        public IdentityServerOptions IdentityServerOptions { get; set; }
        public OpenIdConnectConfiguration OpenIdConnectConfiguration { get; set; }
        public Dictionary<String, BeerAuthenticationClient> BeerAuthenticationClients { get; set; }
    }
}

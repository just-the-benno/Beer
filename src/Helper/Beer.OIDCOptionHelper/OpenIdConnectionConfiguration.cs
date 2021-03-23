using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;

namespace Beer.OIDCOptionHelper
{
    public class OpenIdConnectionConfiguration : OidcProviderOptions
    {
        public IList<String> Scopes { get; set; }
    }
}

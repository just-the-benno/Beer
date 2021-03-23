using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.WASM.Helper
{
    public class ConfigurableAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public ConfigurableAuthorizationMessageHandler(String uri, IAccessTokenProvider provider, NavigationManager navigation) :
            base(provider, navigation)
        {
            ConfigureHandler(new String[] { uri });
        }
    }
}

using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public class DHCPv6ScopePropertyTypeNameConverter
    {
        private readonly IStringLocalizer<DHCPv6ScopePropertyTypeNameConverter> _localizer;

        public DHCPv6ScopePropertyTypeNameConverter(IStringLocalizer<DHCPv6ScopePropertyTypeNameConverter> localizer)
        {
            this._localizer = localizer;
        }

        public String GetName(DHCPv6ScopePropertyType property) => _localizer[property.ToString()];
    }
}

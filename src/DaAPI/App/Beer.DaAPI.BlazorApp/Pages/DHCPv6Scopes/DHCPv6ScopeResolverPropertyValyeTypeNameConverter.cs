using Beer.DaAPI.Core.Scopes.DHCPv4;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public class DHCPv6ScopeResolverPropertyValyeTypeNameConverter
    {
        private readonly IStringLocalizer<DHCPv6ScopeResolverPropertyValyeTypeNameConverter> _localizer;

        public DHCPv6ScopeResolverPropertyValyeTypeNameConverter(IStringLocalizer<DHCPv6ScopeResolverPropertyValyeTypeNameConverter> localizer)
        {
            this._localizer = localizer;
        }

        public String GetName(ScopeResolverPropertyValueTypes property) => _localizer[property.ToString()];
    }
}

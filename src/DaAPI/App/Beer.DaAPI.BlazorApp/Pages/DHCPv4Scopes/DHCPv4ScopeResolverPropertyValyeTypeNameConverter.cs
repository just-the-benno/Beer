using Beer.DaAPI.Core.Scopes.DHCPv4;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public class DHCPv4ScopeResolverPropertyValyeTypeNameConverter
    {
        private readonly IStringLocalizer<DHCPv4ScopeResolverPropertyValyeTypeNameConverter> _localizer;

        public DHCPv4ScopeResolverPropertyValyeTypeNameConverter(IStringLocalizer<DHCPv4ScopeResolverPropertyValyeTypeNameConverter> localizer)
        {
            this._localizer = localizer;
        }

        public String GetName(ScopeResolverPropertyValueTypes property) => _localizer[property.ToString()];
    }
}

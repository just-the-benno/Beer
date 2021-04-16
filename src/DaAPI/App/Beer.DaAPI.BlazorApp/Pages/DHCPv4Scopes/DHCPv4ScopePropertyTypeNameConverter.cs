using Beer.DaAPI.Core.Scopes.DHCPv4;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public class DHCPv4ScopePropertyTypeNameConverter
    {
        private readonly IStringLocalizer<DHCPv4ScopePropertyTypeNameConverter> _localizer;

        public DHCPv4ScopePropertyTypeNameConverter(IStringLocalizer<DHCPv4ScopePropertyTypeNameConverter> localizer)
        {
            this._localizer = localizer;
        }

        public String GetName(DHCPv4ScopePropertyType property) => _localizer[property.ToString()];
    }
}

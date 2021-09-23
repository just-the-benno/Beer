using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv6LeasesResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public partial class DHCPv6ScopeDetailsPage
    {
        [Parameter] public String ScopeId { get; set; }

        private DHCPv6ScopePropertiesResponse _properties;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _properties = await _service.GetDHCPv6ScopeProperties(ScopeId, true);
        }
    }
}



using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv4LeasesResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public partial class DHCPv4ScopeDetailsPage
    {
        [Parameter] public String ScopeId { get; set; }

        private DHCPv4ScopePropertiesResponse _properties;

        protected override async Task OnParametersSetAsync()
        {
            _properties = await _service.GetDHCPv4ScopeProperties(ScopeId, true);
        }
    }
}

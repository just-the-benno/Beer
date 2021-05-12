using Microsoft.AspNetCore.Components;
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
        private IEnumerable<DHCPv6LeaseOverview> _leases;

        private IEnumerable<DHCPv6LeaseOverview> GetActiveLeases() => _leases.Where(x => x.State == Core.Scopes.LeaseStates.Active).OrderBy(x => x.Address).ToArray();
        private IEnumerable<DHCPv6LeaseOverview> GetPendingLeases() => _leases.Where(x => x.State == Core.Scopes.LeaseStates.Pending).OrderBy(x => x.Address).ToArray();

        protected override async Task OnParametersSetAsync()
        {
            _leases = null;
            _properties = null;

            _leases = await _service.GetDHCPv6LeasesByScope(ScopeId, true);
            _properties = await _service.GetDHCPv6ScopeProperties(ScopeId, true);
        }
    }
}

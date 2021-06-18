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
        private IEnumerable<DHCPv6LeaseOverview> _leases;

        private IEnumerable<DHCPv6LeaseOverview> GetActiveLeases() => _leases.Where(x => x.State == Core.Scopes.LeaseStates.Active).OrderBy(x => x.Address).ToArray();
        private IEnumerable<DHCPv6LeaseOverview> GetPendingLeases() => _leases.Where(x => x.State == Core.Scopes.LeaseStates.Pending).OrderBy(x => x.Address).ToArray();

        private void Reset(Boolean updateState)
        {
            _leases = null;
            _properties = null;

            if (updateState == true)
            {
                StateHasChanged();
            }
        }

        private async Task LoadLeases()
        {
            _leases = await _service.GetDHCPv6LeasesByScope(ScopeId, true);
            _properties = await _service.GetDHCPv6ScopeProperties(ScopeId, true);
        }

        protected override async Task OnParametersSetAsync()
        {
            Reset(false);

            await LoadLeases();
        }

        public async Task CancelLease(DHCPv6LeaseOverview lease)
        {
            var result = await _service.SendCancelDHCPv6LeaseRequest(lease.Id);

            if (result == true)
            {
                _snackBarService.Add(String.Format(L["CancelLeaseSuccessSnackbarContent"], lease.Address), Severity.Success);

                Reset(true);
                await LoadLeases();
            }
            else
            {
                _snackBarService.Add(String.Format(L["CancelLeaseFailureSnackbarContent"], lease.Address), Severity.Error);
            }
        }
    }
}


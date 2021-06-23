
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
        private IEnumerable<DHCPv4LeaseOverview> _leases;

        private IEnumerable<DHCPv4LeaseOverview> GetActiveLeases() => _leases.Where(x => x.State == Core.Scopes.LeaseStates.Active).OrderBy(x => x.Address).ToArray();
        private IEnumerable<DHCPv4LeaseOverview> GetPendingLeases() => _leases.Where(x => x.State == Core.Scopes.LeaseStates.Pending).OrderBy(x => x.Address).ToArray();

        private void Reset(Boolean updateState)
        {
            _leases = null;
            _properties = null;

            if(updateState == true)
            {
                StateHasChanged();
            }
        }

        private async Task LoadLeases()
        {
            _leases = await _service.GetDHCPv4LeasesByScope(ScopeId, true);
            _properties = await _service.GetDHCPv4ScopeProperties(ScopeId, true);
        }

        protected override async Task OnParametersSetAsync()
        {
            Reset(false);
            await LoadLeases();
        }

        public async Task CancelLease(DHCPv4LeaseOverview lease)
        {
            var result = await _service.SendCancelDHCPv4LeaseRequest(lease.Id);

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

using Beer.DaAPI.BlazorApp.Helper;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv6InterfaceResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Interfaces
{
    public partial class DHCPv6InterfacesPage
    {
        private DHCPv6InterfaceOverview _interfaceOverview;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _interfaceOverview = await _service.GetDHCPv6Interfaces();
        }

        public async Task ShowAddInterfaceDialog(DHCPv6InterfaceEntry entry)
        {
            var parameters = new DialogParameters
            {
                { nameof(CreateDHCPv6InterfaceDialog.Entry), entry }
            };

            var messageForm = _dialogService.Show<CreateDHCPv6InterfaceDialog>(String.Format(L["CreateInterfaceDialogHeader"], entry.InterfaceName), parameters);
            var result = await messageForm.Result;

            if (result.IsSuccess() == true)
            {
                _snackBarService.Add(String.Format(L["CreateInterfaceSuccessSnackbarContent"], entry.IPv6Address), Severity.Success);
                _interfaceOverview = await _service.GetDHCPv6Interfaces();
            }
        }

        public async Task ShowDeleteDialog(ActiveDHCPv6InterfaceEntry entry)
        {
            var parameters = new DialogParameters
            {
                { nameof(DeleteDHCPv6InterfaceDialog.Entry), entry }
            };

            var messageForm = _dialogService.Show<DeleteDHCPv6InterfaceDialog>(String.Format(L["DeleteDialogTitle"], entry.Name), parameters);
            var result = await messageForm.Result;

            if (result.IsSuccess() == true)
            {
                _snackBarService.Add(String.Format(L["DeleteInterfaceSuccessSnackbarContent"], entry.Name), Severity.Success);
                _interfaceOverview = await _service.GetDHCPv6Interfaces();
            }
        }
    }
}


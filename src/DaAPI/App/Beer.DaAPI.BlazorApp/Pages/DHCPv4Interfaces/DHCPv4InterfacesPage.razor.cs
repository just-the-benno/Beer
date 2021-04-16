using Beer.DaAPI.BlazorApp.Helper;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv4InterfaceResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Interfaces
{
    public partial class DHCPv4InterfacesPage
    {
        private DHCPv4InterfaceOverview _interfaceOverview;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _interfaceOverview = await _service.GetDHCPv4Interfaces();
        }

        public async Task ShowAddInterfaceDialog(DHCPv4InterfaceEntry entry)
        {
            var parameters = new DialogParameters
            {
                { nameof(CreateDHCPv4InterfaceDialog.Entry), entry }
            };

            var messageForm = _dialogService.Show<CreateDHCPv4InterfaceDialog>(String.Format(L["CreateInterfaceDialogHeader"], entry.InterfaceName), parameters);
            var result = await messageForm.Result;

            if (result.IsSuccess() == true)
            {
                _snackBarService.Add(String.Format(L["CreateInterfaceSuccessSnackbarContent"], entry.IPv4Address), Severity.Success);
                _interfaceOverview = await _service.GetDHCPv4Interfaces();
            }
        }

        public async Task ShowDeleteDialog(ActiveDHCPv4InterfaceEntry entry)
        {
            var parameters = new DialogParameters
            {
                { nameof(DeleteDHCPv4InterfaceDialog.Entry), entry }
            };

            var messageForm = _dialogService.Show<DeleteDHCPv4InterfaceDialog>(String.Format(L["DeleteDialogTitle"], entry.Name), parameters);
            var result = await messageForm.Result;

            if (result.IsSuccess() == true)
            {
                _snackBarService.Add(String.Format(L["DeleteInterfaceSuccessSnackbarContent"],entry.Name), Severity.Success);
                _interfaceOverview = await _service.GetDHCPv4Interfaces();
            }
        }
    }
}

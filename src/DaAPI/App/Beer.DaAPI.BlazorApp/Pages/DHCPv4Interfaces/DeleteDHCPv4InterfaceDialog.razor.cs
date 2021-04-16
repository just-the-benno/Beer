using Beer.DaAPI.BlazorApp.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv4InterfaceResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Interfaces
{
    public partial class DeleteDHCPv4InterfaceDialog : DaAPIDialogBase
    {
        private Boolean _sendingInProgress = false;
        private Boolean _hasErrors = false;

        [Parameter] public ActiveDHCPv4InterfaceEntry Entry { get; set; }

        private async Task DeleteListener()
        {
            _sendingInProgress = true;
            _hasErrors = false;

            Boolean result = await _service.SendDeleteDHCPv4InterfaceRequest(Entry.SystemId);
            _sendingInProgress = false;

            if (result == true)
            {
                MudDialog.Close(DialogResult.Ok<Boolean>(result));
            }
            else
            {
                _hasErrors = true;
            }
        }
    }
}

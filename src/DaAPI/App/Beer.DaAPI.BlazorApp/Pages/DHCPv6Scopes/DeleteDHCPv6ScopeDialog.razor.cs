using Beer.DaAPI.BlazorApp.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public partial class DeleteDHCPv6ScopeDialog : DaAPIDialogBase
    {
        private Boolean _sendingInProgress = false;
        private Boolean _hasErrors = false;

        [Parameter] public DHCPv6ScopeTreeViewItem Entry { get; set; }
        [Parameter] public Boolean IncludeChildren { get; set; }

        private async Task DeleteListener()
        {
            _sendingInProgress = true;
            _hasErrors = false;

            Boolean result = await _service.SendDeleteDHCPv6ScopeRequest(new DHCPv6ScopeDeleteRequest
            {
                Id = Entry.Id,
                IncludeChildren = Entry.ChildScopes.Any() != false && IncludeChildren
            });
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

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
    public partial class ChangeParentDHCPv6ScopeDialog : DaAPIDialogBase
    {
        private Boolean _sendingInProgress = false;
        private Boolean _hasErrors = false;

        [Parameter] public DHCPv6ScopeTreeViewItem Entry { get; set; }
        [Parameter] public IEnumerable<DHCPv6ScopeTreeViewItem> Items { get; set; }

        [Parameter] public Guid? ParentId { get; set; }

        private void GoThroughScopeTree(DHCPv6ScopeTreeViewItem item, Int32 depth, ICollection<(DHCPv6ScopeTreeViewItem item, Int32 depth)>  result)
        {
            if(item == Entry) { return; }

            result.Add((item, depth));

            foreach (var child in item.ChildScopes)
            {
                GoThroughScopeTree(child, depth + 1, result);
            }
        }

        private IEnumerable<(DHCPv6ScopeTreeViewItem item, Int32 depth)> GetPossibleParents()
        {
            List<(DHCPv6ScopeTreeViewItem item, Int32 depth)> result = new List<(DHCPv6ScopeTreeViewItem item, int depth)>();

            foreach (var item in Items)
            {
                GoThroughScopeTree(item, 0,result);
            }

            return result;
        }

        private async Task ChangeParent()
        {
            _sendingInProgress = true;
            _hasErrors = false;

            Boolean result = await _service.SendChangeDHCPv6ScopeParentRequest(Entry.Id, ParentId);
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


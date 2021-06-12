using Beer.DaAPI.BlazorApp.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public partial class ChangeParentDHCPv4ScopeDialog : DaAPIDialogBase
    {
        private Boolean _sendingInProgress = false;
        private Boolean _hasErrors = false;

        [Parameter] public DHCPv4ScopeTreeViewItem Entry { get; set; }
        [Parameter] public IEnumerable<DHCPv4ScopeTreeViewItem> Items { get; set; }

        [Parameter] public Guid? ParentId { get; set; }

        private void GoThroughScopeTree(DHCPv4ScopeTreeViewItem item, Int32 depth, ICollection<(DHCPv4ScopeTreeViewItem item, Int32 depth)>  result)
        {
            if(item == Entry) { return; }

            result.Add((item, depth));

            foreach (var child in item.ChildScopes)
            {
                GoThroughScopeTree(child, depth + 1, result);
            }
        }

        private IEnumerable<(DHCPv4ScopeTreeViewItem item, Int32 depth)> GetPossibleParents()
        {
            List<(DHCPv4ScopeTreeViewItem item, Int32 depth)> result = new List<(DHCPv4ScopeTreeViewItem item, int depth)>();

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

            Boolean result = await _service.SendChangeDHCPv4ScopeParentRequest(Entry.Id, ParentId);
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


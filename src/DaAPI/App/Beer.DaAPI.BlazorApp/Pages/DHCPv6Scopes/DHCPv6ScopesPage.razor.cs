using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.BlazorApp.ModelHelper;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public partial class DHCPv6ScopesPage
    {
        private IEnumerable<DHCPv6ScopeTreeViewItem> _scopes;
        private Dictionary<DHCPv6ScopeTreeViewItem, DHCPv6ScopeTreeViewItem> _parentMapper;

        private readonly HashSet<TreeItemData<DHCPv6ScopeTreeViewItem>> _items = new();

        private void GenerateTree(DHCPv6ScopeTreeViewItem item, HashSet<TreeItemData<DHCPv6ScopeTreeViewItem>> parent)
        {
            TreeItemData<DHCPv6ScopeTreeViewItem> treeItem = new() { Value = item, IsExpanded = true, Name = item.Name, Children = new() };

            parent.Add(treeItem);
            if (item.ChildScopes.Any() == true)
            {
                foreach (var child in item.ChildScopes)
                {
                    _parentMapper.Add(child, item);

                    GenerateTree(child, treeItem.Children);
                }
            }
        }

        private async Task LoadItems()
        {
            _scopes = await service.GetDHCPv6ScopesAsTree();
            _parentMapper = _scopes.ToDictionary(x => x, null);

            foreach (var item in _scopes)
            {
                GenerateTree(item, _items);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadItems();
        }

        private void NavigateToScopeDetails(DHCPv6ScopeTreeViewItem item) => _navManager.NavigateTo($"/scopes/dhcpv6/details/{item.Id}");
        private void NavigateToCreateNewChildScope(DHCPv6ScopeTreeViewItem item) => _navManager.NavigateTo($"/scopes/dhcpv6/create/childOf/{item.Id}");
        private void NavigateToEditScope(DHCPv6ScopeTreeViewItem item) => _navManager.NavigateTo($"/scopes/dhcpv6/update/{item.Id}");
        private void NavigateToCreateCopyFromScope(DHCPv6ScopeTreeViewItem item) => _navManager.NavigateTo($"/scopes/dhcpv6/create/copyFrom/{item.Id}");

        public async Task ShowDeleteDialog(DHCPv6ScopeTreeViewItem item)
        {
            var parameters = new DialogParameters
            {
                { nameof(DeleteDHCPv6ScopeDialog.Entry), item }
            };

            var messageForm = _dialogService.Show<DeleteDHCPv6ScopeDialog>(String.Format(L["DeleteDialogTitle"], item.Name), parameters);
            var result = await messageForm.Result;

            if (result.IsSuccess() == true)
            {
                _snackBarService.Add(String.Format(L["DeleteScopesSuccessSnackbarContent"], item.Name), Severity.Success);
                _items.Clear();
                StateHasChanged();
                await LoadItems();
            }
        }

        private String GetScopeAddressRangeAsString(DHCPv6ScopeTreeViewItem item)
        {
            if (item.StartAddress == item.EndAddress)
            {
                return item.StartAddress;
            }
            else
            {
                return $"{item.StartAddress} - {item.EndAddress}";
            }
        }

        //private String GetExcludedAddressAsString(DHCPv6ScopeTreeViewItem item)
        //{
        //    StringBuilder builder = new(300);

        //    foreach (var excludedAddress in item.ExcludedAddresses)
        //    {
        //        builder.Append($"{excludedAddress}, ");
        //    }

        //    return builder.ToString(0, builder.Length - 2);
        //}
    }
}

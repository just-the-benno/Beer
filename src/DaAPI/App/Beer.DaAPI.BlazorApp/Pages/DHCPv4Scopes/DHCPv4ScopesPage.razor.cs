using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.BlazorApp.ModelHelper;
using Beer.DaAPI.Core.Common;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public partial class DHCPv4ScopesPage
    {
        private IEnumerable<DHCPv4ScopeTreeViewItem> _scopes;
        private Dictionary<DHCPv4ScopeTreeViewItem, DHCPv4ScopeTreeViewItem> _parentMapper;

        private readonly HashSet<TreeItemData<DHCPv4ScopeTreeViewItem>> _items = new();

        private void GenerateTree(DHCPv4ScopeTreeViewItem item, HashSet<TreeItemData<DHCPv4ScopeTreeViewItem>> parent)
        {
            TreeItemData<DHCPv4ScopeTreeViewItem> treeItem = new() { Value = item, IsExpanded = true, Name = item.Name, Children = new() };

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
            _scopes = await service.GetDHCPv4ScopesAsTree();
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

        private void NavigateToScopeDetails(DHCPv4ScopeTreeViewItem item) => _navManager.NavigateTo($"/scopes/dhcpv4/details/{item.Id}");
        private void NavigateToCreateNewChildScope(DHCPv4ScopeTreeViewItem item) => _navManager.NavigateTo($"/scopes/dhcpv4/create/childOf/{item.Id}");
        private void NavigateToEditScope(DHCPv4ScopeTreeViewItem item) => _navManager.NavigateTo($"/scopes/dhcpv4/update/{item.Id}");
        private void NavigateToCreateCopyFromScope(DHCPv4ScopeTreeViewItem item) => _navManager.NavigateTo($"/scopes/dhcpv4/create/copyFrom/{item.Id}");

        public async Task ShowDeleteDialog(DHCPv4ScopeTreeViewItem item)
        {
            var parameters = new DialogParameters
            {
                { nameof(DeleteDHCPv4ScopeDialog.Entry), item }
            };

            var messageForm = _dialogService.Show<DeleteDHCPv4ScopeDialog>(String.Format(L["DeleteDialogTitle"], item.Name), parameters);
            var result = await messageForm.Result;

            if (result.IsSuccess() == true)
            {
                _snackBarService.Add(String.Format(L["DeleteScopesSuccessSnackbarContent"], item.Name), Severity.Success);
                _items.Clear();
                StateHasChanged();
                await LoadItems();
            }
        }

        public async Task ShowChangeParentDialog(DHCPv4ScopeTreeViewItem item)
        {
            var parameters = new DialogParameters
            {
                { nameof(ChangeParentDHCPv4ScopeDialog.Entry), item },
                { nameof(ChangeParentDHCPv4ScopeDialog.Items), _scopes }
            };

            var messageForm = _dialogService.Show<ChangeParentDHCPv4ScopeDialog>(String.Format(L["ChangeParentScopeTitle"], item.Name), parameters);
            var result = await messageForm.Result;

            if (result.IsSuccess() == true)
            {
                _snackBarService.Add(String.Format(L["ChangeParentScopesSuccessSnackbarContent"], item.Name), Severity.Success);
                _items.Clear();
                StateHasChanged();
                await LoadItems();
            }
        }

        public async Task OpenExportDialog()
        {
            var messageForm = _dialogService.Show<ExportScopeStructureDialog>(L["ExportScopesTitle"],new DialogOptions
            {
                FullWidth = true,
            });
            await messageForm.Result;
        }

        public async Task OpenImportDialog()
        {
            var messageForm = _dialogService.Show<ImportScopeStructureDialog>(L["ImportScopesTitle"], new DialogOptions
            {
                FullWidth = true,
                DisableBackdropClick = true,
                CloseButton = false,
            });

            var result = await messageForm.Result;

            if (result.IsSuccess() == true)
            {
                _snackBarService.Add(L["ImportSuccessSnackbarContent"], Severity.Success);
                _items.Clear();
                StateHasChanged();
                await LoadItems();
            }
        }

        private String GetScopeAddressRangeAsString(DHCPv4ScopeTreeViewItem item)
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

        private String GetExcludedAddressAsString(DHCPv4ScopeTreeViewItem item)
        {
            StringBuilder builder = new(300);

            foreach (var excludedAddress in item.ExcludedAddresses)
            {
                builder.Append($"{excludedAddress}, ");
            }

            return builder.ToString(0, builder.Length - 2);
        }
    }
}

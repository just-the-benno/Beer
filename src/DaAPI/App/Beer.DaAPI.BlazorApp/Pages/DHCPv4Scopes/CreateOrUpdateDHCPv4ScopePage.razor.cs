using Beer.DaAPI.BlazorApp.Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1.DHCPv4ScopeAddressPropertyReqest;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public partial class CreateOrUpdateDHCPv4ScopePage : IDisposable
    {
        private MudTabs _tabs;

        private CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModel _generellPropertiesModel;
        private CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel _addressRelatedPropertiesModel;
        private CreateOrUpdateDHCPv4ScopePropertiesViewModel _optionalValues;
        private CreateOrUpdateDHCPv4ScopeResolverRelatedViewModel _resolverModel;

        private EditContext _generellPropertiesContext;
        private EditContext _addressRelatedPropertiesContext;
        private EditContext _optionalValuesContext;
        private EditContext _resolverContext;

        private Boolean _submitInProgress;
        private Boolean _isCreateMode = true;

        private List<(Int32 Depth, DHCPv4ScopeTreeViewItem Scope)> _parents;

        private Object _loadingParentInProgress = null;
        private Boolean _shouldRevalidate = false;
        private Guid _excludedAddressKey = Guid.NewGuid();
        private Guid _optionalValuesKey = Guid.NewGuid();
        private Guid _resolverValuesKey = Guid.NewGuid();

        private DHCPv4ScopePropertiesResponse _parent = new();
        private DHCPv4ScopePropertiesResponse _self = new();

        private IEnumerable<DHCPv4ScopeResolverDescription> _possibleResolvers;

        [Parameter] public String ScopeId { get; set; }

        #region EditContextChanges

        private void ValidateContext(EditContext context)
        {
            context.Validate();
            StateHasChanged();
        }

        private void AddressRelatedPropertiesContextChanged(object sender, FieldChangedEventArgs e) => ValidateContext(_addressRelatedPropertiesContext);
        private void OptionalValuesContextChanged(object sender, FieldChangedEventArgs e) => ValidateContext(_optionalValuesContext);

        private async void GenerellPropertiesContextChanged(object sender, FieldChangedEventArgs e)
        {
            if (e.FieldIdentifier.FieldName == nameof(CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModel.HasParent))
            {
                if (_generellPropertiesModel.HasParent == false)
                {
                    _loadingParentInProgress = new object();

                    _parents = null;
                    _generellPropertiesModel.ParentId = null;
                    _generellPropertiesContext.Validate();
                    StateHasChanged();
                }
                else
                {
                    await LoadPossibleParents();

                    _shouldRevalidate = true;

                    StateHasChanged();
                }
            }
            else if (e.FieldIdentifier.FieldName == nameof(CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModel.ParentId))
            {
                await LoadParent(true);

            }
        }

        private async Task LoadPossibleParents()
        {
            _loadingParentInProgress = null;

            var scopes = await _service.GetDHCPv4ScopesAsTree();
            _parents = new();
            foreach (var item in scopes)
            {
                GenereteScopeTreeItems(0, item);
            }
        }

        private async Task LoadDevicesIfNeeded()
        {
            if (_resolverModel.HasDeviceProperty() == true)
            {
                _resolverModel.LoadDevicesInProgress = true;
                StateHasChanged();
                var devices = await _service.GetDeviceOverview();
                _resolverModel.SetDevices(devices);
                _resolverModel.LoadDevicesInProgress = false;
                StateHasChanged();
            }
        }

        private async void OnResolverContextChanged(object sender, FieldChangedEventArgs e)
        {
            if (e.FieldIdentifier.FieldName == nameof(CreateOrUpdateDHCPv4ScopeResolverRelatedViewModel.ResolverType))
            {
                _resolverModel.SetPropertiesToDefault(_possibleResolvers.FirstOrDefault(x => x.TypeName == _resolverModel.ResolverType));
                _resolverValuesKey = Guid.NewGuid();

                await LoadDevicesIfNeeded();


            }

            ValidateContext(_resolverContext);
        }

        #endregion

        #region Helper

        private Boolean HasParent() => _generellPropertiesModel?.HasParent ?? false;

        private String GetLocalizedAddressAllocationName(AddressAllocationStrategies? strategy) => strategy switch
        {
            AddressAllocationStrategies.Next => L["AddressAllocationStrategies_Next_Label"],
            AddressAllocationStrategies.Random => L["AddressAllocationStrategies_Random_Label"],
            _ => String.Empty,
        };

        private IEnumerable<(String caption, AddressAllocationStrategies value)> GetAddressAllocations() => new (String caption, AddressAllocationStrategies value)[]
        {
            (L["AddressAllocationStrategies_Next_Label"],AddressAllocationStrategies.Next),
            (L["AddressAllocationStrategies_Random_Label"],AddressAllocationStrategies.Random),
        };

        private void AddExcludedAddress()
        {
            _addressRelatedPropertiesModel.AddDefaultExcludedAddress();
            _shouldRevalidate = true;
        }

        private void RemoveExcludedAddress(Int32 index)
        {
            _addressRelatedPropertiesModel.RemoveExcludedAddress(index);
            _excludedAddressKey = Guid.NewGuid();
            _shouldRevalidate = true;
        }

        private void AddOptionalValue()
        {
            Byte optionCode = 0;
            foreach (var item in CreateOrUpdateDHCPv4ScopePropertyModel.WellknowOptions)
            {
                if (_optionalValues.Properties.Any(x => x.OptionCode == item.Key) == true)
                {
                    continue;
                }

                optionCode = item.Key;
                break;
            }

            _optionalValues.Properties.Add(new CreateOrUpdateDHCPv4ScopePropertyModel(optionCode));
            _shouldRevalidate = true;
        }

        private void RemoveOptionalValue(Int32 index)
        {
            _optionalValues.Properties.RemoveAt(index);
            _optionalValuesKey = Guid.NewGuid();
            _shouldRevalidate = true;
        }

        private void AddAddressToOption(CreateOrUpdateDHCPv4ScopePropertyModel model)
        {
            model.AddAddressValue();
            _shouldRevalidate = true;
        }

        private void RemoveAddressFromOption(CreateOrUpdateDHCPv4ScopePropertyModel model, Int32 index)
        {
            if (model.RemoveAddressAt(index) == false) { return; }

            _shouldRevalidate = true;
        }

        private void AddAddressToResolverProperty(CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel model)
        {
            model.AddEmptyValue();
            _shouldRevalidate = true;
        }

        private void RemoveAddressFromResolverProperty(CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel model, Int32 index)
        {
            if (model.RemoveValue(index) == false) { return; }

            _shouldRevalidate = true;
        }

        private void GenereteScopeTreeItems(Int32 currentDepth, DHCPv4ScopeTreeViewItem treeItem)
        {
            _parents.Add((currentDepth, treeItem));
            if (treeItem.ChildScopes.Any() == true)
            {
                foreach (var item in treeItem.ChildScopes)
                {
                    GenereteScopeTreeItems(currentDepth + 1, item);
                }
            }
        }

        private void NavigateToStep(Int32 step) => _tabs.ActivatePanel(step - 1);

        private async Task LoadParent(Boolean manuelRefresh)
        {
            if (_generellPropertiesModel.ParentId.HasValue == false)
            {
                _parent = new();
                _loadingParentInProgress = false;
                if (manuelRefresh == true)
                {
                    StateHasChanged();
                }
                return;
            }

            _parent = new();
            _addressRelatedPropertiesModel.RemoveParent();
            _loadingParentInProgress = null;
            if (manuelRefresh == true)
            {
                StateHasChanged();
            }

            _parent = await _service.GetDHCPv4ScopeProperties(_generellPropertiesModel.ParentId.Value, true);
            _addressRelatedPropertiesModel.SetParent(_parent);
            _optionalValues.LoadFromParent(_parent, _self);
            _loadingParentInProgress = true;

            if (manuelRefresh == true)
            {
                StateHasChanged();
            }
        }

        private async Task SubmttingForm()
        {
            var model = new CreateOrUpdateDHCPv4ScopeViewModel
            {
                GenerellProperties = _generellPropertiesModel,
                AddressRelatedProperties = _addressRelatedPropertiesModel,
                ScopeProperties = _optionalValues,
                ResolverProperties = _resolverModel,
            };

            _submitInProgress = true;

            try
            {
                var result = _isCreateMode == true ? (await _service.CreateDHCPv4Scope(model.GetRequest()) != default) : await _service.UpdateDHCPv4Scope(model.GetRequest(), ScopeId);
                if (result == true)
                {
                    _snackBarService.Add(String.Format(_isCreateMode == true ? L["CreateScopeSuccessSnackbarContent"] : L["UpdateScopeSuccessSnackbarContent"], _generellPropertiesModel.Name), Severity.Success);
                    _navManager.NavigateTo("scopes/dhcpv4/");
                }
                else
                {
                    _snackBarService.Add(_isCreateMode == true ? L["CreateScopeFailedSnackbarContent"] : L["UpdateScopeFailedSnackbarContent"], Severity.Error);
                }
            }
            finally
            {
                _submitInProgress = false;
            }
        }

        #endregion

        #region LifeCycles

        private void CreateContexts()
        {
            _generellPropertiesContext = new EditContext(_generellPropertiesModel);
            _addressRelatedPropertiesContext = new EditContext(_addressRelatedPropertiesModel);
            _optionalValuesContext = new EditContext(_optionalValues);
            _resolverContext = new EditContext(_resolverModel);

            _generellPropertiesContext.OnFieldChanged += GenerellPropertiesContextChanged;
            _addressRelatedPropertiesContext.OnFieldChanged += AddressRelatedPropertiesContextChanged;
            _optionalValuesContext.OnFieldChanged += OptionalValuesContextChanged;
            _resolverContext.OnFieldChanged += OnResolverContextChanged;
        }

        private Boolean _loadingOfInitialDataNeededCompled = false;

        protected override void OnInitialized()
        {
            _loadingParentInProgress = new object();

            _generellPropertiesModel = new();
            _addressRelatedPropertiesModel = new();
            _optionalValues = new();
            _resolverModel = new();

            String uri = _navManager.Uri;

            if (uri.Contains("scopes/dhcpv4/create/copyFrom/") == true ||
                uri.Contains("/scopes/dhcpv4/create/childOf/") == true ||
                uri.Contains("/scopes/dhcpv4/update/") == true)
            {

            }
            else
            {
                _loadingOfInitialDataNeededCompled = true;

                _addressRelatedPropertiesModel.SetDefaults();

                CreateContexts();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _possibleResolvers = await _service.GetDHCPv4ScopeResolverDescription();

            String uri = _navManager.Uri;
            if (uri.Contains("scopes/dhcpv4/create/copyFrom/") == true)
            {
                _self = await _service.GetDHCPv4ScopeProperties(ScopeId, false);
                await SetValuesFromProperties(_self);
                _generellPropertiesModel.Name = $"{L["ScopeCopyPrefix"]} {_generellPropertiesModel.Name}";

                CreateContexts();
            }
            else if (uri.Contains("/scopes/dhcpv4/update/") == true)
            {
                _isCreateMode = false;
                _generellPropertiesModel.Id = Guid.Parse(ScopeId);
                _self = await _service.GetDHCPv4ScopeProperties(ScopeId, false);
                await SetValuesFromProperties(_self);

                CreateContexts();
            }
            else if (uri.Contains("/scopes/dhcpv4/create/childOf/") == true)
            {
                _generellPropertiesModel.HasParent = true;
                await LoadPossibleParents();

                _generellPropertiesModel.ParentId = Guid.Parse(ScopeId);
                await LoadParent(false);

                CreateContexts();
            }

            _loadingOfInitialDataNeededCompled = true;
        }

        private async Task SetValuesFromProperties(DHCPv4ScopePropertiesResponse properties)
        {
            _generellPropertiesModel.Name = properties.Name;
            _generellPropertiesModel.Description = properties.Description;
            _generellPropertiesModel.HasParent = properties.ParentId.HasValue;
            if (_generellPropertiesModel.HasParent == true)
            {
                await LoadPossibleParents();

                _generellPropertiesModel.ParentId = properties.ParentId.Value;
                await LoadParent(false);
            }
            else
            {
                _addressRelatedPropertiesModel.SetDefaults();
            }

            _addressRelatedPropertiesModel.SetByResponse(properties);
            _optionalValues.LoadFromResponse(properties);

            _resolverModel.LoadFromResponse(properties, _possibleResolvers.FirstOrDefault(x => x.TypeName == properties.Resolver.Typename));

            await LoadDevicesIfNeeded();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (_shouldRevalidate == true)
            {
                _shouldRevalidate = false;
                _generellPropertiesContext.Validate();
                _addressRelatedPropertiesContext.Validate();
                _optionalValuesContext.Validate();
                _resolverContext.Validate();

                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }

        public void Dispose()
        {
            _generellPropertiesContext.OnFieldChanged -= GenerellPropertiesContextChanged;
            _addressRelatedPropertiesContext.OnFieldChanged -= AddressRelatedPropertiesContextChanged;
            _optionalValuesContext.OnFieldChanged -= OptionalValuesContextChanged;
            _resolverContext.OnFieldChanged -= OnResolverContextChanged;
        }

        #endregion
    }
}

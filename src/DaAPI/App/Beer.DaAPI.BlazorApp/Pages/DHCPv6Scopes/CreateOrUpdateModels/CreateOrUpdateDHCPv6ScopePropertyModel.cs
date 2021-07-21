using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.BlazorApp.ModelHelper;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.DaAPI.Shared.Responses;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1.DHCPv6ScopeAddressPropertyReqest;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public class IPv6AddressForScopePropertyViewModel : ValueObjectWithParent<String, CreateOrUpdateDHCPv6ScopePropertyModel>
    {
        public IPv6AddressForScopePropertyViewModel(String value, CreateOrUpdateDHCPv6ScopePropertyModel parent) : base(
              value ?? String.Empty, parent)
        {
        }
    }

    public class CreateOrUpdateDHCPv6ScopePropertyModel
    {
        public static IEnumerable<DHCPv6ScopePropertyType> PropertyTypes { get; } = new[] {
            DHCPv6ScopePropertyType.AddressList,
            DHCPv6ScopePropertyType.Byte,
            DHCPv6ScopePropertyType.UInt16,
            DHCPv6ScopePropertyType.UInt32,
            DHCPv6ScopePropertyType.Text,
        };

        public static Dictionary<UInt16, (String DisplayName, DHCPv6ScopePropertyType Type)> WellknowOptions
        { get; private set; } =
    new Dictionary<UInt16, (string DisplayName, DHCPv6ScopePropertyType Type)>
{
            { 23,  ("DNS-Server", DHCPv6ScopePropertyType.AddressList) },
            { 31,  ("SNTP-Server", DHCPv6ScopePropertyType.AddressList) },
            { 56,  ("NTP-Server", DHCPv6ScopePropertyType.AddressList) },
            { 7,   ("Preference", DHCPv6ScopePropertyType.Byte) },
        };

        internal Boolean RemoveAddressAt(int index)
        {
            if (AddressesValues.Count < 1) { return false; }

            AddressesValues.RemoveAt(index);
            Key = Guid.NewGuid();

            return true;
        }

        public Guid Key { get; private set; } = Guid.NewGuid();

        internal void AddAddressValue()
        {
            AddressesValues.Add(new IPv6AddressForScopePropertyViewModel("::1", this));
        }

        private readonly DHCPv6ScopePropertyResponse _parent;

        private DHCPv6ScopePropertyType _propertyType;

        public DHCPv6ScopePropertyType PropertyType
        {
            get => _propertyType;
            set
            {
                if (value != _propertyType)
                {
                    ResetValues();
                    SetDefaultValue(value);
                }

                _propertyType = value;
            }
        }

        private void SetDefaultValue(DHCPv6ScopePropertyType value)
        {
            switch (value)
            {
                case DHCPv6ScopePropertyType.AddressList:
                    AddressesValues.Add(new IPv6AddressForScopePropertyViewModel("::1", this));
                    break;
                case DHCPv6ScopePropertyType.Byte:
                case DHCPv6ScopePropertyType.UInt16:
                case DHCPv6ScopePropertyType.UInt32:
                    if (IsNumericType() == false)
                    {
                        NumericValue = 0;
                    }
                    break;
                case DHCPv6ScopePropertyType.Text:
                    TextValue = String.Empty;
                    break;
                default:
                    break;
            }
        }

        private void ResetValues()
        {
            TextValue = String.Empty;
            NumericValue = null;
            AddressesValues.Clear();
        }

        private Boolean _MarkAsRemovedInInheritance;

        public Boolean MarkAsRemovedInInheritance
        {
            get => _MarkAsRemovedInInheritance;
            set
            {
                _MarkAsRemovedInInheritance = value;
                if (value == true)
                {
                    OverrideParentValue = false;
                }
            }
        }

        private UInt16 _optionCode;

        public UInt16 OptionCode
        {
            get => _optionCode;
            set
            {
                if (value != _optionCode)
                {
                    _optionCode = value;
                    if (WellknowOptions.ContainsKey(value) == true)
                    {
                        PropertyType = WellknowOptions[value].Type;
                    }
                    else
                    {
                        PropertyType = DHCPv6ScopePropertyType.Text;
                    }
                }
            }
        }

        public Boolean IsWellKnown => WellknowOptions.ContainsKey(OptionCode);
        public Boolean IsSetByParent { get; private set; }

        private Boolean _overrideParentValue = false;

        public String GetWellKnownOptioName() => WellknowOptions[OptionCode].DisplayName;

        public Boolean OverrideParentValue
        {
            get => _overrideParentValue;
            set
            {
                if (value != _overrideParentValue)
                {
                    if (value == false)
                    {
                        SetValueFromResponse(_parent);
                    }
                    else
                    {
                        MarkAsRemovedInInheritance = false;
                    }

                    _overrideParentValue = value;
                }

            }
        }

        public String TextValue { get; set; }
        public Int64? NumericValue { get; set; }
        public IList<IPv6AddressForScopePropertyViewModel> AddressesValues { get; private set; } = new List<IPv6AddressForScopePropertyViewModel>();

        public CreateOrUpdateDHCPv6ScopePropertyModel(UInt16 optionCode)
        {
            OptionCode = optionCode;
        }

        public CreateOrUpdateDHCPv6ScopePropertyModel(DHCPv6ScopePropertyResponse response, Boolean markAsRemovedInInheritance, Boolean setAsParent)
        {
            if (setAsParent == true)
            {
                IsSetByParent = true;
                _parent = response;
            }

            OptionCode = response.OptionCode;
            PropertyType = response.Type;
            MarkAsRemovedInInheritance = markAsRemovedInInheritance;

            SetValueFromResponse(response);
        }

        private void SetValueFromResponse(DHCPv6ScopePropertyResponse response)
        {
            switch (response)
            {
                case DHCPv6AddressListScopePropertyResponse property:
                    AddressesValues.Clear();
                    foreach (var item in property.Addresses)
                    {
                        AddAddress(item);
                    }
                    break;
                case DHCPv6NumericScopePropertyResponse property:
                    NumericValue = property.Value;
                    break;
                case DHCPv6TextScopePropertyResponse property:
                    TextValue = property.Value;
                    break;
                default:
                    break;
            }
        }

        public void AddAddress() => AddAddress(String.Empty);
        public void AddAddress(String content) => AddressesValues.Add(new IPv6AddressForScopePropertyViewModel(content, this));
        public void RemoveAddress(Int32 index) => AddressesValues.RemoveAt(index);

        internal bool IsNumericType() => PropertyType ==
            DHCPv6ScopePropertyType.Byte ||
            PropertyType == DHCPv6ScopePropertyType.UInt16 ||
            PropertyType == DHCPv6ScopePropertyType.UInt32;

        internal DHCPv6ScopePropertyRequest ToRequest()
        {
            DHCPv6ScopePropertyRequest request = PropertyType switch
            {
                DHCPv6ScopePropertyType.AddressList => new DHCPv6AddressListScopePropertyRequest
                {
                    Addresses = AddressesValues.Select(x => x.Value).ToList(),
                },

                DHCPv6ScopePropertyType.Byte => new DHCPv6NumericScopePropertyRequest
                {
                    NumericType = NumericScopePropertiesValueTypes.Byte,
                    Value = NumericValue.Value,
                },
                DHCPv6ScopePropertyType.UInt16 => new DHCPv6NumericScopePropertyRequest
                {
                    NumericType = NumericScopePropertiesValueTypes.UInt16,
                    Value = NumericValue.Value,
                },
                DHCPv6ScopePropertyType.UInt32 => new DHCPv6NumericScopePropertyRequest
                {
                    NumericType = NumericScopePropertiesValueTypes.UInt32,
                    Value = NumericValue.Value,
                },
                DHCPv6ScopePropertyType.Text => new DHCPv6TextScopePropertyRequest
                {
                    Value = TextValue,
                },
                _ => throw new NotImplementedException(),
            };

            request.MarkAsRemovedInInheritance = MarkAsRemovedInInheritance;
            request.OptionCode = OptionCode;
            request.Type = PropertyType;

            return request;
        }

        internal void UpdateFromResponse(DHCPv6ScopePropertyResponse item, Boolean markAsRemovedInInheritance)
        {
            OverrideParentValue = true;
            ResetValues();
            SetValueFromResponse(item);
            MarkAsRemovedInInheritance = markAsRemovedInInheritance;
        }
    }

    public class CreateOrUpdateDHCPv6ScopePropertiesViewModel
    {
        public IList<CreateOrUpdateDHCPv6ScopePropertyModel> Properties { get; private set; } = new List<CreateOrUpdateDHCPv6ScopePropertyModel>();

        internal void LoadFromParent(DHCPv6ScopePropertiesResponse parent, DHCPv6ScopePropertiesResponse withoutParents)
        {
            Properties.Clear();

            foreach (var item in parent.Properties)
            {
                Properties.Add(new CreateOrUpdateDHCPv6ScopePropertyModel(item, (parent?.InheritanceStopedProperties ?? Array.Empty<Int32>()).Contains(item.OptionCode) || (withoutParents?.InheritanceStopedProperties ?? Array.Empty<Int32>()).Contains(item.OptionCode), true));
            }
        }

        public Boolean PropertiesHasParents() => Properties.Any(x => x.IsSetByParent == true);

        internal IEnumerable<DHCPv6ScopePropertyRequest> GetRequest() => Properties.Where(x => x.IsSetByParent == false || x.OverrideParentValue == true || x.MarkAsRemovedInInheritance == true)
            .Select(x => x.ToRequest());

        internal void LoadFromResponse(DHCPv6ScopePropertiesResponse properties)
        {
            foreach (var item in properties.Properties)
            {
                var existing = Properties.FirstOrDefault(x => x.OptionCode == item.OptionCode);
                if (existing == null)
                {
                    Properties.Add(new CreateOrUpdateDHCPv6ScopePropertyModel(item, properties.InheritanceStopedProperties.Contains(item.OptionCode), false));
                }
                else
                {
                    existing.UpdateFromResponse(item, properties.InheritanceStopedProperties.Contains(item.OptionCode));
                }
            }
        }
    }

    public class CreateOrUpdateDHCPv6ScopePropertiesViewModellValidator : AbstractValidator<CreateOrUpdateDHCPv6ScopePropertiesViewModel>
    {
        public CreateOrUpdateDHCPv6ScopePropertiesViewModellValidator(IStringLocalizer<CreateOrUpdateDHCPv6ScopePage> localizer)
        {
            RuleForEach(x => x.Properties).ChildRules(element =>
            {
                element.RuleFor(x => x.NumericValue).NotNull().When(x => x.IsNumericType(), ApplyConditionTo.CurrentValidator)
                .LessThanOrEqualTo(Byte.MaxValue).When(x => x.PropertyType == DHCPv6ScopePropertyType.Byte, ApplyConditionTo.CurrentValidator)
                .LessThanOrEqualTo(UInt16.MaxValue).When(x => x.PropertyType == DHCPv6ScopePropertyType.UInt16, ApplyConditionTo.CurrentValidator)
                .LessThanOrEqualTo(UInt32.MaxValue).When(x => x.PropertyType == DHCPv6ScopePropertyType.UInt32, ApplyConditionTo.CurrentValidator)
                .WithName(localizer["OptionalPropertiesValueLabel"]);

                element.RuleFor(x => x.TextValue).NotNull().When(x => x.PropertyType == DHCPv6ScopePropertyType.Text);
                element.RuleFor(x => x.AddressesValues).Must(x => x.Count > 0).WithMessage(localizer["ValidationOptionalPropertiesAddressesArtEmpty"]).When(x => x.PropertyType == DHCPv6ScopePropertyType.AddressList); ;
                element.RuleForEach(x => x.AddressesValues).ChildRules(element2 =>
                {
                    element2.RuleFor(x => x.Value).Cascade(CascadeMode.Stop).InjectIPv6AdressValidator(localizer["SingleAddtionalOptionAddressLabel"]);
                    element2.RuleFor(x => x.Value)
                    .Must((properties, address) => properties.Parent.AddressesValues.Count(y => y.Value == address) == 1).WithMessage(localizer["ValidationOptionalAddressAlreadyExists"]);
                });
            });
        }
    }
}


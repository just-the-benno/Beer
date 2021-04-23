using Beer.DaAPI.Core.Common;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beer.DaAPI.BlazorApp.Helper;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;
using Beer.DaAPI.Shared.Requests;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public class IPv4AddressForScopeResolverPropertyViewModel : ValueObjectWithParent<String, CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel>
    {
        public IPv4AddressForScopeResolverPropertyViewModel(String value, CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel parent) : base(
              value ?? String.Empty, parent)
        {
        }
    }

    public class CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel
    {
        public Boolean IsListValue { get; }
        public Boolean IsTextValue { get; }
        public Boolean IsNumericValue { get; }
        public Boolean IsNullableNumericValue { get; }
        public Boolean IsBooleanValue { get; }

        public Guid Key { get; private set; } = Guid.NewGuid();

        public ScopeResolverPropertyValueTypes ValueType { get; }
        public String Name { get; }

        public CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel(
            String propertyName, ScopeResolverPropertyValueTypes valueType)
        {
            ValueType = valueType;
            Name = propertyName;

            switch (valueType)
            {
                case ScopeResolverPropertyValueTypes.IPv4Address:
                case ScopeResolverPropertyValueTypes.String:
                case ScopeResolverPropertyValueTypes.IPv6Address:
                case ScopeResolverPropertyValueTypes.IPv6NetworkAddress:
                case ScopeResolverPropertyValueTypes.IPv6Subnet:
                case ScopeResolverPropertyValueTypes.ByteArray:
                    IsTextValue = true;
                    break;
                case ScopeResolverPropertyValueTypes.Numeric:
                case ScopeResolverPropertyValueTypes.UInt32:
                case ScopeResolverPropertyValueTypes.Byte:
                case ScopeResolverPropertyValueTypes.VLANId:
                case ScopeResolverPropertyValueTypes.IPv4Subnetmask:
                    IsNumericValue = true;
                    break;
                case ScopeResolverPropertyValueTypes.NullableUInt32:
                    IsNullableNumericValue = true;
                    break;
                case ScopeResolverPropertyValueTypes.IPv4AddressList:
                    IsListValue = true;
                    Addresses = new List<IPv4AddressForScopeResolverPropertyViewModel>();
                    AddEmptyValue();
                    break;
                case ScopeResolverPropertyValueTypes.Boolean:
                    IsBooleanValue = true;
                    break;
                default:
                    break;
            }

        }

        public CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel(
             String propertyName, ScopeResolverPropertyValueTypes valueType,
            String rawValue) : this(propertyName, valueType)
        {
            if (IsTextValue == true)
            {
                SingleValue = rawValue;
            }
            else if (IsBooleanValue == true)
            {
                BooleanValue = rawValue == "true";
            }
            else if (IsNumericValue == true)
            {
                NumericValue = Convert.ToInt64(rawValue);
            }
            else if (IsNullableNumericValue == true)
            {
                if (String.IsNullOrEmpty(rawValue) == true || rawValue == "null")
                {
                    NullableNumericValue = null;
                }
                else
                {
                    NullableNumericValue = Convert.ToInt64(rawValue);
                }
            }
            else if (IsListValue == true)
            {
                Addresses = new List<IPv4AddressForScopeResolverPropertyViewModel>();
                foreach (var item in System.Text.Json.JsonSerializer.Deserialize<List<String>>(rawValue).Select(x => new IPv4AddressForScopeResolverPropertyViewModel(x, this)))
                {
                    Addresses.Add(item);
                }
            }
        }

        public String SingleValue { get; set; }
        public Int64? NullableNumericValue { get; set; }
        public Int64 NumericValue { get; set; }
        public Boolean BooleanValue { get; set; }
        public IList<IPv4AddressForScopeResolverPropertyViewModel> Addresses { get; private set; } = new List<IPv4AddressForScopeResolverPropertyViewModel>();

        public void AddEmptyValue()
        {
            Addresses.Add(new IPv4AddressForScopeResolverPropertyViewModel("0.0.0.0", this));
            Key = Guid.NewGuid();
        }

        public Boolean RemoveValue(Int32 index)
        {
            if (Addresses.Count == 1) { return false; }

            Addresses.RemoveAt(index);
            Key = Guid.NewGuid();

            return true;
        }

        public String GetSerializedValue()
        {
            if (IsListValue == true)
            {
                return System.Text.Json.JsonSerializer.Serialize(Addresses.Select(x => x.Value));
            }
            else
            {
                if (IsTextValue == true)
                {
                    return System.Text.Json.JsonSerializer.Serialize(SingleValue);
                }
                else if (IsBooleanValue == true)
                {
                    return BooleanValue == true ? "true" : "false";
                }
                else if (IsNumericValue)
                {
                    return NumericValue.ToString();
                }
                else if (IsNullableNumericValue == true)
                {
                    if (NullableNumericValue.HasValue == true)
                    {
                        return NullableNumericValue.Value.ToString();
                    }
                    else
                    {
                        return "null";
                    }
                }

                return String.Empty;
            }
        }
    }

    public class CreateOrUpdateDHCPv4ScopeResolverRelatedViewModel
    {
        private String _resolverType;

        public String ResolverType
        {
            get => _resolverType;
            set
            {
                if (value != _resolverType)
                {
                    Properties.Clear();

                    _resolverType = value;
                }
            }
        }

        public IList<CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel> Properties { get; set; } = new List<CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel>();

        public void SetPropertiesToDefault(DHCPv4ScopeResolverDescription description)
        {
            if (description == null) { return; }

            Properties.Clear();

            foreach (var item in description.Properties)
            {
                Properties.Add(new CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel(item.PropertyName, item.PropertyValueType));
            }
        }

        internal CreateScopeResolverRequest GetRequest() =>
            new()
            {
                Typename = ResolverType,
                PropertiesAndValues = Properties.ToDictionary(x => x.Name,
                        x => x.GetSerializedValue()),
            };

        internal void LoadFromResponse(DHCPv4ScopePropertiesResponse properties,DHCPv4ScopeResolverDescription description)
        {
            if (description == null) { return; }

            ResolverType = properties.Resolver.Typename;
            foreach (var item in properties.Resolver.PropertiesAndValues)
            {
                var propertyType = description.Properties.FirstOrDefault(x => x.PropertyName.ToLower() == item.Key.ToLower());
                if(propertyType == null) { continue; }

                Properties.Add(new CreateOrUpdateDHCPv4ScopeResolverPropertyViewModel(propertyType.PropertyName, propertyType.PropertyValueType, item.Value));
            }
        }
    }

    public class CreateOrUpdateDHCPv4ScopeResolverRelatedViewModelValidator : AbstractValidator<CreateOrUpdateDHCPv4ScopeResolverRelatedViewModel>
    {
        public CreateOrUpdateDHCPv4ScopeResolverRelatedViewModelValidator(IStringLocalizer<CreateOrUpdateDHCPv4ScopePage> localizer)
        {
            RuleFor(x => x.ResolverType).NotNull().NotEmpty();

            RuleForEach(x => x.Properties).ChildRules(element =>
            {
                element.RuleFor(x => x.BooleanValue).NotNull().When(x => x.ValueType == ScopeResolverPropertyValueTypes.Boolean);
                element.RuleFor(x => x.NumericValue).NotNull().When(x => x.IsNumericValue, ApplyConditionTo.CurrentValidator)
                .LessThanOrEqualTo(Byte.MaxValue).When(x => x.ValueType == ScopeResolverPropertyValueTypes.Byte, ApplyConditionTo.CurrentValidator)
                .LessThanOrEqualTo(UInt32.MaxValue).When(x => x.ValueType == ScopeResolverPropertyValueTypes.UInt32, ApplyConditionTo.CurrentValidator)
                .LessThanOrEqualTo(32).WithMessage(localizer["ValidationNotValidIPv4Subnetmask"]).When(x => x.ValueType == ScopeResolverPropertyValueTypes.IPv4Subnetmask, ApplyConditionTo.CurrentValidator)
                .WithName(localizer["OptionalPropertiesValueLabel"]);

                element.RuleFor(x => x.NullableNumericValue)
                .LessThanOrEqualTo(UInt32.MaxValue).When(x => x.ValueType == ScopeResolverPropertyValueTypes.NullableUInt32 && x.NullableNumericValue.HasValue == true, ApplyConditionTo.CurrentValidator);

                element.RuleFor(x => x.SingleValue).NotNull().NotEmpty().When(x => x.IsTextValue);

                element.RuleFor(x => x.SingleValue).Matches(@"^[0-9a-fA-F]+$").WithMessage(localizer["ValidationNotAValidByteSequnce"]).When(x => x.ValueType == ScopeResolverPropertyValueTypes.ByteArray);
                element.RuleFor(x => x.SingleValue).Must(x => x.AsIPv4Address() != IPv4Address.Empty).WithMessage(localizer["ValidationNotIPv4Address"]).When(x => x.ValueType == ScopeResolverPropertyValueTypes.IPv4Address);

                element.RuleFor(x => x.Addresses).Must(x => x.Count > 0).WithMessage(localizer["ValidationOptionalPropertiesAddressesArtEmpty"]).When(x => x.ValueType == ScopeResolverPropertyValueTypes.IPv4AddressList);

                element.RuleForEach(x => x.Addresses).ChildRules(element2 =>
                {
                    element2.RuleFor(x => x.Value).Cascade(CascadeMode.Stop).InjectIPv4AdressValidator(localizer["SingleAddtionalOptionAddressLabel"]);
                    element2.RuleFor(x => x.Value)
                    .Must((properties, address) => properties.Parent.Addresses.Count(y => y.Value == address) == 1).WithMessage(localizer["ValidationAddressAlreadyExists"]);
                }).When(x => x.ValueType == ScopeResolverPropertyValueTypes.IPv4AddressList);
            });
        }
    }
}

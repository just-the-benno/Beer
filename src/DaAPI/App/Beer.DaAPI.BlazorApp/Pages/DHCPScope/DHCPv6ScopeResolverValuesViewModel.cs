using Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes;
using Beer.DaAPI.BlazorApp.Resources;
using Beer.DaAPI.BlazorApp.Validation;
using Beer.DaAPI.Core.Scopes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPScopes
{
    public class DHCPScopeResolverValuesViewModel
    {
        public Boolean IsListValue { get; }
        public Boolean IsTextValue { get; }
        public Boolean IsNumericValue { get; }
        public Boolean IsNullableNumericValue { get; }
        public Boolean IsBooleanValue { get; }

        public ScopeResolverPropertyValueTypes ValueType { get; }
        public String Name { get; }

        public IEnumerable<DHCPScopeResolverValuesViewModel> OtherItems { get; }

        public DHCPScopeResolverValuesViewModel(
            String propertyName, ScopeResolverPropertyValueTypes valueType,
            IEnumerable<DHCPScopeResolverValuesViewModel> otherItems)
        {
            ValueType = valueType;
            Name = propertyName;
            OtherItems = otherItems;

            switch (valueType)
            {
                case ScopeResolverPropertyValueTypes.IPv4Address:
                case ScopeResolverPropertyValueTypes.String:
                case ScopeResolverPropertyValueTypes.IPv4Subnetmask:
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
                    IsNumericValue = true;
                    break;
                case ScopeResolverPropertyValueTypes.NullableUInt32:
                    IsNullableNumericValue = true;
                    break;
                case ScopeResolverPropertyValueTypes.IPv4AddressList:
                    IsListValue = true;
                    break;
                case ScopeResolverPropertyValueTypes.Boolean:
                    IsBooleanValue = true;
                    break;
                default:
                    break;
            }
        }

        public DHCPScopeResolverValuesViewModel(
             String propertyName, ScopeResolverPropertyValueTypes valueType,
            IEnumerable<DHCPScopeResolverValuesViewModel> otherItems,
            String rawValue) : this(propertyName, valueType, otherItems)
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
                if(String.IsNullOrEmpty(rawValue) == true || rawValue == "null")
                {
                    NullableNumericValue = null;
                }
                else
                {
                    NullableNumericValue = Convert.ToInt64(rawValue);
                }
            }
            else if(IsListValue == true)
            {
                Addresses = new List<SimpleIPv4AddressString>();
                foreach (var item in System.Text.Json.JsonSerializer.Deserialize<List<String>>(rawValue).Select(x => new SimpleIPv4AddressString(x, Addresses)))
                {
                    Addresses.Add(item);
                }
            }
        }

        [ScopeResolverValuesViewModelValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv6ScopeResolverValuesViewModelValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public String SingleValue { get; set; }

        [ScopeResolverValuesViewModelValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv6ScopeResolverValuesViewModelValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Int64? NullableNumericValue { get; set; }

        [ScopeResolverValuesViewModelValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv6ScopeResolverValuesViewModelValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Int64 NumericValue { get; set; }

        public Boolean BooleanValue { get; set; }

        [ScopeResolverValuesViewModelValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv6ScopeResolverValuesViewModelValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public IList<SimpleIPv4AddressString> Addresses { get; private set; } = new List<SimpleIPv4AddressString>();

        public void AddEmptyValue()
        {
            Addresses.Add(new SimpleIPv4AddressString("0.0.0.0"));
        }

        public void RemoveValue(Int32 index)
        {
            Addresses.RemoveAt(index);
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
}

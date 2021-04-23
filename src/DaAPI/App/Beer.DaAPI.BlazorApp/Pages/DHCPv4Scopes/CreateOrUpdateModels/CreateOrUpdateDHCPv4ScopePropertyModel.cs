using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.BlazorApp.ModelHelper;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Shared.Responses;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1.DHCPv4ScopeAddressPropertyReqest;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public class IPv4AddressForScopePropertyViewModel : ValueObjectWithParent<String, CreateOrUpdateDHCPv4ScopePropertyModel>
    {
        public IPv4AddressForScopePropertyViewModel(String value, CreateOrUpdateDHCPv4ScopePropertyModel parent) : base(
              value ?? String.Empty, parent)
        {
        }
    }

    public class CreateOrUpdateDHCPv4ScopePropertyModel
    {
        public static IEnumerable<DHCPv4ScopePropertyType> PropertyTypes { get; } = new[] {
            DHCPv4ScopePropertyType.AddressList,
            DHCPv4ScopePropertyType.Address,
            DHCPv4ScopePropertyType.Boolean,
            DHCPv4ScopePropertyType.Byte,
            DHCPv4ScopePropertyType.UInt16,
            DHCPv4ScopePropertyType.UInt32,
            DHCPv4ScopePropertyType.Text,
            DHCPv4ScopePropertyType.Time,
        };

        public static Dictionary<Byte, (String DisplayName, DHCPv4ScopePropertyType Type)> WellknowOptions
        { get; private set; } =
    new Dictionary<Byte, (string DisplayName, DHCPv4ScopePropertyType Type)>
{
            { 3,  ("Gateway", DHCPv4ScopePropertyType.AddressList) },
            { 6,  ("DNS-Server", DHCPv4ScopePropertyType.AddressList) },
            { 15,   ("Domain Name", DHCPv4ScopePropertyType.Text) },
            { 42,   ("NTP Servers", DHCPv4ScopePropertyType.AddressList) },
            { 4,  ("Time-Server", DHCPv4ScopePropertyType.AddressList) },
            { 5,  ("Name-Server", DHCPv4ScopePropertyType.AddressList) },
            { 7,   ("Log-Server", DHCPv4ScopePropertyType.AddressList) },
            { 8,   ("Cookie Server", DHCPv4ScopePropertyType.AddressList) },
            { 9,   ("LRP Server", DHCPv4ScopePropertyType.AddressList) },
            { 10,   ("Impress Server", DHCPv4ScopePropertyType.AddressList) },
            { 11,   ("Resouce Location Server", DHCPv4ScopePropertyType.AddressList) },
            { 12,   ("Host Name", DHCPv4ScopePropertyType.Text) },
            { 13,   ("Boot File Size", DHCPv4ScopePropertyType.UInt16) },
            { 14,   ("Merit Dump File", DHCPv4ScopePropertyType.Text) },
            { 16,   ("Swap Server", DHCPv4ScopePropertyType.AddressList) },
            { 17,   ("Root Path", DHCPv4ScopePropertyType.Text) },
            { 19,   ("IP Forwarding", DHCPv4ScopePropertyType.Boolean) },
            { 20,   ("Non-Local Source Routing", DHCPv4ScopePropertyType.Boolean) },
            { 21,   ("Policy Filter", DHCPv4ScopePropertyType.AddressAndMask) },
            { 22,   ("Maximum Datagram Reassembly", DHCPv4ScopePropertyType.UInt16) },
            { 23,   ("Default IP Time-to-live", DHCPv4ScopePropertyType.Byte) },
            { 24,   ("Path MTU Aging Timeout", DHCPv4ScopePropertyType.Time) },
            { 26,   ("Interface MTU", DHCPv4ScopePropertyType.UInt16) },
            { 27,   ("All Subnets are Local", DHCPv4ScopePropertyType.Boolean) },
            { 28,   ("Broadcast Address", DHCPv4ScopePropertyType.Address) },
            { 29,   ("Perform Mask Discovery", DHCPv4ScopePropertyType.Boolean) },
            { 30,   ("Mask Supplier", DHCPv4ScopePropertyType.Boolean) },
            { 31,   ("Perform Router Discovery", DHCPv4ScopePropertyType.Boolean) },
            { 32,   ("Router Solicitation Address", DHCPv4ScopePropertyType.Address) },
            //{ 33,   ("Static Route", DHCPv4ScopePropertyType.RouteList) },
            { 34,   ("Trailer Encapsulation", DHCPv4ScopePropertyType.Boolean) },
            { 35,   ("ARP Cache Timeout", DHCPv4ScopePropertyType.Time) },
            { 36,   ("Ethernet Encapsulation", DHCPv4ScopePropertyType.Boolean) },
            { 37,   ("TCP Default TTL", DHCPv4ScopePropertyType.Byte) },
            { 38,   ("TCP Keepalive Interval", DHCPv4ScopePropertyType.Time) },
            { 39,   ("TCP Keepalive Garbage", DHCPv4ScopePropertyType.Boolean) },
            { 40,   ("Network Information Service Domain", DHCPv4ScopePropertyType.Text) },
            { 41,   ("Network Information Servers", DHCPv4ScopePropertyType.AddressList) },
            { 44,   ("NetBIOS over TCP/IP Name Server", DHCPv4ScopePropertyType.AddressList) },
            { 45,   ("NetBIOS over TCP/IP Datagram Distribution Server", DHCPv4ScopePropertyType.AddressList) },
            { 49,   ("X Window System Display Manager", DHCPv4ScopePropertyType.AddressList) },
            { 64,   ("Network Information Service+ Domain", DHCPv4ScopePropertyType.AddressList) },
            { 65,   ("Network Information Service+ Servers", DHCPv4ScopePropertyType.AddressList) },
            { 69,   ("SMTP Server", DHCPv4ScopePropertyType.AddressList) },
            { 70,   ("POP3 Server", DHCPv4ScopePropertyType.AddressList) },
            { 71,   ("NNTP Server", DHCPv4ScopePropertyType.AddressList) },
            { 72,   ("Default World Wide Web Server", DHCPv4ScopePropertyType.AddressList) },
            { 73,   ("Default Finger Server", DHCPv4ScopePropertyType.AddressList) },
            { 74,   ("IRC Server", DHCPv4ScopePropertyType.AddressList) },
            { 75,   ("StreetTalk  Server", DHCPv4ScopePropertyType.AddressList) },
            { 76,   ("StreetTalk Directory Assistance Server", DHCPv4ScopePropertyType.AddressList) },
            { 66,   ("TFTP Server Name", DHCPv4ScopePropertyType.Text) },
            { 67,   ("Bootfile Name", DHCPv4ScopePropertyType.Text) },
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
            AddressesValues.Add(new IPv4AddressForScopePropertyViewModel("0.0.0.0", this));
        }

        private readonly DHCPv4ScopePropertyResponse _parent;

        private DHCPv4ScopePropertyType _propertyType;

        public DHCPv4ScopePropertyType PropertyType
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

        private void SetDefaultValue(DHCPv4ScopePropertyType value)
        {
            switch (value)
            {
                case DHCPv4ScopePropertyType.Address:
                case DHCPv4ScopePropertyType.Subnet:
                    AddressValue = new IPv4AddressForScopePropertyViewModel("0.0.0.0", this);
                    break;
                case DHCPv4ScopePropertyType.AddressList:
                    AddressesValues.Add(new IPv4AddressForScopePropertyViewModel("0.0.0.0", this));
                    break;
                case DHCPv4ScopePropertyType.Time:
                case DHCPv4ScopePropertyType.TimeOffset:
                    TimeValue = TimeSpan.FromHours(2);
                    break;
                case DHCPv4ScopePropertyType.Boolean:
                    BooleanValue = true;
                    break;
                case DHCPv4ScopePropertyType.Byte:
                case DHCPv4ScopePropertyType.UInt16:
                case DHCPv4ScopePropertyType.UInt32:
                    if (IsNumericType() == false)
                    {
                        NumericValue = 0;
                    }
                    break;
                case DHCPv4ScopePropertyType.Text:
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
            BooleanValue = null;
            TimeValue = null;
            AddressesValues.Clear();
            AddressValue = new IPv4AddressForScopePropertyViewModel(String.Empty, this);
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

        private Byte _optionCode;

        public Byte OptionCode
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
                        PropertyType = DHCPv4ScopePropertyType.Text;
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
        public Boolean? BooleanValue { get; set; }
        public TimeSpan? TimeValue { get; set; }
        public IList<IPv4AddressForScopePropertyViewModel> AddressesValues { get; private set; } = new List<IPv4AddressForScopePropertyViewModel>();
        public IPv4AddressForScopePropertyViewModel AddressValue { get; set; }

        public CreateOrUpdateDHCPv4ScopePropertyModel(Byte optionCode)
        {
            OptionCode = optionCode;

        }

        public CreateOrUpdateDHCPv4ScopePropertyModel(DHCPv4ScopePropertyResponse response, Boolean setAsParent)
        {
            if(setAsParent == true)
            {
                IsSetByParent = true;
                _parent = response;
            }    

            OptionCode = response.OptionCode;
            PropertyType = response.Type;

            SetValueFromResponse(response);
        }

        private void SetValueFromResponse(DHCPv4ScopePropertyResponse response)
        {
            switch (response)
            {
                case DHCPv4AddressListScopePropertyResponse property:
                    AddressesValues.Clear();
                    foreach (var item in property.Addresses)
                    {
                        AddAddress(item);
                    }
                    break;
                case DHCPv4AddressScopePropertyResponse property:
                    AddressValue = new IPv4AddressForScopePropertyViewModel(property.Value, this);
                    break;
                case DHCPv4BooleanScopePropertyResponse property:
                    BooleanValue = property.Value;
                    break;
                case DHCPv4NumericScopePropertyResponse property:
                    NumericValue = property.Value;
                    break;
                case DHCPv4TextScopePropertyResponse property:
                    TextValue = property.Value;
                    break;
                case DHCPv4TimeScopePropertyResponse property:
                    TimeValue = property.Value;
                    break;
                default:
                    break;
            }
        }

        public void AddAddress() => AddAddress(String.Empty);
        public void AddAddress(String content) => AddressesValues.Add(new IPv4AddressForScopePropertyViewModel(content, this));
        public void RemoveAddress(Int32 index) => AddressesValues.RemoveAt(index);

        internal bool IsNumericType() => PropertyType ==
            DHCPv4ScopePropertyType.Byte ||
            PropertyType == DHCPv4ScopePropertyType.UInt16 ||
            PropertyType == DHCPv4ScopePropertyType.UInt32;

        internal DHCPv4ScopePropertyRequest ToRequest()
        {
            DHCPv4ScopePropertyRequest request = PropertyType switch
            {
                DHCPv4ScopePropertyType.AddressList => new DHCPv4AddressListScopePropertyRequest
                {
                    Addresses = AddressesValues.Select(x => x.Value).ToList(),
                },
                DHCPv4ScopePropertyType.Address => new DHCPv4AddressScopePropertyRequest
                {
                    Address = AddressValue.Value,
                },
                DHCPv4ScopePropertyType.Byte => new DHCPv4NumericScopePropertyRequest
                {
                    NumericType = DHCPv4NumericValueTypes.Byte,
                    Value = NumericValue.Value,
                },
                DHCPv4ScopePropertyType.UInt16 => new DHCPv4NumericScopePropertyRequest
                {
                    NumericType = DHCPv4NumericValueTypes.UInt16,
                    Value = NumericValue.Value,
                },
                DHCPv4ScopePropertyType.UInt32 => new DHCPv4NumericScopePropertyRequest
                {
                    NumericType = DHCPv4NumericValueTypes.UInt32,
                    Value = NumericValue.Value,
                },
                DHCPv4ScopePropertyType.Boolean => new DHCPv4BooleanScopePropertyRequest
                {
                    Value = BooleanValue.Value,
                },
                DHCPv4ScopePropertyType.Time => new DHCPv4TimeScopePropertyRequest
                {
                    Value = TimeValue.Value,
                },
                DHCPv4ScopePropertyType.Text => new DHCPv4TextScopePropertyRequest
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

        internal void UpdateFromResponse(DHCPv4ScopePropertyResponse item)
        {
            OverrideParentValue = true;
            ResetValues();
            SetValueFromResponse(item);
        }
    }

    public class CreateOrUpdateDHCPv4ScopePropertiesViewModel
    {
        public IList<CreateOrUpdateDHCPv4ScopePropertyModel> Properties { get; private set; } = new List<CreateOrUpdateDHCPv4ScopePropertyModel>();

        internal void LoadFromParent(DHCPv4ScopePropertiesResponse parent)
        {
            Properties.Clear();

            foreach (var item in parent.Properties)
            {
                Properties.Add(new CreateOrUpdateDHCPv4ScopePropertyModel(item, true));
            }
        }

        public Boolean PropertiesHasParents() => Properties.Any(x => x.IsSetByParent == true);

        internal IEnumerable<DHCPv4ScopePropertyRequest> GetRequest() => Properties.Where(x => x.IsSetByParent == false || x.OverrideParentValue == true || x.MarkAsRemovedInInheritance == true)
            .Select(x => x.ToRequest());

        internal void LoadFromResponse(DHCPv4ScopePropertiesResponse properties)
        {
            foreach (var item in properties.Properties)
            {
                var existing = Properties.FirstOrDefault(x => x.OptionCode == item.OptionCode);
                if(existing == null)
                {
                    Properties.Add(new CreateOrUpdateDHCPv4ScopePropertyModel(item, false));
                }
                else
                {
                    existing.UpdateFromResponse(item);
                }
            }
        }
    }

    public class CreateOrUpdateDHCPv4ScopePropertiesViewModellValidator : AbstractValidator<CreateOrUpdateDHCPv4ScopePropertiesViewModel>
    {
        public CreateOrUpdateDHCPv4ScopePropertiesViewModellValidator(IStringLocalizer<CreateOrUpdateDHCPv4ScopePage> localizer)
        {
            RuleForEach(x => x.Properties).ChildRules(element =>
            {
                element.RuleFor(x => x.BooleanValue).NotNull().When(x => x.PropertyType == DHCPv4ScopePropertyType.Boolean);
                element.RuleFor(x => x.NumericValue).NotNull().When(x => x.IsNumericType(), ApplyConditionTo.CurrentValidator)
                .LessThanOrEqualTo(Byte.MaxValue).When(x => x.PropertyType == DHCPv4ScopePropertyType.Byte, ApplyConditionTo.CurrentValidator)
                .LessThanOrEqualTo(UInt16.MaxValue).When(x => x.PropertyType == DHCPv4ScopePropertyType.UInt16, ApplyConditionTo.CurrentValidator)
                .LessThanOrEqualTo(UInt32.MaxValue).When(x => x.PropertyType == DHCPv4ScopePropertyType.UInt32, ApplyConditionTo.CurrentValidator)
                .WithName(localizer["OptionalPropertiesValueLabel"]);

                element.RuleFor(x => x.TextValue).NotNull().When(x => x.PropertyType == DHCPv4ScopePropertyType.Text);
                element.RuleFor(x => x.AddressValue).NotNull().Must(x => x.Value.AsIPv4Address() != IPv4Address.Empty).WithMessage(localizer["ValidationNotIPv4Address"]).When(x => x.PropertyType == DHCPv4ScopePropertyType.Address);
                element.RuleFor(x => x.AddressesValues).Must(x => x.Count > 0).WithMessage(localizer["ValidationOptionalPropertiesAddressesArtEmpty"]).When(x => x.PropertyType == DHCPv4ScopePropertyType.AddressList); ;
                element.RuleForEach(x => x.AddressesValues).ChildRules(element2 =>
                {
                    element2.RuleFor(x => x.Value).Cascade(CascadeMode.Stop).InjectIPv4AdressValidator(localizer["SingleAddtionalOptionAddressLabel"]);
                    element2.RuleFor(x => x.Value)
                    .Must((properties, address) => properties.Parent.AddressesValues.Count(y => y.Value == address) == 1).WithMessage(localizer["ValidationOptionalAddressAlreadyExists"]);
                });
            });
        }
    }
}


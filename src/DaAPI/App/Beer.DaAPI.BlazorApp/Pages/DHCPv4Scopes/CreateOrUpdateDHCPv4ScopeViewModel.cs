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

    public class ValueObjectWithParent<TValue, TParent>
    {
        public ValueObjectWithParent(TValue value, TParent parent)
        {
            Value = value;
            Parent = parent;
        }

        public TValue Value { get; set; }
        public TParent Parent { get; init; }
    }

    public class ExcludedAddressItem : ValueObjectWithParent<String, CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel>
    {
        public ExcludedAddressItem(String value, CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel parent) : base(
            value ?? String.Empty, parent)
        {
        }
    }

    public class IPv4AddressForScopePropertyViewModel : ValueObjectWithParent<String, CreateOrUpdateDHCPv4ScopePropertyModel>
    {
        public IPv4AddressForScopePropertyViewModel(String value, CreateOrUpdateDHCPv4ScopePropertyModel parent) : base(
              value ?? String.Empty, parent)
        {
        }
    }

    public class CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModel
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public Boolean HasParent { get; set; }
        public Guid? ParentId { get; set; }
        public Guid Id { get; set; }
    }

    public class NullableOverrideProperty<T> where T : struct
    {
        public T DefaultValue { get; set; }
        public T? NullableValue { get; set; }
        public T Value { get => NullableValue.Value; }

        private bool _overrideValue = false;

        public Boolean OverrideValue
        {
            get => _overrideValue;
            set
            {
                _overrideValue = value;

                if (value == true)
                {
                    NullableValue = DefaultValue;
                }
                else
                {
                    NullableValue = null;
                }
            }
        }
        public bool HasValue { get => NullableValue.HasValue; }

        public static implicit operator T?(NullableOverrideProperty<T> value) => value.Value;

        internal void UpdateNullableValue(T? value, bool overrideIfNeeded, Func<T?,T?,Boolean> equalityChecker = null)
        {
            if(overrideIfNeeded == true)
            {
                if (value is IEquatable<T> betterEqual)
                {
                    OverrideValue = betterEqual.Equals(DefaultValue) == false;
                }
                else if (equalityChecker != null)
                {
                    OverrideValue = equalityChecker(value, DefaultValue) == false;
                }
                else
                {
                    OverrideValue = value.Equals(DefaultValue);
                }
            }

            NullableValue = value;
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

    public class CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel
    {
        public NullableOverrideProperty<Byte> SubnetMask { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> RenewalTime { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> PreferredLifetime { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> LeaseTime { get; set; } = new();

        public NullableOverrideProperty<Boolean> SupportDirectUnicast { get; set; } = new();
        public NullableOverrideProperty<Boolean> AcceptDecline { get; set; } = new();
        public NullableOverrideProperty<Boolean> InformsAreAllowd { get; set; } = new();
        public NullableOverrideProperty<Boolean> ReuseAddressIfPossible { get; set; } = new();

        public NullableOverrideProperty<AddressAllocationStrategies> AddressAllocationStrategy { get; set; } = new();

        public String Start { get; set; }
        public String End { get; set; }
        public IList<ExcludedAddressItem> ExcludedAddresses { get; set; } = new List<ExcludedAddressItem>();

        public CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel ParentValues { get; set; }

        public void SetDefaults()
        {
            Start = "172.16.0.1";
            End = "172.16.255.255";

            SubnetMask = new() { NullableValue = 16 };
            RenewalTime = new() { NullableValue = TimeSpan.FromHours(6) };
            PreferredLifetime = new() { NullableValue = TimeSpan.FromHours(12) };
            LeaseTime = new() { NullableValue = TimeSpan.FromHours(24) };
            SupportDirectUnicast = new() { NullableValue = true };
            AcceptDecline = new() { NullableValue = false };
            InformsAreAllowd = new() { NullableValue = true };
            ReuseAddressIfPossible = new() { NullableValue = false };

            AddressAllocationStrategy = new() { NullableValue = AddressAllocationStrategies.Random };
        }

        internal void RemoveParent()
        {
            ParentValues = null;
            SetDefaults();
        }

        public void AddDefaultExcludedAddress()
        {
            ExcludedAddresses.Add(new ExcludedAddressItem(Start.AsIPv4Address().ToString(), this));
        }

        internal void RemoveExcludedAddress(Int32 index)
        {
            if (index < ExcludedAddresses.Count)
            {
                ExcludedAddresses.RemoveAt(index);
            }
        }

        internal void SetParent(DHCPv4ScopeResponses.V1.DHCPv4ScopePropertiesResponse parent)
        {
            ParentValues = new()
            {
                Start = parent.AddressRelated.Start,
                End = parent.AddressRelated.End,
                ExcludedAddresses = parent.AddressRelated.ExcludedAddresses.Select(x => new ExcludedAddressItem(x, this)).ToList(),
                SubnetMask = new() { NullableValue = parent.AddressRelated.Mask },
                RenewalTime = new() { NullableValue = parent.AddressRelated.RenewalTime },
                PreferredLifetime = new() { NullableValue = parent.AddressRelated.PreferredLifetime },
                LeaseTime = new() { NullableValue = parent.AddressRelated.LeaseTime },
                SupportDirectUnicast = new() { NullableValue = parent.AddressRelated.SupportDirectUnicast.Value },
                AcceptDecline = new() { NullableValue = parent.AddressRelated.AcceptDecline.Value },
                InformsAreAllowd = new() { NullableValue = parent.AddressRelated.InformsAreAllowd.Value },
                ReuseAddressIfPossible = new() { NullableValue = parent.AddressRelated.ReuseAddressIfPossible.Value },
                AddressAllocationStrategy = new() { NullableValue = parent.AddressRelated.AddressAllocationStrategy.Value },
            };

            Start = ParentValues.Start;
            End = ParentValues.End;

            SubnetMask = new() { DefaultValue = parent.AddressRelated.Mask.Value };
            PreferredLifetime = new() { DefaultValue = parent.AddressRelated.PreferredLifetime.Value };
            RenewalTime = new() { DefaultValue = parent.AddressRelated.RenewalTime.Value };
            LeaseTime = new() { DefaultValue = parent.AddressRelated.LeaseTime.Value };
            SupportDirectUnicast = new() { DefaultValue = parent.AddressRelated.SupportDirectUnicast.Value };
            AcceptDecline = new() { DefaultValue = parent.AddressRelated.AcceptDecline.Value };
            InformsAreAllowd = new() { DefaultValue = parent.AddressRelated.InformsAreAllowd.Value };
            ReuseAddressIfPossible = new() { DefaultValue = parent.AddressRelated.ReuseAddressIfPossible.Value };
            AddressAllocationStrategy = new() { DefaultValue = parent.AddressRelated.AddressAllocationStrategy.Value };

        }

        internal DHCPv4ScopeAddressPropertyReqest GetRequest() => new()
        {
            Start = Start,
            End = End,
            ExcludedAddresses = ExcludedAddresses.Select(x => x.Value).ToList(),
            MaskLength = SubnetMask.NullableValue,
            PreferredLifetime = PreferredLifetime.NullableValue,
            LeaseTime = LeaseTime.NullableValue,
            RenewalTime = RenewalTime.NullableValue,
            AcceptDecline = AcceptDecline.NullableValue,
            InformsAreAllowd = InformsAreAllowd.NullableValue,
            ReuseAddressIfPossible = ReuseAddressIfPossible.NullableValue,
            SupportDirectUnicast = SupportDirectUnicast.NullableValue,
            AddressAllocationStrategy = AddressAllocationStrategy.NullableValue,
        };

        internal void SetByResponse(DHCPv4ScopePropertiesResponse properties)
        {
            var addressRelatedProperties = properties.AddressRelated;

            Start = addressRelatedProperties.Start;
            End = addressRelatedProperties.End;

            SubnetMask.UpdateNullableValue(addressRelatedProperties.Mask, ParentValues != null);
           
            PreferredLifetime.UpdateNullableValue(addressRelatedProperties.PreferredLifetime, ParentValues != null);
            RenewalTime.UpdateNullableValue(addressRelatedProperties.RenewalTime, ParentValues != null);
            LeaseTime.UpdateNullableValue(addressRelatedProperties.LeaseTime, ParentValues != null);
            SupportDirectUnicast.UpdateNullableValue(addressRelatedProperties.SupportDirectUnicast, ParentValues != null);
            AcceptDecline.UpdateNullableValue(addressRelatedProperties.AcceptDecline, ParentValues != null);
            InformsAreAllowd.UpdateNullableValue(addressRelatedProperties.InformsAreAllowd, ParentValues != null);
            ReuseAddressIfPossible.UpdateNullableValue(addressRelatedProperties.ReuseAddressIfPossible, ParentValues != null);
            AddressAllocationStrategy.UpdateNullableValue(addressRelatedProperties.AddressAllocationStrategy, ParentValues != null, (v1,v2) => v1 == v2);
        }
    }

    public class CreateOrUpdateDHCPv4ScopeViewModel
    {
        public CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModel GenerellProperties { get; set; }
        public CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel AddressRelatedProperties { get; set; }
        public CreateOrUpdateDHCPv4ScopePropertiesViewModel ScopeProperties { get; set; }
        public CreateOrUpdateDHCPv4ScopeResolverRelatedViewModel ResolverProperties { get; set; }

        public CreateOrUpdateDHCPv4ScopeRequest GetRequest()
        {
            var request = new CreateOrUpdateDHCPv4ScopeRequest
            {
                Name = GenerellProperties.Name,
                Description = GenerellProperties.Description,
                ParentId = GenerellProperties.HasParent == false ? new Guid?() : GenerellProperties.ParentId,
                AddressProperties = AddressRelatedProperties.GetRequest(),
                Properties = ScopeProperties.GetRequest(),
                Resolver = ResolverProperties.GetRequest(),
            };

            return request;
        }
    }

    public class CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModelValidator : AbstractValidator<CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModel>
    {
        public CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModelValidator(IStringLocalizer<CreateOrUpdateDHCPv4ScopePage> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100).WithName(localizer["NameLabel"]);
            RuleFor(x => x.Description).MinimumLength(3).When(x => String.IsNullOrEmpty(x.Description) == false).MaximumLength(250).WithName(localizer["Description"]);
            RuleFor(x => x.ParentId).NotNull().WithName(localizer["InterfaceIdLabel"]).When(x => x.HasParent == true);
        }
    }

    public class CreateOrUpdateDHCPv4ScopeAdressRelatedPropertiesViewModelValidator : AbstractValidator<CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel>
    {
        public CreateOrUpdateDHCPv4ScopeAdressRelatedPropertiesViewModelValidator(IStringLocalizer<CreateOrUpdateDHCPv4ScopePage> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(y => y.Start).Cascade(CascadeMode.Stop).InjectIPv4AdressValidator(localizer["StartAddressLabel"]);
            RuleFor(y => y.Start).Must((properties, startAddress) => startAddress.AsIPv4Address() <= properties.End.AsIPv4Address()).WithMessage(localizer["ValidationStartSmallerThenEnd"]).When(x => x.End.AsIPv4Address() != IPv4Address.Empty, ApplyConditionTo.CurrentValidator).WithName(localizer["StartAddressLabel"]);

            RuleFor(y => y.End).Cascade(CascadeMode.Stop).InjectIPv4AdressValidator(localizer["EndAddressLabel"]);
            RuleFor(y => y.End).Must((properties, endAddress) => endAddress.AsIPv4Address() >= properties.Start.AsIPv4Address()).WithMessage(localizer["ValidationEndGreaterThenStart"]).When(x => x.Start.AsIPv4Address() != IPv4Address.Empty, ApplyConditionTo.CurrentValidator).WithName(localizer["EndAddressLabel"]);

            When(x => x.ParentValues != null, () =>
           {
               RuleFor(y => y.Start).Cascade(CascadeMode.Stop)
               .Must((properties, address) => address.AsIPv4Address() >= properties.ParentValues.Start.AsIPv4Address()).WithMessage(localizer["ValidationStartAddressSmallerThenInParentRange"])
               .Must((properties, address) => address.AsIPv4Address() <= properties.ParentValues.End.AsIPv4Address()).WithMessage(localizer["ValidationStartAddressGreaterThenInParentRangeEnd"]);

               RuleFor(y => y.End).Cascade(CascadeMode.Stop)
                .Must((properties, address) => address.AsIPv4Address() <= properties.ParentValues.End.AsIPv4Address()).WithMessage(localizer["ValidationEndAddressBiggerThenEndInParentRange"])
                .Must((properties, address) => address.AsIPv4Address() >= properties.ParentValues.Start.AsIPv4Address()).WithMessage(localizer["ValidationEndAddressSmallerThenStartInParentRange"]);

           }).Otherwise(() =>
           {
               RuleFor(x => x.SubnetMask.NullableValue).NotEmpty().WithName(localizer["SubnetMaskLabel"]);
               RuleFor(x => x.RenewalTime.NullableValue).NotEmpty().WithName(localizer["RenewalTimeLabel"]);
               RuleFor(x => x.PreferredLifetime.NullableValue).NotEmpty().WithName(localizer["PreferredLifetimeLabel"]);
               RuleFor(x => x.LeaseTime.NullableValue).NotEmpty().WithName(localizer["LeaseTimeLabel"]);
               RuleFor(x => x.SupportDirectUnicast.NullableValue).NotEmpty().WithName(localizer["SupportUnicastLabel"]);
               RuleFor(x => x.AcceptDecline.NullableValue).NotEmpty().WithName(localizer["AccpetDeclinesLabel"]);
               RuleFor(x => x.InformsAreAllowd.NullableValue).NotEmpty().WithName(localizer["AccpetInformsLabel"]);
               RuleFor(x => x.ReuseAddressIfPossible.NullableValue).NotEmpty().WithName(localizer["ReuseAddressLabel"]);
               RuleFor(x => x.AddressAllocationStrategy.NullableValue).NotEmpty().WithName(localizer["AddressAllocationStrategyLabel"]);
           });

            RuleForEach(x => x.ExcludedAddresses).ChildRules(element =>
            {
                element.RuleFor(x => x.Value).Cascade(CascadeMode.Stop).InjectIPv4AdressValidator(localizer["SingleExcludedAddressLabel"]);
                element.RuleFor(x => x.Value)
                .Must((properties, address) => address.AsIPv4Address() >= properties.Parent.Start.AsIPv4Address()).When(x => x.Parent != null, ApplyConditionTo.CurrentValidator).WithMessage(localizer["ValidationExcludedAddressSmallerThenStart"])
                .Must((properties, address) => address.AsIPv4Address() <= properties.Parent.End.AsIPv4Address()).When(x => x.Parent != null, ApplyConditionTo.CurrentValidator).WithMessage(localizer["ValidationExcludedAddressGreaterThenEnd"])
                .Must((properties, address) => properties.Parent.ExcludedAddresses.Count(y => y.Value == address) == 1).When(x => x.Parent != null, ApplyConditionTo.CurrentValidator).WithMessage(localizer["ValidationExcludedAddressAlreadyExists"])
                .Must((properties, address) => !properties.Parent.ParentValues.ExcludedAddresses.Any(y => y.Value == address)).WithMessage(localizer["ValidationExcludedAddressAlreadyExists"]).When(x => x.Parent.ParentValues != null, ApplyConditionTo.CurrentValidator);
            });

            Transform(x => x.SubnetMask.NullableValue, input => (Int32)input.Value).GreaterThanOrEqualTo(0).LessThanOrEqualTo(32).When(x => x.SubnetMask.HasValue == true).WithName(localizer["SubnetMaskLabel"]);

            When(y => y.RenewalTime.NullableValue.HasValue, () =>
            {
                Transform(x => x.RenewalTime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.PreferredLifetime.Value).WithMessage(localizer["ValidationRenewalTimeGreaterThanPreferredLifetime"]).When(y => y.PreferredLifetime.HasValue);
                Transform(x => x.RenewalTime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.ParentValues.PreferredLifetime.Value).WithMessage(localizer["ValidationRenewalTimeGreaterThanPreferredLifetimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.PreferredLifetime.HasValue && y.PreferredLifetime.HasValue == false);
            });

            When(y => y.PreferredLifetime.NullableValue.HasValue, () =>
            {
                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.RenewalTime.Value).WithMessage(localizer["ValidationPreferredLifetimeSmallerRenewalTime"]).When(y => y.RenewalTime.HasValue);
                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.ParentValues.RenewalTime.Value).WithMessage(localizer["ValidationPreferredLifetimeSmallerRenewalTimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.RenewalTime.HasValue && y.RenewalTime.HasValue == false);

                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.LeaseTime.Value).WithMessage(localizer["ValidationPreferredLifetimeGreaterThanLeaseTime"]).When(y => y.LeaseTime.HasValue);
                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.ParentValues.LeaseTime.Value).WithMessage(localizer["ValidationPreferredLifetimeGreaterThanLeaseTimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.LeaseTime.HasValue && y.LeaseTime.HasValue == false);
            });

            When(y => y.LeaseTime.NullableValue.HasValue, () =>
            {
                Transform(x => x.LeaseTime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.PreferredLifetime.Value).WithMessage(localizer["ValidationLeaseTimeSmallerThanPreferredLifetime"]).When(y => y.PreferredLifetime.HasValue);
                Transform(x => x.LeaseTime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.ParentValues.PreferredLifetime.Value).WithMessage(localizer["ValidationLeaseTimeSmallerThanPreferredLifetimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.PreferredLifetime.HasValue && y.PreferredLifetime.HasValue == false);
            });

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


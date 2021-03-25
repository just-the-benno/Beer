using Beer.DaAPI.BlazorApp.Resources;
using Beer.DaAPI.BlazorApp.Resources.Pages.DHCPv4Scopes;
using Beer.DaAPI.BlazorApp.Validation;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public class DHCPv4ScopePropertyViewModel
    {
        public static IEnumerable<DHCPv4ScopePropertyType> Properties = new[] {
            DHCPv4ScopePropertyType.AddressList,
            DHCPv4ScopePropertyType.Address,
            DHCPv4ScopePropertyType.Boolean,
            DHCPv4ScopePropertyType.Byte,
            DHCPv4ScopePropertyType.UInt16,
            DHCPv4ScopePropertyType.UInt32,
            DHCPv4ScopePropertyType.Text,
            DHCPv4ScopePropertyType.Time,
        };

        public static Dictionary<String, (String DisplayName, DHCPv4ScopePropertyType Type)> WellknowOptions
        { get; private set; } =
            new Dictionary<String, (string DisplayName, DHCPv4ScopePropertyType Type)>
        {

            { "6",  ("DNS-Server", DHCPv4ScopePropertyType.AddressList) },
            { "15",   ("Domain Name", DHCPv4ScopePropertyType.Text) },
            { "42",   ("NTP Servers", DHCPv4ScopePropertyType.AddressList) },

            { "4",  ("Time-Server", DHCPv4ScopePropertyType.AddressList) },
            { "5",  ("Name-Server", DHCPv4ScopePropertyType.AddressList) },
            { "7",   ("Log-Server", DHCPv4ScopePropertyType.AddressList) },
            { "8",   ("Cookie Server", DHCPv4ScopePropertyType.AddressList) },
            { "9",   ("LRP Server", DHCPv4ScopePropertyType.AddressList) },
            { "10",   ("Impress Server", DHCPv4ScopePropertyType.AddressList) },
            { "11",   ("Resouce Location Server", DHCPv4ScopePropertyType.AddressList) },
            { "12",   ("Host Name", DHCPv4ScopePropertyType.Text) },
            { "13",   ("Boot File Size", DHCPv4ScopePropertyType.UInt16) },
            { "14",   ("Merit Dump File", DHCPv4ScopePropertyType.Text) },
            { "16",   ("Swap Server", DHCPv4ScopePropertyType.AddressList) },
            { "17",   ("Root Path", DHCPv4ScopePropertyType.Text) },
            { "19",   ("IP Forwarding", DHCPv4ScopePropertyType.Boolean) },
            { "20",   ("Non-Local Source Routing", DHCPv4ScopePropertyType.Boolean) },
            { "21",   ("Policy Filter", DHCPv4ScopePropertyType.AddressAndMask) },
            { "22",   ("Maximum Datagram Reassembly", DHCPv4ScopePropertyType.UInt16) },
            { "23",   ("Default IP Time-to-live", DHCPv4ScopePropertyType.Byte) },
            { "24",   ("Path MTU Aging Timeout", DHCPv4ScopePropertyType.Time) },
            { "26",   ("Interface MTU", DHCPv4ScopePropertyType.UInt16) },
            { "27",   ("All Subnets are Local", DHCPv4ScopePropertyType.Boolean) },
            { "28",   ("Broadcast Address", DHCPv4ScopePropertyType.Address) },
            { "29",   ("Perform Mask Discovery", DHCPv4ScopePropertyType.Boolean) },
            { "30",   ("Mask Supplier", DHCPv4ScopePropertyType.Boolean) },
            { "31",   ("Perform Router Discovery", DHCPv4ScopePropertyType.Boolean) },
            { "32",   ("Router Solicitation Address", DHCPv4ScopePropertyType.Address) },
            //{ "33",   ("Static Route", DHCPv4ScopePropertyType.RouteList) },
            { "34",   ("Trailer Encapsulation", DHCPv4ScopePropertyType.Boolean) },
            { "35",   ("ARP Cache Timeout", DHCPv4ScopePropertyType.Time) },
            { "36",   ("Ethernet Encapsulation", DHCPv4ScopePropertyType.Boolean) },
            { "37",   ("TCP Default TTL", DHCPv4ScopePropertyType.Byte) },
            { "38",   ("TCP Keepalive Interval", DHCPv4ScopePropertyType.Time) },
            { "39",   ("TCP Keepalive Garbage", DHCPv4ScopePropertyType.Boolean) },
            { "40",   ("Network Information Service Domain", DHCPv4ScopePropertyType.Text) },
            { "41",   ("Network Information Servers", DHCPv4ScopePropertyType.AddressList) },
            { "44",   ("NetBIOS over TCP/IP Name Server", DHCPv4ScopePropertyType.AddressList) },
            { "45",   ("NetBIOS over TCP/IP Datagram Distribution Server", DHCPv4ScopePropertyType.AddressList) },
            { "49",   ("X Window System Display Manager", DHCPv4ScopePropertyType.AddressList) },
            { "64",   ("Network Information Service+ Domain", DHCPv4ScopePropertyType.AddressList) },
            { "65",   ("Network Information Service+ Servers", DHCPv4ScopePropertyType.AddressList) },
            { "69",   ("SMTP Server", DHCPv4ScopePropertyType.AddressList) },
            { "70",   ("POP3 Server", DHCPv4ScopePropertyType.AddressList) },
            { "71",   ("NNTP Server", DHCPv4ScopePropertyType.AddressList) },
            { "72",   ("Default World Wide Web Server", DHCPv4ScopePropertyType.AddressList) },
            { "73",   ("Default Finger Server", DHCPv4ScopePropertyType.AddressList) },
            { "74",   ("IRC Server", DHCPv4ScopePropertyType.AddressList) },
            { "75",   ("StreetTalk  Server", DHCPv4ScopePropertyType.AddressList) },
            { "76",   ("StreetTalk Directory Assistance Server", DHCPv4ScopePropertyType.AddressList) },
            { "66",   ("TFTP Server Name", DHCPv4ScopePropertyType.Text) },
            { "67",   ("Bootfile Name", DHCPv4ScopePropertyType.Text) },
            };

        private String _optionCode;

        [Display(Name = nameof(DHCPv4ScopeDisplay.ScopePropertyOptionCode), ResourceType = typeof(DHCPv4ScopeDisplay))]
        [Max(UInt16.MaxValue, ErrorMessageResourceName = nameof(ValidationErrorMessages.Max), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Int32 CustomOptionCode { get; set; }

        public Boolean MarkAsRemovedInInheritance { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.ScopePropertyOptionCode), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public String OptionCode
        {
            get => _optionCode;
            set
            {
                _optionCode = value;
                IsWellknownType = WellknowOptions.ContainsKey(value);
                if (IsWellknownType == true)
                {
                    Type = WellknowOptions[value].Type;
                }
                else
                {
                    _optionCode = "0";
                }

                if (UInt16.TryParse(value, out UInt16 code) == true && code != 0)
                {
                    CustomOptionCode = code;
                }
            }
        }

        public String GetOptionCodeName() => WellknowOptions.ContainsKey(OptionCode) == true ? WellknowOptions[OptionCode].DisplayName : OptionCode;

        public Boolean IsWellknownType { get; private set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.ScopePropertyType), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public DHCPv4ScopePropertyType Type { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public String TextValue { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Int64 NumericValue { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Boolean BooleanValue { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public TimeSpan TimeValue { get; set; }

        public Boolean IsActive { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [ValidateComplexType]
        public IList<SimpleIPv4AddressString> Addresses { get; private set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public SimpleIPv4AddressString Address { get;  set; }


        public DHCPv4ScopePropertyViewModel()
        {
            Addresses = new List<SimpleIPv4AddressString>();
        }

        public DHCPv4ScopePropertyViewModel(DHCPv4ScopePropertyResponse response) : this()
        {
            Type = response.Type;
            OptionCode = response.OptionCode.ToString();

            switch (response)
            {
                case DHCPv4AddressListScopePropertyResponse property:
                    foreach (var item in property.Addresses)
                    {
                        AddAddress(item);
                    }
                    Type = DHCPv4ScopePropertyType.AddressList;
                    break;
                case DHCPv4AddressScopePropertyResponse property:
                    Address = new SimpleIPv4AddressString(property.Value);
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
        public void AddAddress(String content) => Addresses.Add(new SimpleIPv4AddressString(Addresses) { Value = content });
        public void RemoveAddress(Int32 index) => Addresses.RemoveAt(index);

        public DHCPv4ScopePropertyRequest ToRequest()
        {
            DHCPv4ScopePropertyRequest request = Type switch
            {
                DHCPv4ScopePropertyType.AddressList => new DHCPv4AddressListScopePropertyRequest
                {
                    Addresses = Addresses.Select(x => x.Value).ToList(),
                },
                DHCPv4ScopePropertyType.Address => new DHCPv4AddressScopePropertyRequest
                {
                    Address = Address.Value,
                },
                DHCPv4ScopePropertyType.Byte => new DHCPv4NumericScopePropertyRequest
                {
                    NumericType = DHCPv4NumericValueTypes.Byte,
                    Value = NumericValue,
                },
                DHCPv4ScopePropertyType.UInt16 => new DHCPv4NumericScopePropertyRequest
                {
                    NumericType = DHCPv4NumericValueTypes.UInt16,
                    Value = NumericValue,
                },
                DHCPv4ScopePropertyType.UInt32 => new DHCPv4NumericScopePropertyRequest
                {
                    NumericType = DHCPv4NumericValueTypes.UInt32,
                    Value = NumericValue,
                },
                DHCPv4ScopePropertyType.Boolean => new DHCPv4BooleanScopePropertyRequest
                {
                    Value = BooleanValue,
                },
                DHCPv4ScopePropertyType.Time => new DHCPv4TimeScopePropertyRequest
                {
                    Value = TimeValue,
                },
                DHCPv4ScopePropertyType.Text => new DHCPv4TextScopePropertyRequest
                {
                    Value = TextValue,
                },
                _ => throw new NotImplementedException(),
            };

            request.MarkAsRemovedInInheritance = MarkAsRemovedInInheritance;
            request.OptionCode = (Byte)CustomOptionCode;
            request.Type = Type;

            return request;
        }
    }
}

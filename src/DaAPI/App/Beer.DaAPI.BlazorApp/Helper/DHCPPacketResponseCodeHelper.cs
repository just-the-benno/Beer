using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public class DHCPPacketResponseCodeHelper
    {
        private readonly IStringLocalizer<DHCPPacketResponseCodeHelper> _localizer;

        public DHCPPacketResponseCodeHelper(IStringLocalizer<DHCPPacketResponseCodeHelper> localizer)
        {
            this._localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

            if(_dhcpv6ResponseCodesMapper == null)

            _dhcpv6ResponseCodesMapper = new Dictionary<DHCPv6PacketTypes, Dictionary<Int32, (String Name, String Color)>>
{
                { DHCPv6PacketTypes.Solicit, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv6_Solicit_0"],"#28a745") },
                        { 1, (_localizer["DHCPv6_Solicit_1"],"#ffc107") },
                        { 2, (_localizer["DHCPv6_Solicit_2"],"#dc3545") },
                        { 3, (_localizer["DHCPv6_Solicit_3"],"#d81b60") },
                        { 4, (_localizer["DHCPv6_Solicit_4"],"#f012be") },
                        { 5, (_localizer["DHCPv6_Solicit_5"],"#6f42c1") },
                    }
                },
                { DHCPv6PacketTypes.REQUEST, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv6_Request_0"],"#28a745") },
                        { 1, (_localizer["DHCPv6_Request_1"],"#ffc107") },
                        { 3, (_localizer["DHCPv6_Request_3"],"#d81b60") },
                        { 8, (_localizer["DHCPv6_Request_8"],"#f012be") },
                        { 9, (_localizer["DHCPv6_Request_9"],"#6f42c1") },
                    }
                },
                { DHCPv6PacketTypes.RENEW, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv6_Renew_0"],"#28a745") },
                        { 1, (_localizer["DHCPv6_Renew_1"],"#ffc107") },
                        { 3, (_localizer["DHCPv6_Renew_3"],"#d81b60") },
                        { 4, (_localizer["DHCPv6_Renew_4"],"#dc3545") },
                        { 5, (_localizer["DHCPv6_Renew_5"],"#6f42c1") },
                    }
                },
                { DHCPv6PacketTypes.REBIND, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv6_Rebind_0"],"#28a745") },
                        { 1, (_localizer["DHCPv6_Rebind_1"],"#ffc107") },
                        { 3, (_localizer["DHCPv6_Rebind_3"],"#d81b60") },
                        { 4, (_localizer["DHCPv6_Rebind_4"],"#dc3545") },
                        { 5, (_localizer["DHCPv6_Rebind_5"],"#6f42c1") },
                    }
                },
                { DHCPv6PacketTypes.DECLINE, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv6_Decline_0"],"#28a745") },
                        { 1, (_localizer["DHCPv6_Decline_1"],"#ffc107") },
                        { 2, (_localizer["DHCPv6_Decline_2"],"#6f42c1") },
                        { 3, (_localizer["DHCPv6_Decline_3"],"#d81b60") },
                        { 4, (_localizer["DHCPv6_Decline_4"],"#f012be") },
                        { 5, (_localizer["DHCPv6_Decline_5"],"#dc3545") },
                        { 6, (_localizer["DHCPv6_Decline_6"],"#001f3f") },
                    }
                },
                { DHCPv6PacketTypes.RELEASE, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv6_Release_0"],"#28a745") },
                        { 1, (_localizer["DHCPv6_Release_1"],"#d81b60") },
                        { 2, (_localizer["DHCPv6_Release_2"],"#f012be") },
                        { 3, (_localizer["DHCPv6_Release_3"],"#ffc107") },
                    }
                },
                { DHCPv6PacketTypes.CONFIRM, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv6_Confirm_0"],"#28a745") },
                        { 1, (_localizer["DHCPv6_Confirm_1"],"#ffc107") },
                        { 3, (_localizer["DHCPv6_Confirm_3"],"#d81b60") },
                        { 4, (_localizer["DHCPv6_Confirm_4"],"#f012be") },
                        { 5, (_localizer["DHCPv6_Confirm_5"],"#fd7e14") },
                    }
                }
            };

            _dhcpv4ResponseCodesMapper = new Dictionary<DHCPv4MessagesTypes, Dictionary<Int32, (String Name, String Color)>>
{
                { DHCPv4MessagesTypes.Discover, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv4_Discover_0"],"#28a745") },
                        { 1, (_localizer["DHCPv4_Discover_1"],"#ffc107") },
                        { 2, (_localizer["DHCPv4_Discover_2"],"#dc3545") },
                    }
                },
                { DHCPv4MessagesTypes.Request, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv4_Request_0"],"#28a745") },
                        { 1, (_localizer["DHCPv4_Request_1"],"#ffc107") },
                        { 3, (_localizer["DHCPv4_Request_3"],"#d81b60") },
                        { 4, (_localizer["DHCPv4_Request_4"],"#f012be") },
                        { 5, (_localizer["DHCPv4_Request_5"],"#6f42c1") },
                        { 6, (_localizer["DHCPv4_Request_5"],"#dc3545") },
                        { 7, (_localizer["DHCPv4_Request_5"],"#001f3f") },
                    }
                },
                { DHCPv4MessagesTypes.Release, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv4_Release_0"],"#28a745") },
                        { 1, (_localizer["DHCPv4_Release_1"],"#d81b60") },
                        { 2, (_localizer["DHCPv4_Release_2"],"#f012be") },
                    }
                },
                { DHCPv4MessagesTypes.Inform, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv4_Inform_0"],"#28a745") },
                        { 1, (_localizer["DHCPv4_Inform_1"],"#d81b60") },
                        { 2, (_localizer["DHCPv4_Inform_2"],"#f012be") },
                    }
                },
                { DHCPv4MessagesTypes.Decline, new Dictionary<Int32, (String Name, String Color)> {
                        { 0, (_localizer["DHCPv4_Decline_0"],"#28a745") },
                        { 1, (_localizer["DHCPv4_Decline_1"],"#ffc107") },
                        { 2, (_localizer["DHCPv4_Decline_2"],"#d81b60") },
                        { 3, (_localizer["DHCPv4_Decline_3"],"#f012be") },
                        { 4, (_localizer["DHCPv4_Decline_4"],"#6f42c1") },
                        { 5, (_localizer["DHCPv4_Decline_5"],"#dc3545") },
                        { 6, (_localizer["DHCPv4_Decline_6"],"#001f3f") },
                    }
                },
            };
        }

        private readonly Dictionary<DHCPv6PacketTypes, Dictionary<Int32, (String Name, String Color)>> _dhcpv6ResponseCodesMapper;
        private readonly Dictionary<DHCPv4MessagesTypes, Dictionary<Int32, (String Name, String Color)>> _dhcpv4ResponseCodesMapper;

        public  Dictionary<DHCPv6PacketTypes, Dictionary<Int32, (String Name, String Color)>> GetDHCPv6ResponseCodesMapper() => _dhcpv6ResponseCodesMapper;
        public  Dictionary<DHCPv4MessagesTypes, Dictionary<Int32, (String Name, String Color)>> GetDHCPv4ResponseCodesMapper() => _dhcpv4ResponseCodesMapper;


        public String GetErrorName(DHCPv6PacketTypes request, Int32 errorCode)
        {
            return _dhcpv6ResponseCodesMapper.ContainsKey(request) == true ?
            (_dhcpv6ResponseCodesMapper[request].ContainsKey(errorCode) == true ? _dhcpv6ResponseCodesMapper[request][errorCode].Name : errorCode.ToString()) : errorCode.ToString();
        }

        public String GetErrorName(DHCPv4MessagesTypes request, Int32 errorCode)
        {
            return _dhcpv4ResponseCodesMapper.ContainsKey(request) == true ?
            (_dhcpv4ResponseCodesMapper[request].ContainsKey(errorCode) == true ? _dhcpv4ResponseCodesMapper[request][errorCode].Name : errorCode.ToString()) : errorCode.ToString();
        }
    }
}

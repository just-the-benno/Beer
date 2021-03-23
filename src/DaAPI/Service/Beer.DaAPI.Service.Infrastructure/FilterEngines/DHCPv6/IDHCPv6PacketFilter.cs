using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.FilterEngines.DHCPv6
{
    public interface IDHCPv6PacketFilter : IDHCPPacketFilter<DHCPv6Packet,IPv6Address>
    {
    }
}

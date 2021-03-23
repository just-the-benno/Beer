using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public interface IDHCPv4PacketFilter : IDHCPPacketFilter<DHCPv4Packet, IPv4Address>
    {
    }
}

using Beer.DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public class UnkownDHCPv4RequestTypePaketFilter : IDHCPv4PacketFilter
    {
        public Task<bool> ShouldPacketBeFiltered(DHCPv4Packet packet)
        {
            if (packet.MessageType != DHCPv4MessagesTypes.Request)
            {
                return Task.FromResult(false);
            }

            try
            {
                packet.GetRequestType();
                return Task.FromResult(false);

            }
            catch (Exception)
            {
                return Task.FromResult(true);
            }
        }
    }
}

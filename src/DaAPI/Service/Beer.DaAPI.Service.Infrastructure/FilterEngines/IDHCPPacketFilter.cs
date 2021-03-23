using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.FilterEngines
{
    public interface IDHCPPacketFilter<TPacket, TAddress>
          where TPacket : DHCPPacket<TPacket, TAddress>
          where TAddress : IPAddress<TAddress>
    {
        Task<Boolean> ShouldPacketBeFiltered(TPacket packet);

    }
}

using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public abstract class DHCPv4ScopeResolverContainingOtherResolvers : 
        ScopeResolverContainingOtherResolvers<DHCPv4Packet, IPv4Address>
    {
    }
}

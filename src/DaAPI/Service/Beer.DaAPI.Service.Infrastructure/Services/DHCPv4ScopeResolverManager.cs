using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Infrastructure.Services
{
    public class DHCPv4ScopeResolverManager : ScopeResolverManagerBase<DHCPv4Packet, IPv4Address>
    {
        public DHCPv4ScopeResolverManager(
            ISerializer serializer,
            ILogger<DHCPv4ScopeResolverManager> logger) : base(serializer, logger)
        {
            AddOrUpdateScopeResolver(nameof(DHCPv4RelayAgentResolver), () => new DHCPv4RelayAgentResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv4RelayAgentSubnetResolver), () => new DHCPv4RelayAgentSubnetResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv4Option82Resolver), () => new DHCPv4Option82Resolver());
        }
    }
}

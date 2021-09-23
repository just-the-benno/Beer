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
            IDeviceService deviceService,
            ILogger<DHCPv4ScopeResolverManager> logger) : base(serializer, logger)
        {
            AddOrUpdateScopeResolver(nameof(DHCPv4PseudoResolver), () => new DHCPv4PseudoResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv4RelayAgentResolver), () => new DHCPv4RelayAgentResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv4RelayAgentSubnetResolver), () => new DHCPv4RelayAgentSubnetResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv4SimpleZyxelIESResolver), () => new DHCPv4SimpleZyxelIESResolver());
            AddOrUpdateScopeResolver(nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver), () => new DeviceBasedDHCPv4SimpleZyxelIESResolver(deviceService));
            AddOrUpdateScopeResolver(nameof(DHCPv4SimpleCiscoSGSeriesResolver), () => new DHCPv4SimpleCiscoSGSeriesResolver());
            AddOrUpdateScopeResolver(nameof(NonUniqueDeviceBasedDHCPv4SimpleCiscoSGSeriesResolver), () => new NonUniqueDeviceBasedDHCPv4SimpleCiscoSGSeriesResolver(deviceService));
            
            AddOrUpdateScopeResolver(nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver), () => new DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver(deviceService));

            AddOrUpdateScopeResolver(nameof(DHCPv4Option82Resolver), () => new DHCPv4Option82Resolver());

            AddOrUpdateScopeResolver(nameof(DHCPv4MacAddressResolver), () => new DHCPv4MacAddressResolver());
            AddOrUpdateScopeResolver(nameof(DeviceBasedDHCPv4MacAddressResolver), () => new DeviceBasedDHCPv4MacAddressResolver(deviceService));

        }
    }
}

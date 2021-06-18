using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv6Scopes
{
    public abstract class ManipulateDHCPv6ScopeCommandHandler : DHCPv6ScopeTriggerAwareCommandHandler
    {
        public ManipulateDHCPv6ScopeCommandHandler(IDHCPv6StorageEngine store,
           IServiceBus serviceBus,
           DHCPv6RootScope rootScope) : base(store, serviceBus, rootScope)
        {

        }

        protected static DynamicRenewTime GetDynamicRenewTime(DHCPv6DynamicRenewTimeRequest request) =>
           DynamicRenewTime.WithSpecificRange(request.Hours, request.Minutes, request.MinutesToRebound, request.MinutesToEndOfLife);


        protected static DHCPv6ScopeAddressProperties GetScopeAddressProperties(IScopeChangeCommand request) =>
             request.AddressProperties.DynamicRenewTime == null ?
           new DHCPv6ScopeAddressProperties
               (
                   IPv6Address.FromString(request.AddressProperties.Start),
                   IPv6Address.FromString(request.AddressProperties.End),
                   request.AddressProperties.ExcludedAddresses.Select(x => IPv6Address.FromString(x)),
                   request.AddressProperties.T1 == null ? null : DHCPv6TimeScale.FromDouble(request.AddressProperties.T1.Value),
                   request.AddressProperties.T2 == null ? null : DHCPv6TimeScale.FromDouble(request.AddressProperties.T2.Value),
                   preferredLifeTime: request.AddressProperties.PreferredLifeTime,
                   validLifeTime: request.AddressProperties.ValidLifeTime,
                   reuseAddressIfPossible: request.AddressProperties.ReuseAddressIfPossible,
                   addressAllocationStrategy: (Beer.DaAPI.Core.Scopes.ScopeAddressProperties<DHCPv6ScopeAddressProperties, IPv6Address>.AddressAllocationStrategies?)request.AddressProperties.AddressAllocationStrategy,
                   supportDirectUnicast: request.AddressProperties.SupportDirectUnicast,
                   acceptDecline: request.AddressProperties.AcceptDecline,
                   informsAreAllowd: request.AddressProperties.InformsAreAllowd,
                   rapitCommitEnabled: request.AddressProperties.RapitCommitEnabled,
                   prefixDelgationInfo: request.AddressProperties.PrefixDelgationInfo == null ? null :
                       DHCPv6PrefixDelgationInfo.FromValues(
                           IPv6Address.FromString(request.AddressProperties.PrefixDelgationInfo.Prefix),
                           new IPv6SubnetMaskIdentifier(request.AddressProperties.PrefixDelgationInfo.PrefixLength),
                           new IPv6SubnetMaskIdentifier(request.AddressProperties.PrefixDelgationInfo.AssingedPrefixLength))
               ) : new DHCPv6ScopeAddressProperties
               (
                   IPv6Address.FromString(request.AddressProperties.Start),
                   IPv6Address.FromString(request.AddressProperties.End),
                   request.AddressProperties.ExcludedAddresses.Select(x => IPv6Address.FromString(x)),
                   GetDynamicRenewTime(request.AddressProperties.DynamicRenewTime),
                   reuseAddressIfPossible: request.AddressProperties.ReuseAddressIfPossible,
                   addressAllocationStrategy: (Beer.DaAPI.Core.Scopes.ScopeAddressProperties<DHCPv6ScopeAddressProperties, IPv6Address>.AddressAllocationStrategies?)request.AddressProperties.AddressAllocationStrategy,
                   supportDirectUnicast: request.AddressProperties.SupportDirectUnicast,
                   acceptDecline: request.AddressProperties.AcceptDecline,
                   informsAreAllowd: request.AddressProperties.InformsAreAllowd,
                   rapitCommitEnabled: request.AddressProperties.RapitCommitEnabled,
                   prefixDelgationInfo: request.AddressProperties.PrefixDelgationInfo == null ? null :
                       DHCPv6PrefixDelgationInfo.FromValues(
                           IPv6Address.FromString(request.AddressProperties.PrefixDelgationInfo.Prefix),
                           new IPv6SubnetMaskIdentifier(request.AddressProperties.PrefixDelgationInfo.PrefixLength),
                           new IPv6SubnetMaskIdentifier(request.AddressProperties.PrefixDelgationInfo.AssingedPrefixLength)));

        protected static CreateScopeResolverInformation GetResolverInformation(IScopeChangeCommand request) =>
            new CreateScopeResolverInformation
            {
                PropertiesAndValues = request.Resolver.PropertiesAndValues,
                Typename = request.Resolver.Typename,
            };

        protected static DHCPv6ScopeProperties GetScopeProperties(IScopeChangeCommand request)
        {
            List<DHCPv6ScopeProperty> properties = new List<DHCPv6ScopeProperty>();
            List<ushort> optionsToRemove = new List<ushort>();

            foreach (var item in request.Properties ?? Array.Empty<DHCPv6ScopePropertyRequest>())
            {
                if (item.MarkAsRemovedInInheritance == true)
                {
                    optionsToRemove.Add(item.OptionCode);
                }
                else
                {
                    switch (item)
                    {
                        case DHCPv6AddressListScopePropertyRequest property:
                            properties.Add(new DHCPv6AddressListScopeProperty(item.OptionCode, property.Addresses.Select(x => IPv6Address.FromString(x)).ToList()));
                            break;
                        case DHCPv6NumericScopePropertyRequest property:
                            properties.Add(new DHCPv6NumericValueScopeProperty(item.OptionCode, property.Value, property.NumericType, item.Type));
                            break;
                        default:
                            break;
                    }
                }
            }

            var result = new DHCPv6ScopeProperties(properties);

            foreach (var item in optionsToRemove)
            {
                result.RemoveFromInheritance(item);
            }

            return result;
        }
    }
}

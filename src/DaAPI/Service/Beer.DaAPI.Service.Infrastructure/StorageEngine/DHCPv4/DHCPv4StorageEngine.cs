using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Notifications;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public class DHCPv4StorageEngine : DHCPStoreEngine<IDHCPv4EventStore, IDHCPv4ReadStore>, IDHCPv4StorageEngine
    {
        private static Guid _rootScopeid = Guid.Parse("12d26139-4efe-439c-83b6-ce013dbbe3d0");

        private class PseudoDHCPv4RootScope : PseudoDHCPRootScope
        {
            public PseudoDHCPv4RootScope() : base(_rootScopeid)
            {
            }
        }

        private class PseudoDHCPv4Scope : PseudoAggregateRootWithEvents
        {
            public PseudoDHCPv4Scope(Guid id) : base(id)
            {

            }
        }

        private class PseudoDHCPv4Lease : PseudoAggregateRootWithEvents
        {
            public PseudoDHCPv4Lease(Guid id) : base(id)
            {

            }
        }

        public DHCPv4StorageEngine(IServiceProvider provider) : base(
            provider,
            provider.GetRequiredService<IDHCPv4EventStore>(),
            provider.GetRequiredService<IDHCPv4ReadStore>()
            )
        {
        }

        public Task<IEnumerable<DHCPv4Listener>> GetDHCPv4Listener() => ReadStore.GetDHCPv4Listener();

        public Task<Boolean> LogInvalidDHCPv4Packet(DHCPv4Packet packet) => ReadStore.LogInvalidDHCPv4Packet(packet);
        public Task<Boolean> LogFilteredDHCPv4Packet(DHCPv4Packet packet, String filterName) => ReadStore.LogFilteredDHCPv4Packet(packet, filterName);


        public override async Task<bool> Save(AggregateRootWithEvents aggregateRoot)
        {
            if (aggregateRoot is DHCPv4RootScope == false)
            {
                return await base.Save(aggregateRoot);
            }

            PseudoDHCPv4RootScope pseudoRootScope = new();

            var events = aggregateRoot.GetChanges();

            List<AggregateRootWithEvents> aggregatesToSave = new();
            aggregatesToSave.Add(pseudoRootScope);

            HashSet<Guid> scopesToDelete = new();
            HashSet<Guid> leasesToDelete = new();

            foreach (var item in events)
            {
                switch (item)
                {
                    case DHCPv4ScopeAddedEvent e:
                        {
                            pseudoRootScope.AddScope(e.Instructions.Id);
                            var pseudoScope = new PseudoDHCPv4Scope(e.Instructions.Id);
                            pseudoScope.AddChange(e);
                            aggregatesToSave.Add(pseudoScope);
                        }
                        break;
                    case DHCPv4ScopeDeletedEvent e:
                        {
                            pseudoRootScope.RemoveScope(e.EntityId);
                            scopesToDelete.Add(e.EntityId);
                        }
                        break;
                    case EntityBasedDomainEvent e when
                        item is DHCPv4ScopePropertiesUpdatedEvent ||
                        item is DHCPv4ScopeNameUpdatedEvent ||
                        item is DHCPv4ScopeDescriptionUpdatedEvent ||
                        item is DHCPv4ScopeResolverUpdatedEvent ||
                        item is DHCPv4ScopeAddressPropertiesUpdatedEvent ||
                        item is DHCPv4ScopeParentUpdatedEvent ||
                        item is DHCPv4ScopeAddressesAreExhaustedEvent ||
                        item is DHCPv4ScopeSuspendedEvent ||
                        item is DHCPv4ScopeReactivedEvent:
                        {
                            var pseudoScope = new PseudoDHCPv4Scope(e.EntityId);
                            pseudoScope.AddChange(e);
                            aggregatesToSave.Add(pseudoScope);
                        }
                        break;
                    case DHCPv4LeaseCreatedEvent e:
                        {
                            pseudoRootScope.AddLease(e.EntityId);
                            var pseudoScope = new PseudoDHCPv4Lease(e.EntityId);
                            pseudoScope.AddChange(e);
                            aggregatesToSave.Add(pseudoScope);
                        }
                        break;
                    case DHCPv4LeaseRemovedEvent e:
                        {
                            pseudoRootScope.RemoveLease(e.EntityId);
                            leasesToDelete.Add(e.EntityId);
                        }
                        break;

                    case DHCPv4ScopeRelatedEvent e:
                        {
                            var pseudoLease = new PseudoDHCPv4Lease(e.EntityId);
                            pseudoLease.AddChange(e);
                            aggregatesToSave.Add(pseudoLease);
                        }
                        break;

                    default:
                        break;
                }
            }

            foreach (var item in aggregatesToSave)
            {
                await EventStore.Save(item, 20);
                if (item is PseudoDHCPv4Lease)
                {
                    var propertyResolver = Provider.GetService<IDHCPv6ServerPropertiesResolver>();

                    await EventStore.ApplyMetaValuesForStream<PseudoDHCPv4Lease>(
                        item.Id,
                        new EventStoreStreamMetaValues(EventStoreStreamMetaValues.DoNotTruncate, propertyResolver.GetLeaseLifeTime()));
                }
            }

            foreach (var item in scopesToDelete)
            {
                await EventStore.DeleteAggregateRoot<PseudoDHCPv4Scope>(item);
            }

            foreach (var item in leasesToDelete)
            {
                await EventStore.DeleteAggregateRoot<PseudoDHCPv4Lease>(item);
            }

            aggregateRoot.ClearChanges();

            Boolean projectResult = await ReadStore.Project(events);
            return projectResult;
        }

        public async Task<DHCPv4RootScope> GetRootScope()
        {
            DHCPv4RootScope rootScope = new DHCPv4RootScope(_rootScopeid, Provider.GetRequiredService<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(), Provider.GetRequiredService<ILoggerFactory>());

            PseudoDHCPv4RootScope pseudoRootScope = new PseudoDHCPv4RootScope();
            await EventStore.HydrateAggragate(pseudoRootScope);

            List<DomainEvent> eventsToApply = new();

            foreach (var scopeId in pseudoRootScope.ScopeIds)
            {
                var events = await EventStore.GetEvents<PseudoDHCPv4Scope>(scopeId, 100);
                eventsToApply.AddRange(events);
            }

            foreach (var leaseId in pseudoRootScope.LeaseIds)
            {
                var events = await EventStore.GetEvents<PseudoDHCPv4Lease>(leaseId, 100);
                eventsToApply.AddRange(events);
            }

            rootScope.Load(eventsToApply.OrderBy(x => x.Timestamp));
            return rootScope;
        }
    }
}

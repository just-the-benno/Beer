using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Notifications;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public class DHCPv6StorageEngine : DHCPStoreEngine<IDHCPv6EventStore, IDHCPv6ReadStore>, IDHCPv6StorageEngine
    {
        private static Guid _rootScopeid = Guid.Parse("87a59af9-5da3-4aa4-9709-9dbf15f6efb2");

        private class PseudoDHCPv6RootScope : PseudoDHCPRootScope
        {
            public PseudoDHCPv6RootScope() : base(_rootScopeid)
            {
            }
        }

        private class PseudoDHCPv6Scope : PseudoAggregateRootWithEvents
        {
            public PseudoDHCPv6Scope(Guid id) : base(id)
            {

            }
        }

        private class PseudoDHCPv6Lease : PseudoAggregateRootWithEvents
        {
            public PseudoDHCPv6Lease(Guid id) : base(id)
            {

            }
        }

        public DHCPv6StorageEngine(IServiceProvider provider) : base(
            provider,
            provider.GetRequiredService<IDHCPv6EventStore>(),
            provider.GetRequiredService<IDHCPv6ReadStore>()
            )
        {
        }

        public Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener() => ReadStore.GetDHCPv6Listener();

        public override async Task<bool> Save(AggregateRootWithEvents aggregateRoot)
        {
            if (aggregateRoot is DHCPv6RootScope == false)
            {
                return await base.Save(aggregateRoot);
            }

            PseudoDHCPv6RootScope pseudoRootScope = new();

            var events = aggregateRoot.GetChanges();

            List<AggregateRootWithEvents> aggregatesToSave = new();
            aggregatesToSave.Add(pseudoRootScope);

            HashSet<Guid> scopesToDelete = new();
            HashSet<Guid> leasesToDelete = new();

            foreach (var item in events)
            {
                switch (item)
                {
                    case DHCPv6ScopeAddedEvent e:
                        {
                            pseudoRootScope.AddScope(e.Instructions.Id);
                            var pseudoScope = new PseudoDHCPv6Scope(e.Instructions.Id);
                            pseudoScope.AddChange(e);
                            aggregatesToSave.Add(pseudoScope);
                        }
                        break;
                    case DHCPv6ScopeDeletedEvent e:
                        {
                            pseudoRootScope.RemoveScope(e.EntityId);
                            scopesToDelete.Add(e.EntityId);
                        }
                        break;
                    case EntityBasedDomainEvent e when
                        item is DHCPv6ScopePropertiesUpdatedEvent ||
                        item is DHCPv6ScopeNameUpdatedEvent ||
                        item is DHCPv6ScopeDescriptionUpdatedEvent ||
                        item is DHCPv6ScopeResolverUpdatedEvent ||
                        item is DHCPv6ScopeAddressPropertiesUpdatedEvent ||
                        item is DHCPv6ScopeParentUpdatedEvent ||
                        item is DHCPv6ScopeAddressesAreExhaustedEvent ||
                        item is DHCPv6ScopeSuspendedEvent ||
                        item is DHCPv6ScopeReactivedEvent:
                        {
                            var pseudoScope = new PseudoDHCPv6Scope(e.EntityId);
                            pseudoScope.AddChange(e);
                            aggregatesToSave.Add(pseudoScope);
                        }
                        break;
                    case DHCPv6LeaseCreatedEvent e:
                        {
                            pseudoRootScope.AddLease(e.EntityId);
                            var pseudoScope = new PseudoDHCPv6Lease(e.EntityId);
                            pseudoScope.AddChange(e);
                            aggregatesToSave.Add(pseudoScope);
                        }
                        break;
                    case DHCPv6LeaseRemovedEvent e:
                        {
                            pseudoRootScope.RemoveLease(e.EntityId);
                            leasesToDelete.Add(e.EntityId);
                        }
                        break;

                    case DHCPv6ScopeRelatedEvent e:
                        {
                            var pseudoLease = new PseudoDHCPv6Lease(e.EntityId);
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
                await EventStore.Save(item);
                if (item is PseudoDHCPv6Lease)
                {
                    var propertyResolver = Provider.GetService<IDHCPv6ServerPropertiesResolver>();

                    await EventStore.ApplyMetaValuesForStream<PseudoDHCPv6Lease>(
                        item.Id,
                        new EventStoreStreamMetaValues(EventStoreStreamMetaValues.DoNotTruncate, propertyResolver.GetLeaseLifeTime()));
                }
            }

            foreach (var item in scopesToDelete)
            {
                await EventStore.DeleteAggregateRoot<PseudoDHCPv6Scope>(item);
            }

            foreach (var item in leasesToDelete)
            {
                await EventStore.DeleteAggregateRoot<PseudoDHCPv6Lease>(item);
            }

            aggregateRoot.ClearChanges();

            Boolean projectResult = await ReadStore.Project(events);
            return projectResult;
        }

        public async Task<DHCPv6RootScope> GetRootScope()
        {
            DHCPv6RootScope rootScope = new DHCPv6RootScope(_rootScopeid, Provider.GetRequiredService<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), Provider.GetRequiredService<ILoggerFactory>());

            PseudoDHCPv6RootScope pseudoRootScope = new PseudoDHCPv6RootScope();
            await EventStore.HydrateAggragate(pseudoRootScope);

            List<DomainEvent> eventsToApply = new();

            foreach (var scopeId in pseudoRootScope.ScopeIds)
            {
                var events = await EventStore.GetEvents<PseudoDHCPv6Scope>(scopeId);
                eventsToApply.AddRange(events);
            }

            foreach (var leaseId in pseudoRootScope.LeaseIds)
            {
                var events = await EventStore.GetEvents<PseudoDHCPv6Lease>(leaseId);
                eventsToApply.AddRange(events);
            }

            rootScope.Load(eventsToApply.OrderBy(x => x.Timestamp));
            return rootScope;
        }

        public Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet) => ReadStore.LogInvalidDHCPv6Packet(packet);
        public Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName) => ReadStore.LogFilteredDHCPv6Packet(packet, filterName);

        public Task DeleteLeaseRelatedEventsOlderThan(DateTime leaseThreshold)
        {
            throw new NotImplementedException();
        }

        public Task DeletePacketHandledEventsOlderThan(DateTime handledEventThreshold)
        {
            throw new NotImplementedException();
        }

        public Task DeletePacketHandledEventMoreThan(uint amount)
        {
            throw new NotImplementedException();
        }
    }
}

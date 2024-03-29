﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Notifications;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    public abstract class DHCPStoreEngine<TEventStore, TReadStore>
        where TEventStore : IEventStore
        where TReadStore : IReadStore
    {
        protected TEventStore EventStore { get; private init; }
        protected TReadStore ReadStore { get; private init; }
        protected IServiceProvider Provider { get; private init; }

        public DHCPStoreEngine(IServiceProvider provider, TEventStore writeStore, TReadStore readStore)
        {
            Provider = provider;
            EventStore = writeStore;
            ReadStore = readStore;
        }

        public virtual async Task<Boolean> Save(AggregateRootWithEvents aggregateRoot)
        {
            var events = aggregateRoot.GetChanges();
            await EventStore.Save(aggregateRoot, 20);

            aggregateRoot.ClearChanges();

            Boolean projectResult = await ReadStore.Project(events);
            if (projectResult == false)
            {
                return false;
            }

            return true;
        }

        public Task<Boolean> CheckIfAggrerootExists<T>(Guid id) where T : AggregateRootWithEvents, new()
        {
            return EventStore.CheckIfAggrerootExists<T>(id);
        }

        public Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new()
        {
            return EventStore.GetAggregateRoot<T>(id);
        }

        public async Task<IEnumerable<NotificationPipeline>> GetAllNotificationPipeleines()
        {
            var notificationPipelineIds = await ReadStore.GetAllNotificationPipelineIds();

            List<NotificationPipeline> pipelines = new List<NotificationPipeline>();
            foreach (var item in notificationPipelineIds)
            {
                NotificationPipeline pipeline = new NotificationPipeline(item,
                    Provider.GetService<INotificationConditionFactory>(),
                    Provider.GetService<INotificationActorFactory>(),
                    Provider.GetService<ILogger<NotificationPipeline>>());

                await EventStore.HydrateAggragate(pipeline);
                pipelines.Add(pipeline);
            }

            return pipelines;
        }

        public Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents
        {
            return EventStore.DeleteAggregateRoot<T>(id);
        }

        public async Task<Boolean> DeleteAggregateRoot<T>(T element) where T : AggregateRootWithEvents
        {
            element.PrepareForDelete();

            var lastChanges = element.GetChanges();
            Boolean hasLastChanges = lastChanges.Any();
            if (hasLastChanges == true)
            {
                await EventStore.Save(element, 20);
            }

            await EventStore.DeleteAggregateRoot<T>(element.Id);
            if (hasLastChanges == true)
            {
                await ReadStore.Project(lastChanges);
            }

            return true;
        }

        public async Task DeleteLeaseRelatedEventsOlderThan(DateTime leaseThreshold) => await ReadStore.DeleteLeaseRelatedEventsOlderThan(leaseThreshold);
        public async Task DeletePacketHandledEventsOlderThan(DateTime handledEventThreshold) => await ReadStore.DeletePacketHandledEventsOlderThan(handledEventThreshold);
        public async Task DeletePacketHandledEventMoreThan(uint amount) => await ReadStore.DeletePacketHandledEventMoreThan(amount);

    }
}

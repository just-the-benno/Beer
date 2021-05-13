using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Service.Infrastructure.StorageEngine;
using Beer.TestHelper;
using EventStore.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Beer.DaAPI.Service.IntegrationTests.StorageEngine
{
    public class EventStoreBasedStoreTester
    {
        public class PseudoAggregateRootCreatedEvent : DomainEvent
        {
            public String InitialName { get; set; }
            public Guid Id { get; set; }
        }

        public class PseudoAggregateRootNameChangedEvent : DomainEvent
        {
            public String SecondName { get; set; }
        }

        public class PseudoAggregateRoot : AggregateRootWithEvents
        {
            public String InitialName { get; private set; }
            public String SecondName { get; private set; }

            public PseudoAggregateRoot(Guid id) : base(id)
            {

            }

            public static PseudoAggregateRoot Create(Guid id, String initialName)
            {
                var root = new PseudoAggregateRoot(id);
                root.Apply(new PseudoAggregateRootCreatedEvent { Id = id, InitialName = initialName });
                return root;
            }

            public void ChangeName(String name) => base.Apply(new PseudoAggregateRootNameChangedEvent { SecondName = name });

            protected override void When(DomainEvent domainEvent)
            {
                switch (domainEvent)
                {
                    case PseudoAggregateRootCreatedEvent e:
                        InitialName = e.InitialName;
                        break;
                    case PseudoAggregateRootNameChangedEvent e:
                        SecondName = e.SecondName;
                        break;
                    default:
                        break;
                }
            }
        }

        [Fact]
        public async Task LifeCylce()
        {
            Random random = new Random();
            String firstname = random.GetAlphanumericString();
            String secondName = random.GetAlphanumericString();
            Guid id = random.NextGuid();

            PseudoAggregateRoot aggregateRoot = PseudoAggregateRoot.Create(id, firstname);
            aggregateRoot.ChangeName(secondName);

            var settings = EventStoreClientSettings.Create("esdb://127.0.0.1:2113?tls=false");
            var client = new EventStoreClient(settings);
            String prefix = random.GetAlphanumericString();
            EventStoreBasedStore store = new EventStoreBasedStore(new EventStoreBasedStoreConnenctionOptions(client, prefix));

            try
            {


                Boolean firstExistResult = await store.CheckIfAggrerootExists<PseudoAggregateRoot>(id);
                Assert.False(firstExistResult);

                Boolean saveResult = await store.Save(aggregateRoot);
                Assert.True(saveResult);

                Boolean secondExistResult = await store.CheckIfAggrerootExists<PseudoAggregateRoot>(id);
                Assert.True(secondExistResult);

                var hydratedVersion = new PseudoAggregateRoot(id);
                await store.HydrateAggragate(hydratedVersion);

                Assert.Equal(firstname, hydratedVersion.InitialName);
                Assert.Equal(secondName, hydratedVersion.SecondName);

                Boolean deleteResult = await store.DeleteAggregateRoot<PseudoAggregateRoot>(id);
                Assert.True(deleteResult);

                Boolean thirdExistResult = await store.CheckIfAggrerootExists<PseudoAggregateRoot>(id);
                Assert.False(thirdExistResult);
            }
            finally
            {
                await EventStoreClientDisposer.CleanUp(prefix, settings);
            }
        }

        [Fact]
        public async Task SaveAndGetEventsInChunks()
        {
            Random random = new Random();

            Guid id = random.NextGuid();
            PseudoAggregateRoot aggregateRoot = PseudoAggregateRoot.Create(id, "my name");

            Int32 amount = 10_000;

            for (int i = 0; i < amount; i++)
            {
                aggregateRoot.ChangeName($"iteration {i}");
            }

            var settings = EventStoreClientSettings.Create("esdb://127.0.0.1:2113?tls=false");
            var client = new EventStoreClient(settings);

            String prefix = random.GetAlphanumericString();

            EventStoreBasedStore store = new(new EventStoreBasedStoreConnenctionOptions(client, prefix));

            try
            {
                await store.Save(aggregateRoot);

                var events = await store.GetEvents<PseudoAggregateRoot>(id, 10);
                Assert.Equal(amount + 1, events.Count());
                Assert.Equal($"iteration {amount - 1}", ((PseudoAggregateRootNameChangedEvent)events.Last()).SecondName);

            }
            finally
            {
                await EventStoreClientDisposer.CleanUp(prefix, settings);
            }
        }
    }
}

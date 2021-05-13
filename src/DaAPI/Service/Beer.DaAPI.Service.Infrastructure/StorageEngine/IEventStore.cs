using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    public record EventStoreStreamMetaValues(Int32 AmountOfItemsPerStream, TimeSpan MaxAgeOfEvents)
    {
        public static Int32 DoNotTruncate => -1;
    }

    public interface IEventStore
    {
        Task<Boolean> Save(AggregateRootWithEvents root, Int32 eventsPerRequest);
        Task<Boolean> CheckIfAggrerootExists<T>(Guid id) where T : AggregateRootWithEvents;
        Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new();
        Task HydrateAggragate<T>(T instance) where T : AggregateRootWithEvents;
        Task<IEnumerable<DomainEvent>> GetEvents<T>(Guid id, Int32 eventsPerRequest) where T : AggregateRootWithEvents;
        Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents;
        Task<Boolean> ApplyMetaValuesForStream<T>(Guid id, EventStoreStreamMetaValues values);
    }
}

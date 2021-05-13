using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Infrastructure.StorageEngine;
using Beer.DaAPI.Infrastructure.StorageEngine.Converters;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using EventStore.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine
{
    public record EventStoreBasedStoreConnenctionOptions(EventStoreClient Client, String EventPrefix);

    public class EventStoreBasedStore : IDHCPv6EventStore, IDHCPv4EventStore, IDisposable
    {
        private class EventMeta
        {
            public DateTime Timestamp { get; set; }
            public String BodyType { get; set; }
        }

        private readonly EventStoreClient _client;
        private readonly String _eventPrefix;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public EventStoreBasedStore(EventStoreBasedStoreConnenctionOptions options)
        {
            this._client = options.Client;
            this._eventPrefix = options.EventPrefix;
            _jsonSerializerSettings = new JsonSerializerSettings();
            _jsonSerializerSettings.Converters.Add(new DUIDJsonConverter());

            _jsonSerializerSettings.Converters.Add(new IPv6AddressJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6PacketJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6ScopeAddressPropertiesConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6PrefixDelgationInfoJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6ScopePropertyJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6ScopePropertiesJsonConverter());
            _jsonSerializerSettings.Converters.Add(new IPv6HeaderInformationJsonConverter());

            _jsonSerializerSettings.Converters.Add(new IPv4AddressJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv4PacketJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv4ScopeAddressPropertiesConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv4ScopePropertyJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv4ScopePropertiesJsonConverter());
            _jsonSerializerSettings.Converters.Add(new IPv4HeaderInformationJsonConverter());
        }

        private String GetStreamTypeIdentifer(Type type) => $"{type.Name}";
        private String GetStreamId(Type type, Guid id) => $"{_eventPrefix}-{GetStreamTypeIdentifer(type)}-{id}";
        private String GetStreamId<T>(Guid id) => GetStreamId(typeof(T), id);

        public async Task<bool> CheckIfAggrerootExists<T>(Guid id) where T : AggregateRootWithEvents
        {
            String streamId = GetStreamId<T>(id);

            var readResult = _client.ReadStreamAsync(Direction.Forwards, streamId, StreamPosition.Start, 1);
            return (await readResult.ReadState) == ReadState.Ok;
        }

        public async Task<bool> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents
        {
            String streamId = GetStreamId<T>(id);
            await _client.SoftDeleteAsync(streamId, StreamState.Any);
            return true;
        }

        public async Task<Boolean> Save(AggregateRootWithEvents aggregateRoot, Int32 eventsPerRequest = 100)
        {
            var events = aggregateRoot.GetChanges();
            String streamName = GetStreamId(aggregateRoot.GetType(), aggregateRoot.Id);

            var encoding = new UTF8Encoding();

            IEnumerable<EventData> data = events.Select(x => new EventData(
               Uuid.NewUuid(),
               x.GetType().Name,
                 encoding.GetBytes(JsonConvert.SerializeObject(x, _jsonSerializerSettings)),
                 System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new EventMeta
                 {
                     BodyType = x.GetType().AssemblyQualifiedName
                 })));

            Int32 eventPosition = 0;
            while(true)
            {
                var dataToSend = data.Skip(eventPosition).Take(eventsPerRequest).ToArray();
                await _client.AppendToStreamAsync(streamName, StreamState.Any, dataToSend);
                if(dataToSend.Length != eventsPerRequest)
                {
                    break;
                }

                eventPosition += eventsPerRequest;
            }

            return true;
        }

        public async Task<IEnumerable<DomainEvent>> GetEvents<T>(Guid id, Int32 eventsPerRequest = 100) where T : AggregateRootWithEvents
        {
            String streamName = GetStreamId<T>(id);
            var encoding = new UTF8Encoding();

            var firstEvent = _client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, 1);
            var state = await firstEvent.ReadState;

            if (state == ReadState.StreamNotFound)
            {
                return Array.Empty<DomainEvent>();
            }

            List<DomainEvent> domainEvents = new();
            Int32 streamPosition = 0;
            while (true)
            {
                var partialEvents = _client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.FromInt64(streamPosition), eventsPerRequest);
                var items = await partialEvents.ToListAsync(); 
                if (items.Count == 0)
                {
                    break;
                }

                foreach (var item in items)
                {
                    EventMeta meta = System.Text.Json.JsonSerializer.Deserialize<EventMeta>(new ReadOnlySpan<Byte>(item.Event.Metadata.ToArray()));

                    String content = encoding.GetString(item.Event.Data.ToArray());

                    var domainEvent = (DomainEvent)JsonConvert.DeserializeObject(content, Type.GetType(meta.BodyType), _jsonSerializerSettings);
                    domainEvents.Add(domainEvent);
                }

                streamPosition += eventsPerRequest;
            }

            return domainEvents;
        }

        public async Task HydrateAggragate<T>(T instance) where T : AggregateRootWithEvents
        {
            var events = await GetEvents<T>(instance.Id);
            instance.Load(events);
        }

        public async Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new()
        {
            T instance = new();
            instance.SetId(id);
            await HydrateAggragate(instance);
            return instance;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task<Boolean> ApplyMetaValuesForStream<T>(Guid id, EventStoreStreamMetaValues values)
        {
            String streamName = GetStreamId<T>(id);

            await _client.SetStreamMetadataAsync(streamName, StreamState.Any,
                new StreamMetadata(
                    maxCount: values.AmountOfItemsPerStream <= 0 ? new Int32?() : values.AmountOfItemsPerStream,
                    maxAge: values.MaxAgeOfEvents.Ticks < 0 ? new TimeSpan?() : values.MaxAgeOfEvents
                    ));

            return true;
        }
    }
}

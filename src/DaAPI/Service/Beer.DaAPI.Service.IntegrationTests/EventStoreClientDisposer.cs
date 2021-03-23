using EventStore.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.IntegrationTests
{
    public static class EventStoreClientDisposer
    {
        public static async Task CleanUp(String prefix, EventStoreClientSettings settings)
        {
            if (settings == null)
            {
                settings = EventStoreClientSettings.Create("esdb://127.0.0.1:2113?tls=false");
            }

            var client = new EventStoreClient(settings);

            var result = client.ReadAllAsync(Direction.Backwards, Position.End);
            HashSet<String> streamsToDelete = new HashSet<string>(await result.Where(x => x.OriginalStreamId.StartsWith($"{prefix}-")).Select(x => x.OriginalStreamId).ToListAsync());

            foreach (var item in streamsToDelete)
            {
                await client.TombstoneAsync(item, StreamState.Any);
            }
        }
    }
}

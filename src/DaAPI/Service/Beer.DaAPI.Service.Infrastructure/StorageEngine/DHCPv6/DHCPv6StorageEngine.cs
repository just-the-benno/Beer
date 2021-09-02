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
        private static Guid _rootScopeid = Guid.Parse("87a59af9-5da3-4aa4-9709-9dbf15f6efb3");

        public DHCPv6StorageEngine(IServiceProvider provider) : base(
            provider,
            provider.GetRequiredService<IDHCPv6EventStore>(),
            provider.GetRequiredService<IDHCPv6ReadStore>()
            )
        {
        }

        public Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener() => ReadStore.GetDHCPv6Listener();

        public async Task<DHCPv6RootScope> GetRootScope()
        {
            DHCPv6RootScope rootScope = new DHCPv6RootScope(_rootScopeid, Provider.GetRequiredService<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), Provider.GetRequiredService<ILoggerFactory>());

            await EventStore.HydrateAggragate(rootScope);

            IDictionary<Guid, IEnumerable<DHCPv6LeaseCreatedEvent>> currentLeases = new Dictionary<Guid, IEnumerable<DHCPv6LeaseCreatedEvent>>();
            try
            {
                currentLeases = await ReadStore.GetLatestDHCPv6LeasesForHydration();
            }
            catch (Exception)
            {

            }

            DateTime now = DateTime.Today;

            foreach (var item in currentLeases)
            {
                List<DomainEvent> events = new();

                foreach (var createEvent in item.Value)
                {
                    if (createEvent.ValidUntil < now || rootScope.GetScopeById(createEvent.ScopeId) == null)
                    {
                        continue;
                    }

                    var activateEvent = new DHCPv6LeaseActivatedEvent
                    {
                        ScopeId = createEvent.ScopeId,
                        EntityId = createEvent.EntityId,
                        Timestamp = createEvent.Timestamp,
                    };

                    events.Add(createEvent);
                    events.Add(activateEvent);
                }

                try
                {
                    rootScope.Load(events);
                }
                catch (Exception)
                {
                }
            }

            return rootScope;
        }

        public Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet) => ReadStore.LogInvalidDHCPv6Packet(packet);
        public Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName) => ReadStore.LogFilteredDHCPv6Packet(packet, filterName);

    }
}

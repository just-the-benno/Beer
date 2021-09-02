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
        private static Guid _rootScopeid = Guid.Parse("12d26139-4efe-439c-83b6-ce013dbbe3d1");

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

        public async Task<DHCPv4RootScope> GetRootScope()
        {
            DHCPv4RootScope rootScope = new DHCPv4RootScope(_rootScopeid, Provider.GetRequiredService<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(), Provider.GetRequiredService<ILoggerFactory>());
            await EventStore.HydrateAggragate(rootScope);

            IDictionary<Guid, IEnumerable<DHCPv4LeaseCreatedEvent>> currentLeases = new Dictionary<Guid, IEnumerable<DHCPv4LeaseCreatedEvent>>();
            try
            {
                currentLeases = await ReadStore.GetLatestDHCPv4LeasesForHydration();
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

                    var activateEvent = new DHCPv4LeaseActivatedEvent
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
    }
}


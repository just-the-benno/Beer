using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Notifications;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Service.Infrastructure.StorageEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public interface IDHCPv6StorageEngine : IDHCPStoreEngine<DHCPv6RootScope>
    {
        Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener();
        Task<IEnumerable<NotificationPipeline>> GetAllNotificationPipeleines();
        Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents;
        Task<Boolean> DeleteAggregateRoot<T>(T instance) where T : AggregateRootWithEvents;

        Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet);
        Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName);
        
        Task DeleteLeaseRelatedEventsOlderThan(DateTime leaseThreshold);
        Task DeletePacketHandledEventsOlderThan(DateTime handledEventThreshold);
        Task DeletePacketHandledEventMoreThan(UInt32 amount);
    }
}

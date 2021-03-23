using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Service.Infrastructure.StorageEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public interface IDHCPv4StorageEngine : IDHCPStoreEngine<DHCPv4RootScope>
    {
        Task<IEnumerable<DHCPv4Listener>> GetDHCPv4Listener();
        Task<Boolean> LogInvalidDHCPv4Packet(DHCPv4Packet packet);
        Task<Boolean> LogFilteredDHCPv4Packet(DHCPv4Packet packet, string filterName);

        Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents;
    }
}

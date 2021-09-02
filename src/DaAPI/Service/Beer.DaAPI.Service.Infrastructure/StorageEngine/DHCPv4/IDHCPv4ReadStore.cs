using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Infrastructure.Services;
using Beer.DaAPI.Shared.Helper;
using Beer.DaAPI.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static Beer.DaAPI.Shared.Requests.StatisticsControllerRequests.V1;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public interface IDHCPv4ReadStore : IReadStore
    {
        Task<IEnumerable<DHCPv4Listener>> GetDHCPv4Listener();

        Task<StatisticsControllerResponses.V1.DashboardResponse> GetDashboardOverview();
        Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv4PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, IDictionary<DHCPv4MessagesTypes, Int32>>> GetIncomingDHCPv4PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetFileredDHCPv4Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetErrorDHCPv4Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetActiveDHCPv4Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPv4DHCPv4MessagesTypes(DateTime? start, DateTime? end, DHCPv4MessagesTypes type);
        Task<IEnumerable<StatisticsControllerResponses.V1.DHCPv4PacketHandledEntry>> GetHandledDHCPv4PacketByScopeId(Guid scopeId, Int32 amount);

        Task<Boolean> LogFilteredDHCPv4Packet(DHCPv4Packet packet, String filterName);
        Task<Boolean> LogInvalidDHCPv4Packet(DHCPv4Packet packet);

        IEnumerable<Device> GetAllDevices();
        Task<IDictionary<Guid, IEnumerable<DHCPv4LeaseCreatedEvent>>> GetLatestDHCPv4LeasesForHydration();
        Task<FilteredResult<CommenResponses.V1.LeaseEventOverview>> GetDHCPv4LeaseEvents(DateTime? startTime, DateTime? endDate, string ipAddress, IEnumerable<Guid> scopeIds, int start, int amount);
    }
}

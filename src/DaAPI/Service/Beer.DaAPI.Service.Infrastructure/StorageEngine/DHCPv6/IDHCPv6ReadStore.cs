using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.Services;
using Beer.DaAPI.Shared.Commands;
using Beer.DaAPI.Shared.Helper;
using Beer.DaAPI.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.StatisticsControllerRequests.V1;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public interface IDHCPv6ReadStore : IReadStore
    {
        Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener();
        Task<DHCPv6ServerProperties> GetServerProperties();

        Task<StatisticsControllerResponses.V1.DashboardResponse> GetDashboardOverview();
        Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv6PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>>> GetIncomingDHCPv6PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetFileredDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetErrorDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetActiveDHCPv6Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<FilteredResult<PacketMonitorResponses.V1.DHCPv4PacketOverview>> GetDHCPv4Packet(PacketMonitorRequest.V1.DHCPv4PacketFilter filter);
        Task<FilteredResult<PacketMonitorResponses.V1.DHCPv6PacketOverview>> GetDHCPv6Packet(PacketMonitorRequest.V1.DHCPv6PacketFilter filter);

        Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPV6RequestType(DateTime? start, DateTime? end, DHCPv6PacketTypes type);
        Task<IDictionary<Guid, IEnumerable<DHCPv6LeaseEvents.DHCPv6LeaseCreatedEvent>>> GetLatestDHCPv6LeasesForHydration();
        Task<IEnumerable<StatisticsControllerResponses.V1.DHCPv6PacketHandledEntry>> GetHandledDHCPv6PacketByScopeId(Guid scopeId, Int32 amount);
        
        Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet);
        Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName);

        IEnumerable<Device> GetAllDevices();
        Task<PacketMonitorResponses.V1.PacketInfo> GetDHCPv6PacketRequestDataById(Guid packetEnrtyId);
        Task<PacketMonitorResponses.V1.PacketInfo> GetDHCPv6PacketResponseDataById(Guid packetEnrtyId);
        Task<PacketMonitorResponses.V1.PacketInfo> GetDHCPv4PacketRequestDataById(Guid packetEnrtyId);
        Task<PacketMonitorResponses.V1.PacketInfo> GetDHCPv4PacketResponseDataById(Guid packetEnrtyId);
        Task<FilteredResult<CommenResponses.V1.LeaseEventOverview>> GetDHCPv6LeaseEvents(DateTime? startDate, DateTime? endDate, string ipAddress, IEnumerable<Guid> scopeIds, int start, int amount);
        Task<IDictionary<PacketMonitorResponses.V1.PacketStatisticTimePeriod,PacketMonitorResponses.V1.IncomingAndOutgoingPacketStatisticItem>> GetIncomingAndOutgoingPacketAmount(Guid scopeId, DateTime referenceTime);
        Task<IEnumerable<DHCPv6LeasesResponses.V1.DHCPv6LeaseOverview>> GetDHCPv6LeasesOverview(IEnumerable<Guid> scopeIds, DateTime dateTime);
    }
}

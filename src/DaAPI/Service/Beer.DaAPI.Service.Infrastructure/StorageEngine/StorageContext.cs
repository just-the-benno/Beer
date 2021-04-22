using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.Helper;
using Beer.DaAPI.Infrastructure.StorageEngine.Converters;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Listeners.DHCPListenerEvents;
using static Beer.DaAPI.Core.Notifications.NotificationsEvent.V1;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;
using static Beer.DaAPI.Shared.Requests.StatisticsControllerRequests.V1;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    public class StorageContext : DbContext, IDHCPv6ReadStore, IDHCPv4ReadStore
    {
        #region Fields

        #endregion

        #region Sets

        public DbSet<HelperEntry> Helpers { get; set; }
        public DbSet<DHCPv6InterfaceDataModel> DHCPv6Interfaces { get; set; }
        public DbSet<DHCPv6PacketHandledEntryDataModel> DHCPv6PacketEntries { get; set; }
        public DbSet<DHCPv6LeaseEntryDataModel> DHCPv6LeaseEntries { get; set; }

        public DbSet<DHCPv4InterfaceDataModel> DHCPv4Interfaces { get; set; }
        public DbSet<DHCPv4PacketHandledEntryDataModel> DHCPv4PacketEntries { get; set; }
        public DbSet<DHCPv4LeaseEntryDataModel> DHCPv4LeaseEntries { get; set; }

        public DbSet<NotificationPipelineOverviewEntry> NotificationPipelines { get; set; }

        #endregion

        public StorageContext(DbContextOptions<StorageContext> options) : base(options)
        {

        }

        private async Task<Boolean> SaveChangesAsyncInternal() => await SaveChangesAsync() > 0;


        public async Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener()
        {
            var result = await DHCPv6Interfaces.AsQueryable()
            .OrderBy(x => x.Name).ToListAsync();

            List<DHCPv6Listener> listeners = new List<DHCPv6Listener>(result.Count);
            foreach (var item in result)
            {
                DHCPv6Listener listener = new DHCPv6Listener();
                listener.Load(new DomainEvent[]
                {
                    new DHCPv6ListenerCreatedEvent {
                     Id = item.Id,
                     Name = item.Name,
                     InterfaceId = item.InterfaceId,
                     Address = item.IPv6Address,
                    }
                });

                listeners.Add(listener);
            }

            return listeners;
        }

        public async Task<IEnumerable<DHCPv4Listener>> GetDHCPv4Listener()
        {
            var result = await DHCPv4Interfaces.AsQueryable()
            .OrderBy(x => x.Name).ToListAsync();

            List<DHCPv4Listener> listeners = new List<DHCPv4Listener>(result.Count);
            foreach (var item in result)
            {
                var listener = new DHCPv4Listener();
                listener.Load(new DomainEvent[]
                {
                        new DHCPv4ListenerCreatedEvent {
                         Id = item.Id,
                         Name = item.Name,
                         InterfaceId = item.InterfaceId,
                         Address = item.IPv4Address,
                        }
                });

                listeners.Add(listener);
            }

            return listeners;
        }

        private async Task<Boolean?> ProjectDHCPv6PacketAndLeaseRelatedEvents(DomainEvent @event)
        {
            Boolean? hasChanges = new Boolean?();
            switch (@event)
            {

                case DHCPv6PacketHandledEvent e:
                    DHCPv6PacketHandledEntryDataModel entry = new DHCPv6PacketHandledEntryDataModel
                    {
                        HandledSuccessfully = e.WasSuccessfullHandled,
                        ErrorCode = e.ErrorCode,
                        Id = Guid.NewGuid(),
                        ScopeId = e.ScopeId,
                        Timestamp = e.Timestamp,

                        RequestSize = e.Request.GetSize(),
                        RequestType = e.Request.GetInnerPacket().PacketType,
                        RequestSource = e.Request.Header.Source.ToString(),
                        RequestDestination = e.Request.Header.Destionation.ToString(),
                        RequestStream = e.Request.GetAsStream(),
                    };

                    if (e.Response != null)
                    {
                        entry.ResponseSize = e.Response.GetSize();
                        entry.ResponseType = e.Response.GetInnerPacket().PacketType;
                        entry.ResponseSource = e.Response.Header.Source.ToString();
                        entry.ResponseDestination = e.Response.Header.Destionation.ToString();
                        entry.ResponseStream = e.Response.GetAsStream();
                    }

                    entry.SetTimestampDates();

                    DHCPv6PacketEntries.Add(entry);
                    hasChanges = true;
                    break;

                case DHCPv6LeaseCreatedEvent e:
                    {
                        DHCPv6LeaseEntryDataModel leaseEntry = new DHCPv6LeaseEntryDataModel
                        {
                            Id = Guid.NewGuid(),
                            Address = e.Address.ToString(),
                            Start = e.StartedAt,
                            End = e.ValidUntil,
                            EndOfRenewalTime = e.StartedAt + e.RenewalTime,
                            EndOfPreferredLifetime = e.StartedAt + e.PreferredLifetime,
                            LeaseId = e.EntityId,
                            ScopeId = e.ScopeId,
                            Prefix = e.HasPrefixDelegation == true ? e.DelegatedNetworkAddress.ToString() : null,
                            PrefixLength = e.HasPrefixDelegation == true ? e.PrefixLength : (Byte)0,
                            Timestamp = e.Timestamp,
                        };

                        DHCPv6LeaseEntries.Add(leaseEntry);
                        hasChanges = true;
                    }
                    break;

                case DHCPv6LeaseExpiredEvent e:
                    hasChanges = await UpdateEndToDHCPv6LeaseEntry(e, ReasonToEndLease.Expired);
                    break;

                case DHCPv6LeaseActivatedEvent e:
                    hasChanges = await UpdateLastestDHCPv6LeaseEntry(e, (leaseEntry) =>
                    {
                        leaseEntry.IsActive = true;
                    });
                    break;

                case DHCPv6LeasePrefixAddedEvent e:
                    hasChanges = await UpdateLastestDHCPv6LeaseEntry(e, (leaseEntry) =>
                    {
                        leaseEntry.Prefix = e.NetworkAddress.ToString();
                        leaseEntry.PrefixLength = e.PrefixLength;
                    });
                    break;

                case DHCPv6LeaseCanceledEvent e:
                    hasChanges = await UpdateEndToDHCPv6LeaseEntry(e, ReasonToEndLease.Canceled);
                    break;

                case DHCPv6LeaseReleasedEvent e:
                    hasChanges = await UpdateEndToDHCPv6LeaseEntry(e, ReasonToEndLease.Released);
                    break;

                case DHCPv6LeaseRenewedEvent e:
                    hasChanges = await UpdateLastestDHCPv6LeaseEntry(e, (leaseEntry) =>
                    {
                        leaseEntry.End = e.End;

                        if (e.Reset == true)
                        {
                            leaseEntry.IsActive = false;
                        }

                        leaseEntry.EndOfRenewalTime = e.RenewalTime;
                        leaseEntry.EndOfPreferredLifetime = e.PreferredLifetime;
                    });
                    break;

                case DHCPv6LeaseRevokedEvent e:
                    hasChanges = await UpdateEndToDHCPv6LeaseEntry(e, ReasonToEndLease.Revoked);
                    break;

                case DHCPv6LeaseRemovedEvent e:
                    hasChanges = await RemoveDHCPv6LeaseEntry(e);
                    break;

                default:
                    hasChanges = null;
                    break;
            }

            return hasChanges;
        }

        private async Task<Boolean?> ProjectDHCPv4PacketAndLeaseRelatedEvents(DomainEvent @event)
        {
            Boolean? hasChanges = new Boolean?();
            switch (@event)
            {

                case DHCPv4PacketHandledEvent e:
                    DHCPv4PacketHandledEntryDataModel entry = new DHCPv4PacketHandledEntryDataModel
                    {
                        HandledSuccessfully = e.WasSuccessfullHandled,
                        ErrorCode = e.ErrorCode,
                        Id = Guid.NewGuid(),

                        RequestSize = e.Request.GetSize(),
                        RequestDestination = e.Request.Header.Destionation.ToString(),
                        RequestSource = e.Request.Header.Source.ToString(),
                        RequestStream = e.Request.GetAsStream(),

                        ScopeId = e.ScopeId,
                        RequestType = e.Request.MessageType,
                        Timestamp = e.Timestamp,
                    };

                    if (e.Response != null)
                    {
                        entry.ResponseSize = e.Response.GetSize();
                        entry.ResponseType = e.Response.MessageType;
                        entry.ResponseDestination = e.Response.Header.Destionation.ToString();
                        entry.ResponseSource = e.Response.Header.Source.ToString();
                        entry.ResponseStream = e.Response.GetAsStream();
                    }

                    entry.SetTimestampDates();

                    DHCPv4PacketEntries.Add(entry);
                    hasChanges = true;
                    break;

                case DHCPv4LeaseCreatedEvent e:
                    {
                        DHCPv4LeaseEntryDataModel leaseEntry = new DHCPv4LeaseEntryDataModel
                        {
                            Id = Guid.NewGuid(),
                            Address = e.Address.ToString(),
                            Start = e.StartedAt,
                            End = e.ValidUntil,
                            LeaseId = e.EntityId,
                            ScopeId = e.ScopeId,
                            Timestamp = e.Timestamp,
                            IsActive = false,
                            EndOfRenewalTime = e.StartedAt + e.RenewalTime,
                            EndOfPreferredLifetime = e.StartedAt + e.PreferredLifetime,
                        };

                        DHCPv4LeaseEntries.Add(leaseEntry);
                        hasChanges = true;
                    }
                    break;

                case DHCPv4LeaseExpiredEvent e:
                    hasChanges = await UpdateEndToDHCPv4LeaseEntry(e, ReasonToEndLease.Expired);
                    break;

                case DHCPv4LeaseActivatedEvent e:
                    hasChanges = await UpdateLastestDHCPv4LeaseEntry(e, e => e.IsActive = true);
                    break;

                case DHCPv4LeaseCanceledEvent e:
                    hasChanges = await UpdateEndToDHCPv4LeaseEntry(e, ReasonToEndLease.Canceled);
                    break;

                case DHCPv4LeaseReleasedEvent e:
                    hasChanges = await UpdateEndToDHCPv4LeaseEntry(e, ReasonToEndLease.Released);
                    break;

                case DHCPv4LeaseRenewedEvent e:
                    hasChanges = await UpdateLastestDHCPv4LeaseEntry(e, (leaseEntry) =>
                    {
                        leaseEntry.End = e.End;
                        if (e.Reset == true)
                        {
                            leaseEntry.IsActive = false;
                        }

                        leaseEntry.EndOfRenewalTime = e.RenewalTime;
                        leaseEntry.EndOfPreferredLifetime = e.PreferredLifetime;
                    });
                    break;

                case DHCPv4LeaseRevokedEvent e:
                    hasChanges = await UpdateEndToDHCPv4LeaseEntry(e, ReasonToEndLease.Revoked);
                    break;

                case DHCPv4LeaseRemovedEvent e:
                    hasChanges = await RemoveDHCPv4LeaseEntry(e);
                    break;

                default:
                    hasChanges = null;
                    break;
            }

            return hasChanges;
        }

        public async Task<Boolean> Project(IEnumerable<DomainEvent> events)
        {
            Boolean hasChanges = true;
            foreach (var item in events)
            {
                Boolean handled = true;
                switch (item)
                {
                    case NotificationPipelineCreatedEvent e:
                        NotificationPipelines.Add(new NotificationPipelineOverviewEntry
                        {
                            Id = e.Id,
                            Name = e.Name
                        });
                        break;
                    case NotificationPipelineDeletedEvent e:
                        var existingPipeline = await NotificationPipelines.AsQueryable().FirstAsync(x => x.Id == e.Id);
                        NotificationPipelines.Remove(existingPipeline);
                        break;
                    case DHCPv6ListenerCreatedEvent e:
                        DHCPv6Interfaces.Add(new DHCPv6InterfaceDataModel
                        {
                            Id = e.Id,
                            InterfaceId = e.InterfaceId,
                            IPv6Address = e.Address,
                            Name = e.Name,
                        });
                        break;

                    case DHCPv6ListenerDeletedEvent e:
                        var existingv6Interface = await DHCPv6Interfaces.AsQueryable().FirstAsync(x => x.Id == e.Id);
                        DHCPv6Interfaces.Remove(existingv6Interface);
                        break;

                    case DHCPv4ListenerCreatedEvent e:
                        DHCPv4Interfaces.Add(new DHCPv4InterfaceDataModel
                        {
                            Id = e.Id,
                            InterfaceId = e.InterfaceId,
                            IPv4Address = e.Address,
                            Name = e.Name,
                        });

                        break;

                    case DHCPv4ListenerDeletedEvent e:
                        var existingv4Interface = await DHCPv4Interfaces.AsQueryable().FirstAsync(x => x.Id == e.Id);
                        DHCPv4Interfaces.Remove(existingv4Interface);
                        break;
                    default:
                        hasChanges = false;
                        handled = false;
                        break;
                }

                if (handled == true)
                {
                    continue;
                }

                Boolean? dhcpv6Related = await ProjectDHCPv6PacketAndLeaseRelatedEvents(item);
                if (dhcpv6Related.HasValue == true)
                {
                    if (hasChanges == false && dhcpv6Related.Value == true)
                    {
                        hasChanges = true;
                    }
                }
                else
                {
                    Boolean? dhcpv4Related = await ProjectDHCPv4PacketAndLeaseRelatedEvents(item);
                    if (dhcpv4Related.HasValue == true && hasChanges == false && dhcpv4Related.Value == true)
                    {
                        hasChanges = true;
                    }
                }
            }

            if (hasChanges == false) { return true; }

            return await SaveChangesAsyncInternal();
        }

        private async Task<Boolean> UpdateEndToDHCPv6LeaseEntry(DHCPv6ScopeRelatedEvent e, ReasonToEndLease reason) =>

             await UpdateLastestDHCPv6LeaseEntry(e, (leaseEntry) =>
             {
                 leaseEntry.End = e.Timestamp;
                 leaseEntry.EndReason = reason;
             });

        private async Task<Boolean> UpdateLastestDHCPv6LeaseEntry(DHCPv6ScopeRelatedEvent e, Action<DHCPv6LeaseEntryDataModel> updater)
        {
            DHCPv6LeaseEntryDataModel entry = await GetLatestDHCPv6LeaseEntry(e);
            if (entry != null)
            {
                updater(entry);
                return true;
            }

            return false;
        }

        private async Task<DHCPv6LeaseEntryDataModel> GetLatestDHCPv6LeaseEntry(DHCPv6ScopeRelatedEvent e) =>
        await DHCPv6LeaseEntries.AsQueryable().Where(x => x.LeaseId == e.EntityId).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync();

        private async Task<Boolean> RemoveDHCPv6LeaseEntry(DHCPv6ScopeRelatedEvent e)
        {
            DHCPv6LeaseEntryDataModel entry = await GetLatestDHCPv6LeaseEntry(e);
            if (entry != null) { return false; }

            DHCPv6LeaseEntries.Remove(entry);
            return true;
        }

        private async Task<Boolean> UpdateEndToDHCPv4LeaseEntry(DHCPv4ScopeRelatedEvent e, ReasonToEndLease reason) =>
             await UpdateLastestDHCPv4LeaseEntry(e, (leaseEntry) =>
             {
                 leaseEntry.End = e.Timestamp;
                 leaseEntry.EndReason = reason;
                 leaseEntry.IsActive = false;
             });

        private async Task<Boolean> UpdateLastestDHCPv4LeaseEntry(DHCPv4ScopeRelatedEvent e, Action<DHCPv4LeaseEntryDataModel> updater)
        {
            DHCPv4LeaseEntryDataModel entry = await GetLatestDHCPv4LeaseEntry(e);
            if (entry != null)
            {
                updater(entry);
                return true;
            }

            return false;
        }

        private async Task<DHCPv4LeaseEntryDataModel> GetLatestDHCPv4LeaseEntry(DHCPv4ScopeRelatedEvent e) =>
            await DHCPv4LeaseEntries.AsQueryable().Where(x => x.LeaseId == e.EntityId).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync();

        private async Task<Boolean> RemoveDHCPv4LeaseEntry(DHCPv4ScopeRelatedEvent e)
        {
            DHCPv4LeaseEntryDataModel entry = await GetLatestDHCPv4LeaseEntry(e);
            if (entry != null) { return false; }

            DHCPv4LeaseEntries.Remove(entry);
            return true;
        }

        private const String _DHCPv6ServerConfigKey = "DHCPv6ServerConfig";

        public async Task<DHCPv6ServerProperties> GetServerProperties()
        {
            String content = await Helpers.AsQueryable().Where(x => x.Name == _DHCPv6ServerConfigKey).Select(x => x.Content).FirstOrDefaultAsync();
            if (String.IsNullOrEmpty(content) == true)
            {
                return new DHCPv6ServerProperties
                {
                    IsInitilized = true,
                    LeaseLifeTime = TimeSpan.FromDays(15),
                    HandledLifeTime = TimeSpan.FromDays(15),
                    MaximumHandldedCounter = 100_000,
                    ServerDuid = new UUIDDUID(Guid.Parse("bbd541ea-8499-44b4-ad9d-d398c4643e79"))
                };
            }

            var result = JsonConvert.DeserializeObject<DHCPv6ServerProperties>(content, new DUIDJsonConverter());
            result.SetDefaultIfNeeded();

            return result;
        }

        public async Task<Boolean> SaveInitialServerConfiguration(DHCPv6ServerProperties config)
        {
            config.IsInitilized = true;
            config.SetDefaultIfNeeded();
            String content = JsonConvert.SerializeObject(config, new DUIDJsonConverter());

            Helpers.Add(new HelperEntry
            {
                Name = _DHCPv6ServerConfigKey,
                Content = content,
            });

            return await SaveChangesAsyncInternal();
        }


        private async Task<Boolean> DeleteEntriesBasedOnTimestampAndEventType(DateTime leaseThreshold, Boolean isLsesed)
        {
            if (isLsesed == false)
            {
                var statisticEntriesToRemove = await DHCPv6PacketEntries.AsQueryable().Where(x => x.Timestamp < leaseThreshold).ToListAsync();
                DHCPv6PacketEntries.RemoveRange(statisticEntriesToRemove);
            }
            else
            {
                var leaseEntriesToRmeove = await DHCPv6LeaseEntries.AsQueryable().Where(x => x.Timestamp < leaseThreshold).ToListAsync();
                DHCPv6LeaseEntries.RemoveRange(leaseEntriesToRmeove);
            }

            return await SaveChangesAsyncInternal();
        }

        public async Task<Boolean> DeleteLeaseRelatedEventsOlderThan(DateTime leaseThreshold)
        {
            Boolean result =
                 await DeleteEntriesBasedOnTimestampAndEventType(leaseThreshold, true) &&
                 await DeleteEntriesBasedOnTimestampAndEventType(leaseThreshold, true);
            return result;
        }

        public async Task<Boolean> DeletePacketHandledEventsOlderThan(DateTime handledEventThreshold)
        {
            Boolean result =
                await DeleteEntriesBasedOnTimestampAndEventType(handledEventThreshold, false) &&
                await DeleteEntriesBasedOnTimestampAndEventType(handledEventThreshold, false);

            return result;

        }

        public async Task<Boolean> DeletePacketHandledEventMoreThan(UInt32 threshold)
        {
            {
                Int32 statisticsDiff = (Int32)threshold - await DHCPv6PacketEntries.AsQueryable().CountAsync();
                var statisticEntriesToRemove = await DHCPv6PacketEntries.AsQueryable().OrderBy(x => x.Timestamp).Take(statisticsDiff).ToListAsync();
                DHCPv6PacketEntries.RemoveRange(statisticEntriesToRemove);
            }
            {
                Int32 statisticsDiff = (Int32)threshold - await DHCPv4PacketEntries.AsQueryable().CountAsync();
                var statisticEntriesToRemove = await DHCPv4PacketEntries.AsQueryable().OrderBy(x => x.Timestamp).Take(statisticsDiff).ToListAsync();
                DHCPv4PacketEntries.RemoveRange(statisticEntriesToRemove);
            }

            return await SaveChangesAsyncInternal();
        }

        private async Task<IList<DHCPv6PacketHandledEntry>> GetDHCPv6PacketsFromHandledEvents(Int32 amount, Guid? scopeId)
        {
            return await GetPacketsFromHandledEvents(DHCPv6PacketEntries, amount, scopeId, (item) =>
            {
                DHCPv6PacketHandledEntry entry = new DHCPv6PacketHandledEntry
                {
                    FilteredBy = item.FilteredBy,
                    InvalidRequest = item.InvalidRequest,
                    RequestType = item.RequestType,
                    ResponseType = item.ResponseType,
                    ResultCode = item.ErrorCode,
                    ScopeId = item.ScopeId,
                    Timestamp = item.Timestamp,
                };

                if (String.IsNullOrEmpty(item.RequestSource) == true)
                {
                    Console.WriteLine($"{item.RequestSize} | {item.ScopeId}");
                }

                if (String.IsNullOrEmpty(item.RequestDestination) == true)
                {
                    Console.WriteLine($"{item.RequestSize} | {item.ScopeId}");
                }

                DHCPv6Packet requestPacket = DHCPv6Packet.FromByteArray(
                      item.RequestStream, new IPv6HeaderInformation(
                          IPv6Address.FromString(item.RequestSource),
                          IPv6Address.FromString(item.RequestDestination)));

                entry.Request = new DHCPv6PacketInformation(requestPacket);

                if (item.ResponseSize.HasValue == true)
                {
                    DHCPv6Packet responsePacket = DHCPv6Packet.FromByteArray(
                        item.ResponseStream, new IPv6HeaderInformation(
                            IPv6Address.FromString(item.ResponseSource), IPv6Address.FromString(item.ResponseDestination)));

                    entry.Response = new DHCPv6PacketInformation(responsePacket);
                }

                return entry;
            });
        }

        public async Task<IDictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>>> GetIncomingDHCPv6PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetIncomingPacketTypes(DHCPv6PacketEntries, start, end, groupedBy);
        }

        public async Task<IDictionary<DateTime, Int32>> GetFileredDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetFileredDHCPPackets(DHCPv6PacketEntries, start, end, groupedBy);
        }

        public async Task<IDictionary<DateTime, Int32>> GetErrorDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetErrorFromDHCPPackets(DHCPv6PacketEntries, start, end, groupedBy);
        }

        public async Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv6PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetIncomingDHCPPacketAmount(DHCPv6PacketEntries, start, end, groupedBy);
        }

        public async Task<IDictionary<DateTime, Int32>> GetActiveDHCPv6Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetActiveDHCPLeases(DHCPv6LeaseEntries, start, end, groupedBy);
        }

        public async Task<DashboardResponse> GetDashboardOverview()
        {
            DateTime now = DateTime.UtcNow;

            DashboardResponse response = new DashboardResponse
            {
                DHCPv6 = new DHCPOverview<DHCPv6LeaseEntry, DHCPv6PacketHandledEntry>
                {
                    ActiveInterfaces = await DHCPv6Interfaces.AsQueryable().CountAsync(),
                    ActiveLeases = await DHCPv6LeaseEntries.AsQueryable().Where(x => now >= x.Start && now <= x.End && x.EndReason == ReasonToEndLease.Nothing).OrderByDescending(x => x.End).Select(x => new DHCPv6LeaseEntry
                    {
                        Address = x.Address,
                        End = x.End,
                        EndReason = x.EndReason,
                        LeaseId = x.LeaseId,
                        Prefix = x.Prefix,
                        PrefixLength = x.PrefixLength,
                        ScopeId = x.ScopeId,
                        Start = x.Start,
                        Timestamp = x.Timestamp,
                        ExpectedRenewalAt = x.EndOfRenewalTime,
                        ExpectedRebindingAt = x.EndOfPreferredLifetime
                    }).Take(1000).ToListAsync(),
                    Packets = await GetDHCPv6PacketsFromHandledEvents(100, null),
                },
                DHCPv4 = new DHCPOverview<DHCPv4LeaseEntry, DHCPv4PacketHandledEntry>
                {
                    ActiveInterfaces = await DHCPv4Interfaces.AsQueryable().CountAsync(),
                    ActiveLeases = await DHCPv4LeaseEntries.AsQueryable().Where(x => now >= x.Start && now <= x.End && x.EndReason == ReasonToEndLease.Nothing).OrderByDescending(x => x.End).Select(x => new DHCPv4LeaseEntry
                    {
                        Address = x.Address,
                        End = x.End,
                        EndReason = x.EndReason,
                        LeaseId = x.LeaseId,
                        ScopeId = x.ScopeId,
                        Start = x.Start,
                        Timestamp = x.Timestamp,
                        ExpectedRenewalAt = x.EndOfRenewalTime,
                        ExpectedRebindingAt = x.EndOfPreferredLifetime
                    }).Take(1000).ToListAsync(),
                    Packets = await GetDHCPv4PacketsFromHandledEvents(100, null),
                },
            };

            return response;
        }

        public async Task<IDictionary<DateTime, IDictionary<TPacketType, Int32>>> GetIncomingPacketTypes<TPacketType>(
            IQueryable<IPacketHandledEntry<TPacketType>> set,
            DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy) where TPacketType : struct
        {

            IQueryable<IPacketHandledEntry<TPacketType>> packets = GetPrefiltedPackets(set, start, end);

            var packetEntryResult = await packets.ToListAsync();

            IEnumerable<IGrouping<DateTime, IPacketHandledEntry<TPacketType>>> groupedResult = null;
            switch (groupedBy)
            {
                case GroupStatisticsResultBy.Day:
                    groupedResult = packetEntryResult.GroupBy(x => x.TimestampDay);
                    break;
                case GroupStatisticsResultBy.Week:
                    groupedResult = packetEntryResult.GroupBy(x => x.TimestampWeek);
                    break;
                case GroupStatisticsResultBy.Month:
                    groupedResult = packetEntryResult.GroupBy(x => x.TimestampMonth);
                    break;
                default:
                    break;
            }

            var result = groupedResult.Select(x => new
            {
                Key = x.Key,
                RequestTypes = x.Select(x => x.RequestType)
            }).ToDictionary(
                    x => x.Key,
                    x => x.RequestTypes.GroupBy(y => y).ToDictionary(y => y.Key, y => y.Count()) as IDictionary<TPacketType, Int32>);

            return result;
        }

        private async Task<IDictionary<DateTime, Int32>> GetFileredDHCPPackets<TMessageType>(
            IQueryable<IPacketHandledEntry<TMessageType>> packets, DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy) where TMessageType : struct
        {
            var prefilter = GetPrefiltedPackets(packets, start, end);
            packets = prefilter.Where(x => x.FilteredBy != null);
            return await GroupPackets(groupedBy, packets);
        }

        private async Task<IDictionary<DateTime, Int32>> GetErrorFromDHCPPackets<TMessageType>(
            IQueryable<IPacketHandledEntry<TMessageType>> packets, DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy) where TMessageType : struct
        {
            var prefilter = GetPrefiltedPackets(packets, start, end);
            packets = prefilter.Where(x => x.InvalidRequest == true);
            return await GroupPackets(groupedBy, packets);
        }

        private async Task<IDictionary<DateTime, Int32>> GetIncomingDHCPPacketAmount<TMessageType>(
            IQueryable<IPacketHandledEntry<TMessageType>> packets, DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy) where TMessageType : struct
        {
            var prefilter = GetPrefiltedPackets(packets, start, end);
            return await GroupPackets(groupedBy, prefilter);
        }

        private static async Task<IDictionary<DateTime, Int32>> GroupPackets<TMessageType>(GroupStatisticsResultBy groupedBy, IQueryable<IPacketHandledEntry<TMessageType>> packets) where TMessageType : struct
        {
            IQueryable<IGrouping<DateTime, IPacketHandledEntry<TMessageType>>> groupedResult = null;
            switch (groupedBy)
            {
                case GroupStatisticsResultBy.Day:
                    groupedResult = packets.GroupBy(x => x.TimestampDay);
                    break;
                case GroupStatisticsResultBy.Week:
                    groupedResult = packets.GroupBy(x => x.TimestampWeek);
                    break;
                case GroupStatisticsResultBy.Month:
                    groupedResult = packets.GroupBy(x => x.TimestampMonth);
                    break;
                default:
                    break;
            }

            var result = await groupedResult.Select(x => new
            {
                Key = x.Key,
                Amount = x.Count()
            }).ToDictionaryAsync(x => x.Key, x => x.Amount);

            return result;
        }

        private IQueryable<IPacketHandledEntry<TMessageType>> GetPrefiltedPackets<TMessageType>(IQueryable<IPacketHandledEntry<TMessageType>> set, DateTime? start, DateTime? end)
            where TMessageType : struct
        {
            if (start.HasValue == true)
            {
                set = set.Where(x => x.Timestamp >= start);
            }
            if (end.HasValue == true)
            {
                set = set.Where(x => x.Timestamp <= end.Value);
            }

            return set;
        }

        public async Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPRequestType<TMessageType>(IQueryable<IPacketHandledEntry<TMessageType>> set, DateTime? start, DateTime? end, TMessageType type) where TMessageType : struct
        {

            IQueryable<IPacketHandledEntry<TMessageType>> packets = GetPrefiltedPackets(set, start, end);
            packets = packets.Where(x => x.RequestType.Equals(type) == true);

            var result = await packets.GroupBy(x => x.ErrorCode).Select(x => new
            {
                Key = x.Key,
                Amount = x.Count()
            }).ToDictionaryAsync(x => x.Key, x => x.Amount);

            return result;
        }

        public async Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPV6RequestType(DateTime? start, DateTime? end, DHCPv6PacketTypes type)
        {
            return await GetErrorCodesPerDHCPRequestType(DHCPv6PacketEntries, start, end, type);
        }

        private class DateTimeRange
        {
            public DateTime RangeStart { get; set; }
            public DateTime RangeEnd { get; set; }
        }

        private Int32 CountTimeIntersection(DateTime start, DateTime end, IEnumerable<DateTimeRange> ranges) =>
            ranges.Count(x => start < x.RangeEnd && end >= x.RangeStart);


        public async Task<IDictionary<DateTime, Int32>> GetActiveDHCPLeases(IQueryable<ILeaseEntry> set, DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            start ??= await set.OrderBy(x => x.Timestamp).Select(x => x.Timestamp).FirstOrDefaultAsync();
            if (start.Value == default)
            {
                return new Dictionary<DateTime, Int32>();
            }

            end ??= DateTime.UtcNow;
            DateTime currentStart;

            var preResult = await set.Where(x => start.Value < x.End && end.Value >= x.Start)
                .Select(x => new DateTimeRange { RangeStart = x.Start, RangeEnd = x.End }).ToListAsync();

            if (preResult.Count == 0)
            {
                return new Dictionary<DateTime, Int32>();
            }

            DateTime firstStart = start.Value;
            Func<DateTime, DateTime> timeAdjuster;
            switch (groupedBy)
            {
                case GroupStatisticsResultBy.Day:
                    currentStart = firstStart.Date.AddDays(1);
                    timeAdjuster = x => x.AddDays(1);
                    break;
                case GroupStatisticsResultBy.Week:
                    currentStart = firstStart.GetFirstWeekDay().AddDays(7);
                    timeAdjuster = x => x.AddDays(7);
                    break;
                case GroupStatisticsResultBy.Month:
                    currentStart = new DateTime(firstStart.Year, firstStart.Month, 1).AddMonths(1);
                    timeAdjuster = x => x.AddMonths(1);

                    break;
                default:
                    throw new NotImplementedException();
            }

            Dictionary<DateTime, Int32> result = new Dictionary<DateTime, int>
            {
                { firstStart, CountTimeIntersection(firstStart, currentStart, preResult) }
            };
            while (currentStart < end.Value)
            {
                DateTime intervallEnd = timeAdjuster(currentStart);

                Int32 elements = CountTimeIntersection(currentStart, intervallEnd, preResult);
                result.Add(currentStart, elements);

                currentStart = intervallEnd;
            }

            return result;
        }

        private async Task<Boolean> AddDHCPv6PacketHandledEntryDataModel(DHCPv6Packet packet, Action<DHCPv6PacketHandledEntryDataModel> modifier)
        {
            var entry = new DHCPv6PacketHandledEntryDataModel
            {
                RequestSize = packet.GetSize(),
                RequestType = packet.GetInnerPacket().PacketType,
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid(),
            };
            modifier?.Invoke(entry);

            entry.SetTimestampDates();

            DHCPv6PacketEntries.Add(entry);
            return await SaveChangesAsyncInternal();
        }

        public async Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet) => await AddDHCPv6PacketHandledEntryDataModel(packet, (x) => x.InvalidRequest = true);
        public async Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName) => await AddDHCPv6PacketHandledEntryDataModel(packet, (x) => x.FilteredBy = filterName);

        public async Task<IEnumerable<DHCPv6PacketHandledEntry>> GetHandledDHCPv6PacketByScopeId(Guid scopeId, Int32 amount) => await GetDHCPv6PacketsFromHandledEvents(amount, scopeId);

        private async Task<Boolean> AddDHCPv4PacketHandledEntryDataModel(DHCPv4Packet packet, Action<DHCPv4PacketHandledEntryDataModel> modifier)
        {

            var entry = new DHCPv4PacketHandledEntryDataModel
            {
                RequestSize = packet.GetSize(),
                RequestType = packet.MessageType,
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid(),
            };
            modifier?.Invoke(entry);

            entry.SetTimestampDates();

            DHCPv4PacketEntries.Add(entry);
            return await SaveChangesAsyncInternal();
        }

        public async Task<Boolean> LogInvalidDHCPv4Packet(DHCPv4Packet packet) => await AddDHCPv4PacketHandledEntryDataModel(packet, (x) => x.InvalidRequest = true);
        public async Task<Boolean> LogFilteredDHCPv4Packet(DHCPv4Packet packet, String filterName) => await AddDHCPv4PacketHandledEntryDataModel(packet, (x) => x.FilteredBy = filterName);

        private async Task<IList<TEntry>> GetPacketsFromHandledEvents<TEntry, TDHCPMessagesTypes>(IQueryable<IPacketHandledEntry<TDHCPMessagesTypes>> input, Int32 amount, Guid? scopeId, Func<IPacketHandledEntry<TDHCPMessagesTypes>, TEntry> activator)
            where TDHCPMessagesTypes : struct
        {
            if (scopeId.HasValue == true)
            {
                input = input.Where(x => x.ScopeId == scopeId.Value);
            }

            var entries = await input.OrderByDescending(x => x.Timestamp).Take(amount)
               .ToListAsync();

            List<TEntry> result = new List<TEntry>(entries.Count);

            foreach (var item in entries)
            {
                var parsedItem = activator(item);

                result.Add(parsedItem);

            }

            return result;
        }

        private async Task<IList<DHCPv4PacketHandledEntry>> GetDHCPv4PacketsFromHandledEvents(Int32 amount, Guid? scopeId)
        {
            return await GetPacketsFromHandledEvents(DHCPv4PacketEntries, amount, scopeId, (item) =>
           {
               DHCPv4PacketHandledEntry entry = new DHCPv4PacketHandledEntry
               {
                   FilteredBy = item.FilteredBy,
                   InvalidRequest = item.InvalidRequest,
                   RequestType = item.RequestType,
                   ResponseType = item.ResponseType,
                   ResultCode = item.ErrorCode,
                   ScopeId = item.ScopeId,
                   Timestamp = item.Timestamp,
               };

               DHCPv4Packet requestPacket = DHCPv4Packet.FromByteArray(
                     item.RequestStream, new IPv4HeaderInformation(
                         IPv4Address.FromString(item.RequestSource), IPv4Address.FromString(item.RequestDestination)));

               entry.Request = new DHCPv4PacketInformation(requestPacket);

               if (item.ResponseSize.HasValue == true)
               {
                   DHCPv4Packet responsePacket = DHCPv4Packet.FromByteArray(
                       item.ResponseStream, new IPv4HeaderInformation(
                           IPv4Address.FromString(item.ResponseSource), IPv4Address.FromString(item.ResponseDestination)));

                   entry.Response = new DHCPv4PacketInformation(responsePacket);
               }

               return entry;
           });
        }

        public async Task<IDictionary<DateTime, IDictionary<DHCPv4MessagesTypes, Int32>>> GetIncomingDHCPv4PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetIncomingPacketTypes(DHCPv4PacketEntries, start, end, groupedBy);
        }

        public async Task<IDictionary<DateTime, Int32>> GetFileredDHCPv4Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetFileredDHCPPackets(DHCPv4PacketEntries, start, end, groupedBy);
        }

        public async Task<IDictionary<DateTime, Int32>> GetErrorDHCPv4Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetErrorFromDHCPPackets(DHCPv4PacketEntries, start, end, groupedBy);
        }

        public async Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv4PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetIncomingDHCPPacketAmount(DHCPv4PacketEntries, start, end, groupedBy);
        }

        public async Task<IDictionary<DateTime, Int32>> GetActiveDHCPv4Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await GetActiveDHCPLeases(DHCPv4LeaseEntries, start, end, groupedBy);
        }

        public async Task<IEnumerable<DHCPv4PacketHandledEntry>> GetHandledDHCPv4PacketByScopeId(Guid scopeId, Int32 amount) => await GetDHCPv4PacketsFromHandledEvents(amount, scopeId);

        public async Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPv4DHCPv4MessagesTypes(DateTime? start, DateTime? end, DHCPv4MessagesTypes type)
        {
            return await GetErrorCodesPerDHCPRequestType(DHCPv4PacketEntries, start, end, type);
        }

        public async Task<IEnumerable<Guid>> GetAllNotificationPipelineIds() => await NotificationPipelines.AsQueryable().Select(x => x.Id).ToListAsync();
    }
}

﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Core.Tracing;
using Beer.DaAPI.Infrastructure.Helper;
using Beer.DaAPI.Infrastructure.Services;
using Beer.DaAPI.Infrastructure.StorageEngine.Converters;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Shared.Commands;
using Beer.DaAPI.Shared.Helper;
using Beer.DaAPI.Shared.JsonConverters;
using Beer.DaAPI.Shared.Responses;
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
using static Beer.DaAPI.Shared.Requests.TracingRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4LeasesResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6LeasesResponses.V1;
using static Beer.DaAPI.Shared.Responses.PacketMonitorResponses.V1;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    public static class DateTimeExtensions
    {
        public static DateTime? SetKindUtc(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return dateTime.Value.SetKindUtc();
            }
            else
            {
                return null;
            }
        }
        public static DateTime SetKindUtc(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc) { return dateTime; }
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }

    public class StorageContext : DbContext, IDHCPv6ReadStore, IDHCPv4ReadStore, IDHCPv6PrefixCollector
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

        public DbSet<DeviceEntryDataModel> Devices { get; set; }

        public DbSet<TracingStreamDataModel> TracingStreams { get; set; }
        public DbSet<TracingStreamEntryDataModel> TracingStreamRecords { get; set; }

        public DbSet<LeaseEventEntryDataModel> LeaseEventEntries { get; set; }

        #endregion

        public StorageContext(DbContextOptions<StorageContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TracingStreamDataModel>()
                .Property(b => b.FirstEntryData)
                .HasJsonConversion();

            modelBuilder.Entity<TracingStreamEntryDataModel>()
                 .Property(b => b.AddtionalData)
                 .HasJsonConversion();
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

        public async Task<IDictionary<Guid, IEnumerable<DHCPv6LeaseCreatedEvent>>> GetLatestDHCPv6LeasesForHydration()
        {
            var filteredLeases = await ((IQueryable<ILeaseEntry>)DHCPv6LeaseEntries).Where(x => x.EndReason == ReasonToEndLease.Nothing && x.IsActive == true).ToArrayAsync();
            var firstGroupResult = (from lease in filteredLeases
                                    group lease by lease.ScopeId into g
                                    select new
                                    {
                                        Id = g.Key,
                                        Items = (from subLease in g
                                                 group subLease by subLease.Address into subG
                                                 select subG.OrderByDescending(x => x.Timestamp).Cast<DHCPv6LeaseEntryDataModel>().FirstOrDefault())
                                    }).ToDictionary(x => x.Id, x => x.Items.ToArray());

            return firstGroupResult.ToDictionary(x => x.Key, x => (IEnumerable<DHCPv6LeaseCreatedEvent>)x.Value.Select(y => new DHCPv6LeaseCreatedEvent
            {
                Address = IPv6Address.FromString(y.Address),
                DelegatedNetworkAddress = String.IsNullOrEmpty(y.Prefix) == true ? null : IPv6Address.TryFromString(y.Prefix),
                HasPrefixDelegation = String.IsNullOrEmpty(y.Prefix) == false && IPv6Address.TryFromString(y.Prefix) != null,
                PrefixLength = y.PrefixLength,
                IdentityAssocationId = y.IdentityAssocationId,
                IdentityAssocationIdForPrefix = y.IdentityAssocationIdForPrefix,
                ClientIdentifier = DUIDFactory.GetDUID(y.ClientIdentifier),
                EntityId = y.LeaseId,
                ScopeId = y.ScopeId,
                StartedAt = y.Start,
                ValidUntil = y.End,
                Timestamp = y.Timestamp,
                UniqueIdentiifer = y.UniqueIdentifier,
            }).ToList());
        }

        private async Task<Boolean?> ProjectDHCPv6PacketAndLeaseRelatedEvents(DomainEvent @event)
        {
            Boolean? hasChanges = new Boolean?();
            switch (@event)
            {

                case DHCPv6PacketHandledEvent e:
                    var entry = new DHCPv6PacketHandledEntryDataModel();

                    DHCPv6PacketEntries.Add(entry);
                    await SetLeaseEventAndPacketEntryRelation(entry.Id, e.Response != null);

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
                            IdentityAssocationIdForPrefix = e.HasPrefixDelegation == true ? e.IdentityAssocationIdForPrefix : 0,
                            IdentityAssocationId = e.IdentityAssocationId,
                            Timestamp = e.Timestamp,
                            ClientIdentifier = e.ClientIdentifier?.GetAsByteStream(),
                            UniqueIdentifier = e.UniqueIdentiifer,
                        };

                        await RemoveOldDHCPv6Leases(e.ScopeId, e.Address.ToString());

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
                    DHCPv6LeaseEntryDataModel oldEntry = await GetLatestDHCPv6LeaseEntry(e);
                    hasChanges = await UpdateEndToDHCPv6LeaseEntry(e, ReasonToEndLease.PseudoCancel);
                    if (hasChanges == true)
                    {
                        DHCPv6LeaseEntryDataModel leaseEntry = new DHCPv6LeaseEntryDataModel
                        {
                            Id = Guid.NewGuid(),
                            Address = oldEntry.Address,
                            Start = e.Timestamp,
                            End = e.End,
                            IsActive = e.Reset == false,
                            EndOfRenewalTime = e.Timestamp + e.RenewSpan,
                            EndOfPreferredLifetime = e.Timestamp + e.ReboundSpan,
                            LeaseId = e.EntityId,
                            ScopeId = e.ScopeId,
                            Prefix = oldEntry.Prefix,
                            PrefixLength = oldEntry.PrefixLength,
                            IdentityAssocationIdForPrefix = oldEntry.IdentityAssocationIdForPrefix,
                            IdentityAssocationId = oldEntry.IdentityAssocationId,
                            Timestamp = e.Timestamp,
                            ClientIdentifier = oldEntry.ClientIdentifier,
                            UniqueIdentifier = oldEntry.UniqueIdentifier,
                        };

                        await RemoveOldDHCPv6Leases(e.ScopeId, oldEntry.Address);

                        DHCPv6LeaseEntries.Add(leaseEntry);
                    }

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

        private async Task RemoveOldDHCPv6Leases(Guid scopeId, String address)
        {
            var oldLeases = await ((IQueryable<ILeaseEntry>)DHCPv6LeaseEntries).Where(x =>
                x.Address == address &&
                x.ScopeId == scopeId &&
                x.EndReason == ReasonToEndLease.Nothing)
                .Cast<DHCPv6LeaseEntryDataModel>().ToArrayAsync();

            if (oldLeases.Length > 0)
            {
                DHCPv6LeaseEntries.RemoveRange(oldLeases);
            }
        }

        public async Task<IDictionary<Guid, IEnumerable<DHCPv4LeaseCreatedEvent>>> GetLatestDHCPv4LeasesForHydration()
        {
            var filteredLeases = await ((IQueryable<ILeaseEntry>)DHCPv4LeaseEntries).Where(x => x.EndReason == ReasonToEndLease.Nothing && x.IsActive == true).ToArrayAsync();
            var firstGroupResult = (from lease in filteredLeases
                                    group lease by lease.ScopeId into g
                                    select new
                                    {
                                        Id = g.Key,
                                        Items = (from subLease in g
                                                 group subLease by subLease.Address into subG
                                                 select subG.OrderByDescending(x => x.Timestamp).Cast<DHCPv4LeaseEntryDataModel>().FirstOrDefault())
                                    }).ToDictionary(x => x.Id, x => x.Items.ToArray());

            return firstGroupResult.ToDictionary(x => x.Key, x => x.Value.Select(y => new DHCPv4LeaseCreatedEvent
            {
                Address = IPv4Address.FromString(y.Address),
                ClientIdenfier = y.ClientIdentifier,
                ClientMacAddress = y.ClientMacAddress,
                EntityId = y.LeaseId,
                ScopeId = y.ScopeId,
                StartedAt = y.Start,
                ValidUntil = y.End,
                Timestamp = y.Timestamp,
                UniqueIdentifier = y.UniqueIdentifier,
            }));
        }

        private async Task SetLeaseEventAndPacketEntryRelation(Guid id, Boolean hasResponse)
        {

            if (_latestLeaseEvents.Count > 0)
            {
                var items = await ((IQueryable<LeaseEventEntryDataModel>)LeaseEventEntries).Where(x =>
                _latestLeaseEvents.Contains(x.Id)).ToListAsync();

                items.ForEach(x =>
                {
                    x.PacketHandledEntryId = id;
                    x.HasResponse = hasResponse;
                });
            }
        }

        private async Task<Boolean?> ProjectDHCPv4PacketAndLeaseRelatedEvents(DomainEvent @event)
        {
            Boolean? hasChanges = new Boolean?();
            switch (@event)
            {
                case DHCPv4PacketHandledEvent e:

                    var entry = new DHCPv4PacketHandledEntryDataModel(e);
                    DHCPv4PacketEntries.Add(entry);
                    await SetLeaseEventAndPacketEntryRelation(entry.Id, e.Response != null);

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
                            ClientIdentifier = e.ClientIdenfier,
                            ClientMacAddress = e.ClientMacAddress,
                            UniqueIdentifier = e.UniqueIdentifier,
                            EndOfRenewalTime = e.StartedAt + e.RenewalTime,
                            EndOfPreferredLifetime = e.StartedAt + e.PreferredLifetime,
                            OrderNumber = e.Address.GetNumericValue(),
                        };

                        await RemoveOldDHCPv4Leases(e.ScopeId, e.Address.ToString());

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
                    DHCPv4LeaseEntryDataModel exisitingentry = await GetLatestDHCPv4LeaseEntry(e);
                    hasChanges = await UpdateEndToDHCPv4LeaseEntry(e, ReasonToEndLease.PseudoCancel);
                    if (hasChanges == true)
                    {
                        await SaveChangesAsyncInternal();

                        DHCPv4LeaseEntryDataModel leaseEntry = new DHCPv4LeaseEntryDataModel
                        {
                            Id = Guid.NewGuid(),
                            Address = exisitingentry.Address,
                            Start = e.Timestamp,
                            End = e.End,
                            LeaseId = e.EntityId,
                            ScopeId = e.ScopeId,
                            Timestamp = e.Timestamp,
                            IsActive = e.Reset == false,
                            ClientIdentifier = exisitingentry.ClientIdentifier,
                            ClientMacAddress = exisitingentry.ClientMacAddress,
                            UniqueIdentifier = exisitingentry.UniqueIdentifier,
                            EndOfRenewalTime = e.Timestamp + e.RenewSpan,
                            EndOfPreferredLifetime = e.Timestamp + e.ReboundSpan,
                            OrderNumber = exisitingentry.OrderNumber,
                        };

                        if (leaseEntry.OrderNumber == 0)
                        {
                            leaseEntry.OrderNumber = IPv4Address.FromString(exisitingentry.Address).GetNumericValue();
                        }

                        await RemoveOldDHCPv4Leases(e.ScopeId, exisitingentry.Address);
                        DHCPv4LeaseEntries.Add(leaseEntry);
                    }
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

        private async Task RemoveOldDHCPv4Leases(Guid scopeId, String address)
        {
            var oldLeases = await ((IQueryable<ILeaseEntry>)DHCPv4LeaseEntries).Where(x =>
            x.Address == address &&
            x.ScopeId == scopeId &&
            x.EndReason == ReasonToEndLease.Nothing).Cast<DHCPv4LeaseEntryDataModel>().ToArrayAsync();

            if (oldLeases.Length > 0)
            {
                DHCPv4LeaseEntries.RemoveRange(oldLeases);
            }
        }

        private List<Guid> _latestLeaseEvents = new();

        public async Task<Boolean> Project(IEnumerable<DomainEvent> events)
        {
            _latestLeaseEvents.Clear();

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

                Dictionary<Guid, String> leaseAddressMapper = new();
                var settings = new JsonSerializerSettings();
                settings.LoadCustomerConverters();

                await SaveChangesAsyncInternal();

                if (item is DHCPv4ScopeRelatedEvent || item is DHCPv6ScopeRelatedEvent)
                {
                    var entry = new LeaseEventEntryDataModel
                    {
                        Id = Guid.NewGuid(),
                        EventType = item.GetType().Name,
                        FullEventType = item.GetType().FullName,
                        ScopeId = item is DHCPv4ScopeRelatedEvent ? ((DHCPv4ScopeRelatedEvent)item).ScopeId : ((DHCPv6ScopeRelatedEvent)item).ScopeId,
                        Timestamp = DateTime.Now.SetKindUtc(),
                        LeaseId = item is DHCPv4ScopeRelatedEvent ? ((DHCPv4ScopeRelatedEvent)item).EntityId : ((DHCPv6ScopeRelatedEvent)item).EntityId,
                        EventData = JsonConvert.SerializeObject(item, settings)
                    };

                    if (leaseAddressMapper.ContainsKey(entry.LeaseId) == false)
                    {
                        IQueryable<ILeaseEntry> collection = item is DHCPv4ScopeRelatedEvent ? DHCPv4LeaseEntries : DHCPv6LeaseEntries;

                        var address = await collection.Where(x => x.LeaseId == entry.LeaseId).Select(x => x.Address).FirstOrDefaultAsync();
                        leaseAddressMapper.Add(entry.LeaseId, address);
                    }

                    entry.Address = leaseAddressMapper[entry.LeaseId];

                    _latestLeaseEvents.Add(entry.Id);
                    LeaseEventEntries.Add(entry);

                    hasChanges = true;
                    await SaveChangesAsyncInternal();
                }
            }


            if (hasChanges == false) { return true; }

            await SaveChangesAsyncInternal();

            return hasChanges;
        }

        private async Task<Boolean> UpdateEndToDHCPv6LeaseEntry(DHCPv6ScopeRelatedEvent e, ReasonToEndLease reason) =>

             await UpdateLastestDHCPv6LeaseEntry(e, (leaseEntry) =>
             {
                 leaseEntry.End = e.Timestamp;
                 leaseEntry.EndReason = reason;
                 leaseEntry.IsActive = false;
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
                    TracingStreamLifeTime = TimeSpan.FromDays(7),
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


        private async Task<Boolean> DeleteDHCPv6EntriesBasedOnTimestampAndEventType(DateTime leaseThreshold, Boolean isLsesed)
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

        private async Task<Boolean> DeleteDHCPv4EntriesBasedOnTimestampAndEventType(DateTime leaseThreshold, Boolean isLsesed)
        {
            if (isLsesed == false)
            {
                var statisticEntriesToRemove = await DHCPv4PacketEntries.AsQueryable().Where(x => x.Timestamp < leaseThreshold).ToListAsync();
                DHCPv4PacketEntries.RemoveRange(statisticEntriesToRemove);
            }
            else
            {
                var leaseEntriesToRmeove = await DHCPv4LeaseEntries.AsQueryable().Where(x => x.Timestamp < leaseThreshold).ToListAsync();
                DHCPv4LeaseEntries.RemoveRange(leaseEntriesToRmeove);
            }

            return await SaveChangesAsyncInternal();
        }

        public async Task<Boolean> DeleteLeaseRelatedEventsOlderThan(DateTime leaseThreshold)
        {
            Boolean result =
                await DeleteDHCPv6EntriesBasedOnTimestampAndEventType(leaseThreshold, true) &&
                await DeleteDHCPv4EntriesBasedOnTimestampAndEventType(leaseThreshold, true);

            return result;
        }

        public async Task<Boolean> DeletePacketHandledEventsOlderThan(DateTime handledEventThreshold)
        {
            Boolean result =
              await DeleteDHCPv6EntriesBasedOnTimestampAndEventType(handledEventThreshold, false) &&
              await DeleteDHCPv4EntriesBasedOnTimestampAndEventType(handledEventThreshold, false);

            return result;
        }

        public async Task<Boolean> DeletePacketHandledEventMoreThan(UInt32 threshold)
        {
            {
                Int32 statisticsDiff = await DHCPv6PacketEntries.AsQueryable().CountAsync() - (Int32)threshold;
                if (statisticsDiff > 0)
                {
                    var statisticEntriesToRemove = await DHCPv6PacketEntries.AsQueryable().OrderBy(x => x.Timestamp).Take(statisticsDiff).ToListAsync();
                    DHCPv6PacketEntries.RemoveRange(statisticEntriesToRemove);
                }
            }
            {
                Int32 statisticsDiff = await DHCPv4PacketEntries.AsQueryable().CountAsync() - (Int32)threshold;
                if (statisticsDiff > 0)
                {
                    var statisticEntriesToRemove = await DHCPv4PacketEntries.AsQueryable().OrderBy(x => x.Timestamp).Take(statisticsDiff).ToListAsync();
                    DHCPv4PacketEntries.RemoveRange(statisticEntriesToRemove);
                }
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
            DateTime now = DateTime.UtcNow.SetKindUtc();

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
                        ExpectedRebindingAt = x.EndOfPreferredLifetime,
                        IsActive = x.IsActive,
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
                        ExpectedRebindingAt = x.EndOfPreferredLifetime,
                        IsActive = x.IsActive,
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

            end ??= DateTime.UtcNow.SetKindUtc();
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
                Timestamp = DateTime.UtcNow.SetKindUtc(),

                RequestSize = packet.GetSize(),
                RequestDestination = packet.Header.Destionation.ToString(),
                RequestSource = packet.Header.Source.ToString(),
                RequestStream = packet.GetAsStream(),
                RequestType = packet.GetInnerPacket().PacketType,

                Id = Guid.NewGuid(),
            };

            modifier?.Invoke(entry);

            entry.SetTimestampDates();
            entry.UpgradeToVersion2();

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
                Timestamp = DateTime.UtcNow.SetKindUtc(),

                RequestSize = packet.GetSize(),
                RequestDestination = packet.Header.Destionation.ToString(),
                RequestSource = packet.Header.Source.ToString(),
                RequestStream = packet.GetAsStream(),
                RequestType = packet.MessageType,

                Id = Guid.NewGuid(),
            };
            modifier?.Invoke(entry);

            entry.SetTimestampDates();
            entry.UpgradeToVersion2();

            DHCPv4PacketEntries.Add(entry);
            return await SaveChangesAsyncInternal();
        }

        public async Task<Boolean> LogInvalidDHCPv4Packet(DHCPv4Packet packet) => await AddDHCPv4PacketHandledEntryDataModel(packet, (x) => x.InvalidRequest = true);
        public async Task<Boolean> LogFilteredDHCPv4Packet(DHCPv4Packet packet, String filterName) => await AddDHCPv4PacketHandledEntryDataModel(packet, (x) => x.FilteredBy = filterName);

        private async Task<IList<TEntry>> GetPacketsFromHandledEvents<TEntry, TDHCPMessagesTypes>(IQueryable<IPacketHandledEntry<TDHCPMessagesTypes>> input, Int32 amount, Guid? scopeId, Func<IPacketHandledEntry<TDHCPMessagesTypes>, TEntry> activator)
            where TDHCPMessagesTypes : struct
        {
            input = input.Where(x => x.FilteredBy == null && x.InvalidRequest == false);

            if (scopeId.HasValue == true)
            {
                input = input.Where(x => x.ScopeId == scopeId.Value);
            }

            var entries = await input.OrderByDescending(x => x.Timestamp).Take(amount)
               .ToListAsync();

            List<TEntry> result = new(entries.Count);

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

        public IEnumerable<Device> GetAllDevices()
        {
            var result = Devices.ToList().Select(x => new Device
            {
                Id = x.Id,
                Name = x.Name,
                DUID = x.DUID == null ? new UUIDDUID(Guid.Empty) : DUIDFactory.GetDUID(x.DUID),
                MacAddress = x.MacAddress,
                LinkLocalAddress = IPv6Address.GetAsLinkLocal(x.MacAddress),
            }).OrderBy(x => x.Name).ToList();

            return result;
        }

        //public async Task<Boolean> AddTracingStream(TracingStream stream)
        //{
        //    TracingStreamDataModel dataModel = new(stream);
        //    TracingStreamEntryDataModel entry = new(stream, dataModel);

        //    TracingStreams.Add(dataModel);
        //    TracingStreamRecords.Add(entry);

        //    return await SaveChangesAsyncInternal();
        //}

        //public async Task<Boolean> AddTracingRecord(TracingRecord record)
        //{
        //    TracingStreamDataModel dataModel = await ((IQueryable<TracingStreamDataModel>)TracingStreams).FirstOrDefaultAsync(x => x.Id == record.StreamId);
        //    if (dataModel == null) { return false; }

        //    dataModel.RecordCount += 1;
        //    if (record.Status == TracingRecordStatus.Error)
        //    {
        //        dataModel.ResultType = (Int32)TracingRecordStatus.Error;
        //    }
        //    else if (record.Status == TracingRecordStatus.Success)
        //    {
        //        dataModel.ResultType = (Int32)TracingRecordStatus.Success;
        //    }

        //    TracingStreamEntryDataModel entry = new(record, dataModel);
        //    TracingStreamRecords.Add(entry);

        //    return await SaveChangesAsyncInternal();
        //}

        //public async Task<Boolean> CloseTracingStream(Guid streamId)
        //{
        //    TracingStreamDataModel dataModel = await ((IQueryable<TracingStreamDataModel>)TracingStreams).FirstOrDefaultAsync(x => x.Id == streamId);
        //    if (dataModel == null) { return false; }

        //    dataModel.ClosedAt = DateTime.UtcNow.SetKindUtc();
        //    return await SaveChangesAsyncInternal();
        //}

        public async Task<Boolean> RemoveTracingStreamsOlderThan(DateTime tracingStreamThreshold)
        {
            var streams = await ((IQueryable<TracingStreamDataModel>)TracingStreams).Where(x => x.CreatedAt <= tracingStreamThreshold).ToListAsync();
            var entries = await ((IQueryable<TracingStreamEntryDataModel>)TracingStreamRecords).Where(x => x.Stream.CreatedAt <= tracingStreamThreshold).ToListAsync();

            TracingStreamRecords.RemoveRange(entries);
            await SaveChangesAsyncInternal();

            TracingStreams.RemoveRange(streams);
            await SaveChangesAsyncInternal();

            return true;
        }

        public async Task<FilteredResult<TracingStreamOverview>> GetTracingOverview(FilterTracingRequest request)
        {
            IQueryable<TracingStreamDataModel> streams = TracingStreams;

            if (request.ModuleIdentifier.HasValue == true)
            {
                streams = streams.Where(x => x.SystemIdentifier == request.ModuleIdentifier.Value);
            }
            if (request.ProcedureIdentifier.HasValue == true)
            {
                streams = streams.Where(x => x.ProcedureIdentifier == request.ProcedureIdentifier.Value);
            }
            if (request.StartedBefore.HasValue == true)
            {
                streams = streams.Where(x => x.CreatedAt >= request.StartedBefore.Value);
            }
            if (request.EntitiyId.HasValue == true)
            {
                streams = streams.Where(x => x.Entries.Any(y => y.EntityId == request.EntitiyId.Value) == true);
            }

            Int32 total = await streams.CountAsync();

            streams = streams.OrderByDescending(x => x.CreatedAt).Skip(request.Start).Take(request.Amount);

            var result = await streams.Select(x => new TracingStreamOverview
            {
                Id = x.Id,
                IsInProgress = x.ClosedAt.HasValue == false,
                Timestamp = x.CreatedAt,
                RecordAmount = x.RecordCount,
                ProcedureIdentifier = x.ProcedureIdentifier,
                ModuleIdentifier = x.SystemIdentifier,
                FirstEntryData = x.FirstEntryData,
                Status = (TracingRecordStatusForResponses)x.ResultType,
            }).ToListAsync();

            return new(result, total);
        }

        public async Task<IEnumerable<TracingStreamRecord>> GetTracingStreamRecords(Guid traceid, Guid? entityId)
        {
            IQueryable<TracingStreamEntryDataModel> preResult = ((IQueryable<TracingStreamEntryDataModel>)TracingStreamRecords).Where(x => x.StreamId == traceid);
            if (entityId.HasValue == true)
            {
                preResult.Where(x => x.EntityId == entityId.Value);
            }

            var result = await preResult.OrderBy(x => x.Timestamp).Select(x => new TracingStreamRecord
            {
                Identifier = x.Identifier,
                AddtionalData = x.AddtionalData,
                EntityId = x.EntityId,
                Timestamp = x.Timestamp,
                Status = (TracingRecordStatusForResponses)x.ResultType,
            }).ToListAsync();

            return result;
        }

        private IQueryable<IPacketHandledEntry<TPacketType>> FilterPackets<TPacketType>(
            IQueryable<IPacketHandledEntry<TPacketType>> items, PacketMonitorRequest.V1.IPacketFilter<TPacketType> filter)
            where TPacketType : struct
        {
            if (filter.From.HasValue == true)
            {
                items = items.Where(x => x.Timestamp >= filter.From.Value);
            }
            if (filter.To.HasValue == true)
            {
                items = items.Where(x => x.Timestamp <= filter.To.Value);
            }
            if (String.IsNullOrEmpty(filter.MacAddress) == false)
            {
                String searchPattern = $"%{filter.MacAddress.Replace(".", String.Empty).Replace(":", String.Empty).ToUpper()}%";
                items = items.Where(x => EF.Functions.Like(x.MacAddress, searchPattern));
            }
            if (String.IsNullOrEmpty(filter.RequestedIp) == false)
            {
                items = items.Where(x => EF.Functions.Like(x.RequestedAddress, $"%{filter.RequestedIp.ToLower()}%"));
            }
            if (String.IsNullOrEmpty(filter.LeasedIp) == false)
            {
                items = items.Where(x => EF.Functions.Like(x.LeasedAddressInResponse, $"%{filter.LeasedIp.ToLower()}%"));
            }
            if (filter.Filtered.HasValue == true)
            {
                items = items.Where(x => string.IsNullOrEmpty(x.FilteredBy) == !filter.Filtered);
            }
            if (filter.Invalid.HasValue == true)
            {
                items = items.Where(x => x.InvalidRequest == filter.Invalid.Value);
            }
            if (String.IsNullOrEmpty(filter.SourceAddress) == false)
            {
                items = items.Where(x => EF.Functions.Like(x.RequestSource, $"%{filter.SourceAddress.ToLower()}%"));
            }
            if (String.IsNullOrEmpty(filter.DestinationAddress) == false)
            {
                items = items.Where(x => EF.Functions.Like(x.ResponseDestination, $"%{filter.DestinationAddress.ToLower()}%"));
            }

            if (filter.ScopeId.HasValue == true)
            {
                items = items.Where(x => x.ScopeId == filter.ScopeId.Value);
            }

            if (filter.RequestMessageType.HasValue == true)
            {
                items = items.Where(x => x.RequestType.Equals(filter.RequestMessageType.Value));
            }

            if (filter.ResponseMessageType.HasValue == true)
            {
                items = items.Where(x => x.ResponseType.HasValue == true && x.ResponseType.Value.Equals(filter.ResponseMessageType.Value));
            }

            if (filter.HasAnswer.HasValue == true)
            {
                items = items.Where(x => x.ResponseType.HasValue == true);
            }

            if (filter.ResultCode.HasValue == true)
            {
                items = items.Where(x => x.ErrorCode == filter.ResultCode.Value);
            }

            items = items.OrderByDescending(x => x.Timestamp);

            return items;

        }

        public async Task<FilteredResult<PacketMonitorResponses.V1.DHCPv4PacketOverview>> GetDHCPv4Packet(PacketMonitorRequest.V1.DHCPv4PacketFilter filter)
        {
            IQueryable<IPacketHandledEntry<DHCPv4MessagesTypes>> items = DHCPv4PacketEntries;
            items = FilterPackets(items, filter);

            int total = await items.CountAsync();
            var result = await items.Cast<DHCPv4PacketHandledEntryDataModel>().Select(x => new DHCPv4PacketOverview
            {
                DestinationAddress = x.ResponseDestination ?? x.RequestDestination,
                Filtered = String.IsNullOrEmpty(x.FilteredBy) == false,
                Id = x.Id,
                Invalid = x.InvalidRequest,
                LeasedIp = x.LeasedAddressInResponse,
                MacAddress = x.MacAddress,
                RequestedIp = x.RequestedAddress,
                RequestMessageType = x.RequestType,
                ResponseMessageType = x.ResponseType,
                ResultCode = x.ErrorCode,
                Scope = x.ScopeId == null ? null : new PacketMonitorResponses.V1.ScopeOverview
                {
                    Id = x.ScopeId.Value,
                },
                SourceAddress = x.RequestSource,
                Timestamp = x.Timestamp,
                RequestSize = x.RequestSize,
                ResponseSize = x.ResponseSize.HasValue == true ? x.ResponseSize.Value : 0
            }).Skip(filter.Start).Take(filter.Amount).ToListAsync();

            return new FilteredResult<PacketMonitorResponses.V1.DHCPv4PacketOverview>
            {
                Result = result,
                Total = total,
            };
        }

        public async Task<FilteredResult<PacketMonitorResponses.V1.DHCPv6PacketOverview>> GetDHCPv6Packet(PacketMonitorRequest.V1.DHCPv6PacketFilter filter)
        {
            IQueryable<IPacketHandledEntry<DHCPv6PacketTypes>> items = DHCPv6PacketEntries;
            items = FilterPackets(items, filter);

            var castedItems = items.Cast<DHCPv6PacketHandledEntryDataModel>();
            if (String.IsNullOrEmpty(filter.LeasedPrefix) == false)
            {
                castedItems = castedItems.Where(x => EF.Functions.Like(x.LeaseddPrefixCombined, $"&{filter.LeasedPrefix}&"));
            }
            if (String.IsNullOrEmpty(filter.RequestedPrefix) == false)
            {
                castedItems = castedItems.Where(x => EF.Functions.Like(x.RequestedPrefixCombined, $"&{filter.LeasedPrefix}&"));
            }

            int total = await castedItems.CountAsync();
            var result = await castedItems.Select(x => new DHCPv6PacketOverview
            {
                DestinationAddress = x.ResponseDestination ?? x.RequestDestination,
                Filtered = String.IsNullOrEmpty(x.FilteredBy) == false,
                Id = x.Id,
                Invalid = x.InvalidRequest,
                LeasedIp = x.LeasedAddressInResponse,
                MacAddress = x.MacAddress,
                RequestedIp = x.RequestedAddress,
                RequestMessageType = x.RequestType,
                ResponseMessageType = x.ResponseType,
                RequestedPrefix = x.RequestedPrefixLength == 0 ? null : new DHCPv6PrefixModel
                {
                    Length = x.RequestedPrefixLength,
                    Network = x.RequestedPrefix,
                },
                LeasedPrefix = x.LeasedPrefixLength == 0 ? null : new DHCPv6PrefixModel
                {
                    Length = x.LeasedPrefixLength,
                    Network = x.LeasedPrefix,
                },
                ResultCode = x.ErrorCode,
                Scope = x.ScopeId == null ? null : new PacketMonitorResponses.V1.ScopeOverview
                {
                    Id = x.ScopeId.Value,
                },
                SourceAddress = x.RequestSource,
                Timestamp = x.Timestamp,
                RequestSize = x.RequestSize,
                ResponseSize = x.ResponseSize.HasValue == true ? x.ResponseSize.Value : 0
            }).Skip(filter.Start).Take(filter.Amount).ToListAsync();

            return new FilteredResult<DHCPv6PacketOverview>
            {
                Result = result,
                Total = total,
            };
        }

        public async Task<PacketInfo> GetDHCPv6PacketRequestDataById(Guid packetEnrtyId)
        {
            IQueryable<DHCPv6PacketHandledEntryDataModel> items = DHCPv6PacketEntries;

            var query = items.Where(x => x.Id == packetEnrtyId).Select(x => new PacketInfo
            {
                Content = x.RequestStream,
                Source = x.RequestSource,
                Destination = x.RequestDestination,
            });

            return await query.FirstOrDefaultAsync();

        }

        public async Task<PacketInfo> GetDHCPv6PacketResponseDataById(Guid packetEnrtyId)
        {
            IQueryable<DHCPv6PacketHandledEntryDataModel> items = DHCPv6PacketEntries;

            var query = items.Where(x => x.Id == packetEnrtyId).Select(x => new PacketInfo
            {
                Content = x.ResponseStream,
                Source = x.ResponseSource,
                Destination = x.ResponseDestination,
            });

            return await query.FirstOrDefaultAsync();
        }

        public async Task<PacketInfo> GetDHCPv4PacketRequestDataById(Guid packetEnrtyId)
        {
            IQueryable<DHCPv4PacketHandledEntryDataModel> items = DHCPv4PacketEntries;

            var query = items.Where(x => x.Id == packetEnrtyId).Select(x => new PacketInfo
            {
                Content = x.RequestStream,
                Source = x.RequestSource,
                Destination = x.RequestDestination,
            });

            return await query.FirstOrDefaultAsync();
        }

        public async Task<PacketInfo> GetDHCPv4PacketResponseDataById(Guid packetEnrtyId)
        {
            IQueryable<DHCPv4PacketHandledEntryDataModel> items = DHCPv4PacketEntries;

            var query = items.Where(x => x.Id == packetEnrtyId).Select(x => new PacketInfo
            {
                Content = x.ResponseStream,
                Source = x.ResponseSource,
                Destination = x.ResponseDestination,
            });

            return await query.FirstOrDefaultAsync();
        }

        private async Task<FilteredResult<CommenResponses.V1.LeaseEventOverview>> GetLeaseEvents(DateTime? startDate, DateTime? endDate, string ipAddress, IEnumerable<Guid> scopeIds, int start, int amount, string eventName)
        {
            var items = ((IQueryable<LeaseEventEntryDataModel>)LeaseEventEntries).Where(x => x.EventType.Contains(eventName));

            if (startDate.HasValue == true)
            {
                items = items.Where(x => x.Timestamp >= startDate.Value);
            }
            if (endDate.HasValue == true)
            {
                items = items.Where(x => x.Timestamp <= endDate.Value);
            }
            if (String.IsNullOrEmpty(ipAddress) == false)
            {
                items = items.Where(x => EF.Functions.Like(x.Address, $"%{ipAddress}%"));
            }
            if (scopeIds.Any() == true)
            {
                items = items.Where(x => scopeIds.Contains(x.ScopeId));
            }

            items = items.OrderByDescending(x => x.Timestamp);

            Int32 total = await items.CountAsync();

            var result = await items.Skip(start).Take(amount).ToArrayAsync();
            return new FilteredResult<CommenResponses.V1.LeaseEventOverview>(result.Select(x => new CommenResponses.V1.LeaseEventOverview
            {
                Address = x.Address,
                EventData = x.EventData,
                EventName = x.EventType,
                EventType = x.FullEventType,
                HasResponsePacket = x.HasResponse,
                PacketHandledId = x.PacketHandledEntryId,
                Scope = new()
                {
                    Id = x.ScopeId,
                },
                Timestamp = x.Timestamp,
            }).ToArray(), total);
        }

        public async Task<FilteredResult<CommenResponses.V1.LeaseEventOverview>> GetDHCPv6LeaseEvents(DateTime? startDate, DateTime? endDate, string ipAddress, IEnumerable<Guid> scopeIds, int start, int amount) =>
            await GetLeaseEvents(startDate, endDate, ipAddress, scopeIds, start, amount, "DHCPv6");

        public async Task<FilteredResult<CommenResponses.V1.LeaseEventOverview>> GetDHCPv4LeaseEvents(DateTime? startDate, DateTime? endDate, string ipAddress, IEnumerable<Guid> scopeIds, int start, int amount) =>
            await GetLeaseEvents(startDate, endDate, ipAddress, scopeIds, start, amount, "DHCPv4");

        public async Task<IDictionary<PacketStatisticTimePeriod, IncomingAndOutgoingPacketStatisticItem>> GetIncomingAndOutgoingPacketAmount(Guid scopeId, DateTime referenceTime)
        {
            IQueryable<DHCPv4PacketHandledEntryDataModel> packetOverviews = DHCPv4PacketEntries;
            var hourBefore = referenceTime.AddHours(-1);
            var dayBefore = referenceTime.AddDays(-1);
            var weekBefore = referenceTime.AddDays(-7);

            var items = await packetOverviews.Where(x => x.ScopeId == scopeId && x.Timestamp >= weekBefore && x.Timestamp <= referenceTime)
                .Select(x => new
                {
                    Timestamp = x.Timestamp,
                    RequestSize = x.RequestSize,
                    ResponseSize = x.ResponseSize
                }).ToArrayAsync();

            if (items.Length == 0)
            {
                IQueryable<DHCPv6PacketHandledEntryDataModel> dhcpv6PacketOverview = DHCPv6PacketEntries;
                items = await dhcpv6PacketOverview.Where(x => x.ScopeId == scopeId && x.Timestamp >= weekBefore && x.Timestamp <= referenceTime)
                .Select(x => new
                {
                    Timestamp = x.Timestamp,
                    RequestSize = x.RequestSize,
                    ResponseSize = x.ResponseSize
                }).ToArrayAsync();
            }

            var lastHourInfo = new IncomingAndOutgoingPacketStatisticItem();
            var lastDayInfo = new IncomingAndOutgoingPacketStatisticItem();
            var lastWeekInfo = new IncomingAndOutgoingPacketStatisticItem();
            foreach (var item in items)
            {
                IncomingAndOutgoingPacketStatisticItem statisticItem = lastWeekInfo;
                if (item.Timestamp >= hourBefore)
                {
                    statisticItem = lastHourInfo;
                }
                else if (item.Timestamp >= dayBefore)
                {
                    statisticItem = lastDayInfo;
                }

                statisticItem.IncomingPacketAmount += 1;
                statisticItem.IncomingPacketTotalSize += item.RequestSize;

                if (item.ResponseSize.HasValue == true)
                {
                    statisticItem.OutgoingPacketAmount += 1;
                    statisticItem.OutgoingPacketTotalSize += item.ResponseSize.Value;
                }
            }

            return new Dictionary<PacketStatisticTimePeriod, IncomingAndOutgoingPacketStatisticItem>()
            {
                { PacketStatisticTimePeriod.LastHour, lastHourInfo},
                { PacketStatisticTimePeriod.LastDay, lastDayInfo},
                { PacketStatisticTimePeriod.LastWeek, lastWeekInfo},
            };
        }

        private IQueryable<T> GetLeaseEntries<T>(IQueryable<T> leaseEntries, IEnumerable<Guid> scopeIds, DateTime pointOfView)
            where T : ILeaseEntry
        {
            leaseEntries = leaseEntries.Where(x => scopeIds.Contains(x.ScopeId) == true &&
             pointOfView >= x.Start && pointOfView <= x.End);

            return leaseEntries;
        }

        public async Task<IEnumerable<DHCPv6LeaseOverview>> GetDHCPv6LeasesOverview(IEnumerable<Guid> scopeIds, DateTime pointOfView)
        {
            var leaseEntries = GetLeaseEntries(DHCPv6LeaseEntries, scopeIds, pointOfView);

            var result = await leaseEntries.Select(x => new DHCPv6LeaseOverview
            {
                Id = x.LeaseId,
                Address = x.Address,
                ExpectedEnd = x.End,
                Started = x.Start,
                ReboundTime = x.EndOfPreferredLifetime,
                RenewTime = x.EndOfRenewalTime,
                ClientIdentifier = x.ClientIdentifier,
                Prefix = x.PrefixLength == 0 ? null : new PrefixOverview
                {
                    Address = x.Prefix,
                    Mask = x.PrefixLength,
                },
                Scope = new CommenResponses.V1.ScopeOverview
                {
                    Id = x.ScopeId,
                },
                UniqueIdentifier = x.UniqueIdentifier,
                State = x.IsActive == true ? Core.Scopes.LeaseStates.Active : Core.Scopes.LeaseStates.Pending
            }).ToListAsync();

            return result;
        }

        public async Task<IEnumerable<DHCPv4LeaseOverview>> GetDHCPv4LeasesOverview(IEnumerable<Guid> scopeIds, DateTime pointOfView)
        {
            var leaseEntries = GetLeaseEntries(DHCPv4LeaseEntries, scopeIds, pointOfView);

            leaseEntries = leaseEntries.OrderBy(x => x.OrderNumber);

            var result = await leaseEntries.Select(x => new DHCPv4LeaseOverview
            {
                Id = x.LeaseId,
                Address = x.Address,
                ExpectedEnd = x.End,
                Started = x.Start,
                MacAddress = x.ClientMacAddress,
                ReboundTime = x.EndOfPreferredLifetime,
                RenewTime = x.EndOfRenewalTime,
                Scope = new CommenResponses.V1.ScopeOverview
                {
                    Id = x.ScopeId,
                },
                UniqueIdentifier = x.UniqueIdentifier,
                State = x.IsActive == true ? Core.Scopes.LeaseStates.Active : Core.Scopes.LeaseStates.Pending
            }).ToArrayAsync();

            return result;


        }

        public async Task<IEnumerable<(Guid, PrefixBinding)>> GetActiveDHCPv6Prefixes()
        {
            var preResult = await ((IQueryable<DHCPv6LeaseEntryDataModel>)DHCPv6LeaseEntries).Where(x => x.IsActive == true && x.PrefixLength > 0).Select(x => new
            {
                Address = x.Address,
                Prefix = x.Prefix,
                Length = x.PrefixLength,
                ScopeId = x.ScopeId,
            }).ToListAsync();

            return preResult.Select(x =>
            {
                try
                {
                    return (x.ScopeId, new PrefixBinding(IPv6Address.FromString(x.Prefix), new IPv6SubnetMaskIdentifier(x.Length), IPv6Address.FromString(x.Address)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Couldn't parse {x.Address} {x.Prefix}/{x.Length} into a binding");
                    Console.WriteLine(ex.ToString());
                    return (Guid.Empty, null);
                }
            }).ToArray();
        }
    }
}

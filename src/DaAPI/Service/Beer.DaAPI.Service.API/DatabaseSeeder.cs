using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Infrastructure.StorageEngine;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents.DHCPv6DeclineHandledEvent;

namespace Beer.DaAPI.Service.API
{
    public class DatabaseSeeder
    {
        private Byte[] GetRandomBytes(Random random, Int32 min = 5, Int32 max = 15)
        {
            Int32 length = min == max ? min : random.Next(min, max);
            Byte[] result = new byte[length];
            random.NextBytes(result);

            return result;
        }

        public async Task SeedDatabase(Boolean reset, StorageContext storageContext)
        {
            if (reset == true)
            {
                {
                    var packets = await storageContext.DHCPv6PacketEntries.AsQueryable().ToListAsync();
                    var entries = await storageContext.DHCPv6LeaseEntries.AsQueryable().ToListAsync();

                    storageContext.RemoveRange(packets);
                    storageContext.RemoveRange(entries);
                }

                {
                    var packets = await storageContext.DHCPv4PacketEntries.AsQueryable().ToListAsync();
                    var entries = await storageContext.DHCPv4LeaseEntries.AsQueryable().ToListAsync();

                    storageContext.RemoveRange(packets);
                    storageContext.RemoveRange(entries);
                }

                {
                    var entries = await storageContext.LeaseEventEntries.AsQueryable().ToListAsync();
                    storageContext.LeaseEventEntries.RemoveRange(entries);
                }

                await storageContext.SaveChangesAsync();
            }

            if (storageContext.DHCPv6PacketEntries.Count() == 0)
            {
                DateTime start = DateTime.UtcNow.AddDays(-20);
                DateTime end = DateTime.UtcNow.AddDays(20);

                Int32 diff = (Int32)(end - start).TotalMinutes;

                Random random = new Random();

                List<DHCPv6PacketHandledEntryDataModel> dhcpv6PacketEntries = new List<DHCPv6PacketHandledEntryDataModel>();
                var requestPacketTypes = new[] { DHCPv6PacketTypes.Solicit, DHCPv6PacketTypes.CONFIRM, DHCPv6PacketTypes.DECLINE, DHCPv6PacketTypes.REBIND, DHCPv6PacketTypes.RELEASE, DHCPv6PacketTypes.RENEW, DHCPv6PacketTypes.REQUEST };
                for (int i = 0; i < 30_000; i++)
                {
                    var request =

                  DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")), true, 2, IPv6Address.FromString("faf::2"), IPv6Address.FromString("fefc::23"),
                  new DHCPv6PacketOption[] {
                                new DHCPv6PacketRemoteIdentifierOption((UInt32)random.Next(), GetRandomBytes(random)),
                                new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId,GetRandomBytes(random)),
                  },
                    DHCPv6RelayPacket.AsInnerRelay(true, 1, IPv6Address.FromString("fe70::2"), IPv6Address.FromString("fecc::23"),
                     new DHCPv6PacketOption[] {
                                new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId,GetRandomBytes(random)),
                      },
                      DHCPv6Packet.AsInner(
                      (UInt16)random.Next(0, UInt16.MaxValue),
                      random.NextDouble() > 0.3 ? DHCPv6PacketTypes.Solicit : DHCPv6PacketTypes.RELEASE,
                      new DHCPv6PacketOption[]
                      {
                                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(Guid.NewGuid())),
                                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(Guid.NewGuid())),
                                    new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption((UInt32)random.Next()),
                      })));

                    var entry = new DHCPv6PacketHandledEntryDataModel
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = start.AddMinutes(random.Next(0, diff)),
                        ScopeId = Guid.NewGuid(),
                        RequestType = requestPacketTypes[random.Next(0, requestPacketTypes.Length)],
                        RequestSize = request.GetSize(),
                        RequestStream = request.GetAsStream(),
                        RequestDestination = request.Header.Destionation.ToString(),
                        RequestSource = request.Header.Source.ToString(),
                    };

                    if (random.NextDouble() > 0.8)
                    {
                        entry.FilteredBy = "something";
                    }
                    else
                    {
                        if (random.NextDouble() > 0.8)
                        {
                            entry.InvalidRequest = true;
                        }
                        else
                        {
                            if (random.NextDouble() > 0.5)
                            {
                                entry.HandledSuccessfully = true;
                                entry.ErrorCode = 0;

                                var response = DHCPv6Packet.AsOuter(
                                     new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),
                                     (UInt16)random.Next(0, UInt16.MaxValue),
                                     random.NextDouble() > 0.3 ? DHCPv6PacketTypes.REPLY : DHCPv6PacketTypes.ADVERTISE,
                                     new DHCPv6PacketOption[]
                                     {
                                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(Guid.NewGuid())),
                                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(Guid.NewGuid())),
                                    DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption.AsSuccess(
                                            (UInt16) random.Next(0, UInt16.MaxValue),TimeSpan.FromMinutes(random.Next(30,100)),TimeSpan.FromMinutes(random.Next(30,100)),IPv6Address.FromString("fe80::100"),
                                            TimeSpan.FromMinutes(random.Next(30,100)),TimeSpan.FromMinutes(random.Next(30,100))),
                                     DHCPv6PacketIdentityAssociationPrefixDelegationOption.AsSuccess((UInt32)random.Next(),TimeSpan.FromMinutes(random.Next(30,100)),TimeSpan.FromMinutes(random.Next(30,100)),
                                     (Byte)random.Next(30,68),IPv6Address.FromString("fc:12::0"),TimeSpan.FromMinutes(random.Next(30,100)),TimeSpan.FromMinutes(random.Next(30,100))),
                                    new DHCPv6PacketBooleanOption(DHCPv6PacketOptionTypes.Auth,random.NextDouble() > 0.5),
                                    new DHCPv6PacketByteOption(DHCPv6PacketOptionTypes.Preference,(Byte)random.Next(0,256)),
                                    new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                                    new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fd::1")),
                                    new DHCPv6PacketIPAddressListOption(48,new [] {IPv6Address.FromString("2001::1"), IPv6Address.FromString("2001::1") }),
                                    });

                                entry.ResponseDestination = response.Header.Destionation.ToString();
                                entry.ResponseSource = response.Header.Source.ToString();
                                entry.ResponseStream = response.GetAsStream();
                                entry.ResponseSize = response.GetSize();
                                entry.ResponseType = random.NextDouble() > 0.3 ? DHCPv6PacketTypes.REPLY : DHCPv6PacketTypes.ADVERTISE;
                            }
                            else
                            {
                                entry.HandledSuccessfully = false;
                                entry.ErrorCode = random.Next(0, 5);
                            }
                        }
                    }

                    entry.SetTimestampDates();

                    dhcpv6PacketEntries.Add(entry);
                }

                List<DHCPv4PacketHandledEntryDataModel> dhcpv4PacketEntries = new();
                var requestDHCPv4PacketTypes = new[] { DHCPv4MessagesTypes.Discover, DHCPv4MessagesTypes.Decline, DHCPv4MessagesTypes.Inform, DHCPv4MessagesTypes.Release, DHCPv4MessagesTypes.Request };
                for (int i = 0; i < 30_000; i++)
                {
                    var hwAddress = new Byte[6];
                    random.NextBytes(hwAddress);

                    var option82Value = new Byte[30];
                    random.NextBytes(option82Value);

                    var request =
                    new DHCPv4Packet(new IPv4HeaderInformation(IPv4Address.FromString("192.168.0.1"), IPv4Address.FromString("10.10.10.10")),
                        hwAddress, (UInt32)random.Next(), IPv4Address.FromString("0.0.0.0"), IPv4Address.FromString("192.168.0.5"), IPv4Address.FromString("0.0.0.0"),
                        DHCPv4PacketFlags.Unicast,
                        new DHCPv4PacketParameterRequestListOption(new DHCPv4OptionTypes[] { DHCPv4OptionTypes.NetworkTimeProtocolServers, DHCPv4OptionTypes.DNSServers, DHCPv4OptionTypes.Router, DHCPv4OptionTypes.DomainName }),
                        new DHCPv4PacketRawByteOption((Byte)DHCPv4OptionTypes.Option82, option82Value)
                        );


                    var entry = new DHCPv4PacketHandledEntryDataModel
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = start.AddMinutes(random.Next(0, diff)),
                        ScopeId = Guid.NewGuid(),
                        RequestType = requestDHCPv4PacketTypes[random.Next(0, requestDHCPv4PacketTypes.Length)],
                        RequestSize = request.GetSize(),
                        RequestStream = request.GetAsStream(),
                        RequestDestination = request.Header.Destionation.ToString(),
                        RequestSource = request.Header.Source.ToString(),
                    };

                    if (random.NextDouble() > 0.8)
                    {
                        entry.FilteredBy = "something";
                    }
                    else
                    {
                        if (random.NextDouble() > 0.8)
                        {
                            entry.InvalidRequest = true;
                        }
                        else
                        {
                            if (random.NextDouble() > 0.5)
                            {
                                entry.HandledSuccessfully = true;
                                entry.ErrorCode = 0;

                                var response = new DHCPv4Packet(new IPv4HeaderInformation(IPv4Address.FromString("10.10.10.10"), IPv4Address.FromString("192.168.0.1")),
                            hwAddress, (UInt32)random.Next(), IPv4Address.FromString("0.0.0.0"), IPv4Address.FromString("192.168.0.5"), IPv4Address.FromString("192.168.0.15"),
                            DHCPv4PacketFlags.Unicast,
                            new DHCPv4PacketAddressListOption(DHCPv4OptionTypes.DNSServers, new[] { IPv4Address.FromString("1.1.1.1"), IPv4Address.FromString("8.8.8.8") }),
                            new DHCPv4PacketAddressOption(DHCPv4OptionTypes.Router, IPv4Address.FromString("192.168.0.253"))
                            );

                                entry.ResponseDestination = response.Header.Destionation.ToString();
                                entry.ResponseSource = response.Header.Source.ToString();
                                entry.ResponseStream = response.GetAsStream();
                                entry.ResponseSize = response.GetSize();
                                entry.ResponseType = random.NextDouble() > 0.3 ? DHCPv4MessagesTypes.Offer : DHCPv4MessagesTypes.Acknowledge;

                            }
                            else
                            {
                                entry.HandledSuccessfully = false;
                                entry.ErrorCode = random.Next(0, 5);
                            }
                        }
                    }

                    entry.SetTimestampDates();
                    dhcpv4PacketEntries.Add(entry);
                }

                List<DHCPv6LeaseEntryDataModel> dhcpv6LeaseEntries = new List<DHCPv6LeaseEntryDataModel>();
                for (int i = 0; i < 30_000; i++)
                {
                    Byte[] addressBytes = new byte[16];
                    Byte[] prefixBytes = new byte[16];
                    random.NextBytes(addressBytes);
                    random.NextBytes(prefixBytes);

                    DHCPv6LeaseEntryDataModel entryDataModel = new DHCPv6LeaseEntryDataModel
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = start.AddMinutes(random.Next(0, diff)),
                        LeaseId = Guid.NewGuid(),
                        Address = IPv6Address.FromByteArray(addressBytes).ToString(),
                        EndReason = StatisticsControllerResponses.V1.ReasonToEndLease.Nothing,
                        ScopeId = Guid.NewGuid(),
                        Start = start.AddMinutes(random.Next(0, diff - 50)),
                    };

                    Int32 leaseDiff = (Int32)(end.AddDays(4) - entryDataModel.Start).TotalMinutes;
                    entryDataModel.End = entryDataModel.Start.AddMinutes(random.Next(10, leaseDiff));

                    TimeSpan lifetime = entryDataModel.End - entryDataModel.Start;
                    TimeSpan renewalTime = lifetime / 2;
                    TimeSpan rebindingTime = lifetime * (2.0 / 3.0);

                    entryDataModel.EndOfRenewalTime = entryDataModel.Start + renewalTime;
                    entryDataModel.EndOfPreferredLifetime = entryDataModel.Start + rebindingTime;

                    if (random.NextDouble() > 0.5)
                    {
                        entryDataModel.Prefix = IPv6Address.FromByteArray(prefixBytes).ToString();
                        entryDataModel.PrefixLength = (Byte)random.Next(48, 76);
                    }

                    dhcpv6LeaseEntries.Add(entryDataModel);
                }

                List<DHCPv4LeaseEntryDataModel> dhcpv4LeaseEntries = new();
                for (int i = 0; i < 30_000; i++)
                {
                    Byte[] addressBytes = new byte[4];
                    random.NextBytes(addressBytes);
                    var address = IPv4Address.FromByteArray(addressBytes);

                    Byte[] macADdress = new byte[6];
                    random.NextBytes(macADdress);

                    DHCPv4LeaseEntryDataModel entryDataModel = new DHCPv4LeaseEntryDataModel
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = start.AddMinutes(random.Next(0, diff)),
                        LeaseId = Guid.NewGuid(),
                        Address = address.ToString(),
                        EndReason = StatisticsControllerResponses.V1.ReasonToEndLease.Nothing,
                        ScopeId = Guid.NewGuid(),
                        Start = start.AddMinutes(random.Next(0, diff - 50)),
                        ClientMacAddress = macADdress,
                        OrderNumber = address.GetNumericValue(),
                    };

                    Int32 leaseDiff = (Int32)(end.AddDays(4) - entryDataModel.Start).TotalMinutes;
                    entryDataModel.End = entryDataModel.Start.AddMinutes(random.Next(10, leaseDiff));

                    TimeSpan lifetime = entryDataModel.End - entryDataModel.Start;
                    TimeSpan renewalTime = lifetime / 2;
                    TimeSpan rebindingTime = lifetime * (2.0 / 3.0);

                    entryDataModel.EndOfRenewalTime = entryDataModel.Start + renewalTime;
                    entryDataModel.EndOfPreferredLifetime = entryDataModel.Start + rebindingTime;

                    dhcpv4LeaseEntries.Add(entryDataModel);
                }

              
           

                storageContext.AddRange(dhcpv6PacketEntries);
                storageContext.AddRange(dhcpv6LeaseEntries);

                storageContext.AddRange(dhcpv4PacketEntries);
                storageContext.AddRange(dhcpv4LeaseEntries);

                storageContext.SaveChanges();

                Guid[] scopeGuids = new[] { Guid.Parse("159b9e07-181c-42f8-9643-1030d630bf68"), Guid.Parse("6321f65e-aa81-499c-b4f7-c58074235ae9"), Guid.Parse("f858a664-ed84-4eaa-aba5-90e148935040") };
                Guid[] leases = dhcpv6LeaseEntries.Select(x => x.LeaseId).Take(100).Union(dhcpv4LeaseEntries.Select(x => x.LeaseId).Take(100)).ToArray();

                Int32 projectEventCylces = 500;
                for (int i = 0; i < projectEventCylces; i++)
                {
                    Guid scopeId = scopeGuids[random.Next(0, scopeGuids.Length)];
                    Guid leaseId = leases[random.Next(0, leases.Length)];

                    DHCPv4LeaseCreatedEvent createdEvent = new DHCPv4LeaseCreatedEvent
                    {
                        Address = IPv4Address.FromString("10.10.10.10"),
                        EntityId = leaseId,
                        ScopeId = scopeId,
                        StartedAt = DateTime.UtcNow.AddMinutes(-random.Next(10, 1000)),
                        RenewalTime = TimeSpan.FromHours(2),
                        PreferredLifetime = TimeSpan.FromHours(4),
                        UniqueIdentifier = GetRandomBytes(random),
                        ClientMacAddress = GetRandomBytes(random, 6, 6),
                        ClientIdenfier = DHCPv4ClientIdentifier.FromIdentifierValue("My Identifier").GetBytes(),
                        ValidUntil = DateTime.UtcNow.AddMinutes(random.Next(10, 1000)),
                    };

                    DHCPv4LeaseActivatedEvent activatedEvent = new DHCPv4LeaseActivatedEvent
                    {
                        EntityId = leaseId,
                        ScopeId = scopeId,
                        Timestamp = createdEvent.Timestamp.AddSeconds(random.Next(3, 20)),
                    };

                    DHCPv4DiscoverHandledEvent handledEvent = new DHCPv4DiscoverHandledEvent(scopeId,
                        new DHCPv4Packet(new IPv4HeaderInformation(IPv4Address.FromString("192.168.0.1"), IPv4Address.FromString("10.10.10.10")),
                        GetRandomBytes(random, 6, 6), (UInt32)random.Next(), IPv4Address.FromString("0.0.0.0"), IPv4Address.FromString("192.168.0.5"), IPv4Address.FromString("0.0.0.0"),
                        DHCPv4PacketFlags.Unicast,
                        new DHCPv4PacketParameterRequestListOption(new DHCPv4OptionTypes[] { DHCPv4OptionTypes.NetworkTimeProtocolServers, DHCPv4OptionTypes.DNSServers, DHCPv4OptionTypes.Router, DHCPv4OptionTypes.DomainName }),
                        new DHCPv4PacketRawByteOption((Byte)DHCPv4OptionTypes.Option82, GetRandomBytes(random))
                        ),
                            new DHCPv4Packet(new IPv4HeaderInformation(IPv4Address.FromString("10.10.10.10"), IPv4Address.FromString("192.168.0.1")),
                            GetRandomBytes(random, 6, 6), (UInt32)random.Next(), IPv4Address.FromString("0.0.0.0"), IPv4Address.FromString("192.168.0.5"), IPv4Address.FromString("192.168.0.15"),
                            DHCPv4PacketFlags.Unicast,
                            new DHCPv4PacketAddressListOption(DHCPv4OptionTypes.DNSServers, new[] { IPv4Address.FromString("1.1.1.1"), IPv4Address.FromString("8.8.8.8") }),
                            new DHCPv4PacketAddressOption(DHCPv4OptionTypes.Router, IPv4Address.FromString("192.168.0.253"))
                            )
                        , DHCPv4DiscoverHandledEvent.DisoverErros.NoError);



                    createdEvent.Timestamp = createdEvent.StartedAt;

                    await storageContext.Project(new DomainEvent[] { createdEvent, activatedEvent, handledEvent });

                }
            }
        }
    }
}


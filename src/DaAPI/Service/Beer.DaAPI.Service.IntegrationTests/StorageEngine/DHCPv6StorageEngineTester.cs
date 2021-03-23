using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.Resolvers;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Infrastructure.Services;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Service.Infrastructure.StorageEngine;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using EventStore.Client;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;

namespace Beer.DaAPI.Service.IntegrationTests.StorageEngine
{
    public class DHCPv6StorageEngineTester
    {
        private async Task ExecuteWithStreamErase(Random random, Func<EventStoreBasedStore, Task> executor)
        {
            String prefix = random.GetAlphanumericString();
            var settings = EventStoreClientSettings.Create("esdb://127.0.0.1:2113?tls=false");

            try
            {
                var client = new EventStoreClient(settings);

                EventStoreBasedStore store = new EventStoreBasedStore(new EventStoreBasedStoreConnenctionOptions(client, prefix));

                await executor(store);
            }
            finally
            {
               await EventStoreClientDisposer.CleanUp(prefix, settings);
            }
        }

        [Fact]
        public async Task SaveAndHydrateRootScope_AddScopes()
        {
            Random random = new Random();

            await ExecuteWithStreamErase(random, async (eventStore) =>
            {
                Mock<IDHCPv6ServerPropertiesResolver> propertyResolverMock;
                DHCPv6StorageEngine engine;
                PrepareEngine(random, eventStore, out propertyResolverMock, out engine);

                DHCPv6RootScope initialRootScope = await engine.GetRootScope();

                Guid rootScopeId = random.NextGuid();
                Guid firstChildScopeId = random.NextGuid();
                Guid secondChildScopeId = random.NextGuid();

                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address> { IPv6Address.FromString("fe80::1") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        rapitCommitEnabled: true,
                        informsAreAllowd: true,
                        supportDirectUnicast: true,
                        reuseAddressIfPossible: false,
                        acceptDecline: true,
                        t1: DHCPv6TimeScale.FromDouble(0.6),
                        t2: DHCPv6TimeScale.FromDouble(0.8),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                    },
                    Name = "Testscope",
                    Id = rootScopeId,
                });
                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                       IPv6Address.FromString("fe80::0"),
                       IPv6Address.FromString("fe80::ff"),
                       new List<IPv6Address> { IPv6Address.FromString("fe80::1") }),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                    },
                    Name = "First Child Testscope",
                    Id = firstChildScopeId,
                    ParentId = rootScopeId,
                });
                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                     IPv6Address.FromString("fe80::0"),
                     IPv6Address.FromString("fe80::ff"),
                     new List<IPv6Address> { IPv6Address.FromString("fe80::1") }),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                    },
                    Name = "Second child Testscope",
                    Id = secondChildScopeId,
                    ParentId = firstChildScopeId,
                });

                await engine.Save(initialRootScope);

                var rehydratedRoot = await engine.GetRootScope();

                Assert.Single(rehydratedRoot.GetRootScopes());
                var rehydratedRootScope = rehydratedRoot.GetRootScopes().First();
                Assert.Equal(rootScopeId, rehydratedRootScope.Id);

                Assert.Single(rehydratedRootScope.GetChildScopes());
                var rehydratedFirstLevelChild = rehydratedRootScope.GetChildScopes().First();
                Assert.Equal(firstChildScopeId, rehydratedFirstLevelChild.Id);

                Assert.Single(rehydratedFirstLevelChild.GetChildScopes());
                var rehydratedSecondLevelChild = rehydratedFirstLevelChild.GetChildScopes().First();
                Assert.Equal(secondChildScopeId, rehydratedSecondLevelChild.Id);
            });
        }

        [Fact]
        public async Task SaveAndHydrateRootScope_AddChangeAndRemoveScopes()
        {
            Random random = new Random();

            await ExecuteWithStreamErase(random, async (eventStore) =>
            {
                Mock<IDHCPv6ServerPropertiesResolver> propertyResolverMock;
                DHCPv6StorageEngine engine;
                PrepareEngine(random, eventStore, out propertyResolverMock, out engine);

                DHCPv6RootScope initialRootScope = await engine.GetRootScope();

                Guid rootScopeId = random.NextGuid();
                Guid firstChildScopeId = random.NextGuid();
                Guid secondChildScopeId = random.NextGuid();

                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address> { IPv6Address.FromString("fe80::1") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        rapitCommitEnabled: true,
                        informsAreAllowd: true,
                        supportDirectUnicast: true,
                        reuseAddressIfPossible: false,
                        acceptDecline: true,
                        t1: DHCPv6TimeScale.FromDouble(0.6),
                        t2: DHCPv6TimeScale.FromDouble(0.8),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                    },
                    Name = "Testscope",
                    Id = rootScopeId,
                });
                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                       IPv6Address.FromString("fe80::0"),
                       IPv6Address.FromString("fe80::ff"),
                       new List<IPv6Address> { IPv6Address.FromString("fe80::1") }),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                    },
                    Name = "First Child Testscope",
                    Id = firstChildScopeId,
                    ParentId = rootScopeId,
                });
                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                     IPv6Address.FromString("fe80::0"),
                     IPv6Address.FromString("fe80::ff"),
                     new List<IPv6Address> { IPv6Address.FromString("fe80::1") }),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                    },
                    Name = "Second child Testscope",
                    Id = secondChildScopeId,
                    ParentId = firstChildScopeId,
                });

                initialRootScope.UpdateParent(firstChildScopeId, null);
                initialRootScope.DeleteScope(firstChildScopeId, true);

                Assert.Single(initialRootScope.GetRootScopes());

                await engine.Save(initialRootScope);

                var rehydratedRoot = await engine.GetRootScope();

                Assert.Single(rehydratedRoot.GetRootScopes());
                var rehydratedRootScope = rehydratedRoot.GetRootScopes().First();

                Assert.Empty(rehydratedRootScope.GetChildScopes());

            });


        }

        [Fact]
        public async Task SaveAndHydrateRootScope_AddAndChangeScopes()
        {
            Random random = new Random();

            await ExecuteWithStreamErase(random, async (eventStore) =>
            {
                Mock<IDHCPv6ServerPropertiesResolver> propertyResolverMock;
                DHCPv6StorageEngine engine;
                PrepareEngine(random, eventStore, out propertyResolverMock, out engine);

                DHCPv6RootScope initialRootScope = await engine.GetRootScope();

                Guid rootScopeId = random.NextGuid();
                Guid firstChildScopeId = random.NextGuid();
                Guid secondChildScopeId = random.NextGuid();

                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address> { IPv6Address.FromString("fe80::1") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        rapitCommitEnabled: true,
                        informsAreAllowd: true,
                        supportDirectUnicast: true,
                        reuseAddressIfPossible: false,
                        acceptDecline: true,
                        t1: DHCPv6TimeScale.FromDouble(0.6),
                        t2: DHCPv6TimeScale.FromDouble(0.8),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                    },
                    Name = "Testscope",
                    Id = rootScopeId,
                });
                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                       IPv6Address.FromString("fe80::0"),
                       IPv6Address.FromString("fe80::ff"),
                       new List<IPv6Address> { IPv6Address.FromString("fe80::1") }),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                    },
                    Name = "First Child Testscope",
                    Id = firstChildScopeId,
                    ParentId = rootScopeId,
                });
                initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                     IPv6Address.FromString("fe80::0"),
                     IPv6Address.FromString("fe80::ff"),
                     new List<IPv6Address> { IPv6Address.FromString("fe80::1") }),
                    ScopeProperties = DHCPv6ScopeProperties.Empty,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                        PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                    },
                    Name = "Second child Testscope",
                    Id = secondChildScopeId,
                    ParentId = firstChildScopeId,
                });

                initialRootScope.UpdateParent(firstChildScopeId, null);

                await engine.Save(initialRootScope);

                var rehydratedRoot = await engine.GetRootScope();

                Assert.Equal(2, rehydratedRoot.GetRootScopes().Count());
                var rehydratedRootScope = rehydratedRoot.GetRootScopes().First();
                var rehydratedFirstLevelScope = rehydratedRoot.GetRootScopes().ElementAt(1);

                Assert.Equal(rootScopeId, rehydratedRootScope.Id);
                Assert.Equal(firstChildScopeId, rehydratedFirstLevelScope.Id);

                Assert.Empty(rehydratedRootScope.GetChildScopes());
                Assert.Single(rehydratedFirstLevelScope.GetChildScopes());

                var rehydratedSecondLevelChild = rehydratedFirstLevelScope.GetChildScopes().First();
                Assert.Equal(secondChildScopeId, rehydratedSecondLevelChild.Id);
            });
        }

        [Fact]
        public async Task SaveAndHydrateRootScope_AddLeases()
        {
            Random random = new Random();

            await ExecuteWithStreamErase(random, async (eventStore) =>
             {
                 Mock<IDHCPv6ServerPropertiesResolver> propertyResolverMock;
                 DHCPv6StorageEngine engine;
                 PrepareEngine(random, eventStore, out propertyResolverMock, out engine);

                 DHCPv6RootScope initialRootScope = await engine.GetRootScope();

                 Guid rootScopeId = random.NextGuid();

                 initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
                 {
                     AddressProperties = new DHCPv6ScopeAddressProperties(
                         IPv6Address.FromString("fe80::0"),
                         IPv6Address.FromString("fe80::ff"),
                         new List<IPv6Address> { IPv6Address.FromString("fe80::1") },
                         preferredLifeTime: TimeSpan.FromDays(0.5),
                         validLifeTime: TimeSpan.FromDays(1),
                         rapitCommitEnabled: true,
                         informsAreAllowd: true,
                         supportDirectUnicast: false,
                         reuseAddressIfPossible: false,
                         acceptDecline: true,
                         t1: DHCPv6TimeScale.FromDouble(0.6),
                         t2: DHCPv6TimeScale.FromDouble(0.8),
                         addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                     ScopeProperties = DHCPv6ScopeProperties.Empty,
                     ResolverInformation = new CreateScopeResolverInformation
                     {
                         Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                         PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                     },
                     Name = "Testscope",
                     Id = rootScopeId,
                 });

                 DHCPv6Packet packet = DHCPv6Packet.AsInner(14, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(Guid.NewGuid())),
                new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(15,TimeSpan.Zero,TimeSpan.Zero,new List<DHCPv6PacketSuboption>()),
            });

                 DHCPv6RelayPacket outerPacket = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(random.GetIPv6Address(), random.GetIPv6Address()),
                     true, 0, IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::2"), new DHCPv6PacketOption[]
                     {
                        new DHCPv6PacketRemoteIdentifierOption(random.NextUInt16(),new byte[]{ 1,2,3,4,5,6,7,8,9 }),
                     }, packet);


                 initialRootScope.HandleSolicit(outerPacket, propertyResolverMock.Object);

                 var firstLease = initialRootScope.GetRootScopes().First().Leases.GetAllLeases().First();

                 await engine.Save(initialRootScope);

                 var rehydratedRoot = await engine.GetRootScope();

                 Assert.Single(rehydratedRoot.GetRootScopes());

                 Assert.Single(rehydratedRoot.GetRootScopes().First().Leases.GetAllLeases());

                 var rehydratedFirstLease = rehydratedRoot.GetRootScopes().First().Leases.GetAllLeases().First();

                 Assert.Equal(firstLease.Id, rehydratedFirstLease.Id);
                 Assert.Equal(firstLease.Address, rehydratedFirstLease.Address);
                 Assert.Equal(firstLease.Start, rehydratedFirstLease.Start);
                 Assert.Equal(firstLease.End, rehydratedFirstLease.End);
                 Assert.Equal(firstLease.State, rehydratedFirstLease.State);
             });
        }

        [Fact]
        public async Task SaveAndHydrateRootScope_AddAndRemoveLeases()
        {
            Random random = new Random();

            await ExecuteWithStreamErase(random, async (eventStore) =>
          {
              Mock<IDHCPv6ServerPropertiesResolver> propertyResolverMock;
              DHCPv6StorageEngine engine;
              PrepareEngine(random, eventStore, out propertyResolverMock, out engine);

              DHCPv6RootScope initialRootScope = await engine.GetRootScope();

              Guid rootScopeId = random.NextGuid();

              initialRootScope.AddScope(new DHCPv6ScopeCreateInstruction
              {
                  AddressProperties = new DHCPv6ScopeAddressProperties(
                      IPv6Address.FromString("fe80::0"),
                      IPv6Address.FromString("fe80::ff"),
                      new List<IPv6Address> { IPv6Address.FromString("fe80::1") },
                      preferredLifeTime: TimeSpan.FromDays(0.5),
                      validLifeTime: TimeSpan.FromDays(1),
                      rapitCommitEnabled: true,
                      informsAreAllowd: true,
                      supportDirectUnicast: false,
                      reuseAddressIfPossible: false,
                      acceptDecline: true,
                      t1: DHCPv6TimeScale.FromDouble(0.6),
                      t2: DHCPv6TimeScale.FromDouble(0.8),
                      addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                  ScopeProperties = DHCPv6ScopeProperties.Empty,
                  ResolverInformation = new CreateScopeResolverInformation
                  {
                      Typename = nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                      PropertiesAndValues = new Dictionary<String, String>
                                    {
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.EnterpriseNumber), random.NextUInt16().ToString()  },
                                        { nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver.RelayAgentIndex), 0.ToString()  },
                                    }
                  },
                  Name = "Testscope",
                  Id = rootScopeId,
              });

              var clientDuid = new UUIDDUID(random.NextGuid());

              DHCPv6Packet solicitPacket = DHCPv6Packet.AsInner(14, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(15,TimeSpan.Zero,TimeSpan.Zero,new List<DHCPv6PacketSuboption>()),
            });

              DHCPv6RelayPacket outerSolicitPacket = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(random.GetIPv6Address(), random.GetIPv6Address()),
                  true, 0, IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::2"), new DHCPv6PacketOption[]
                  {
                        new DHCPv6PacketRemoteIdentifierOption(random.NextUInt16(),new byte[]{ 1,2,3,4,5,6,7,8,9 }),
                  }, solicitPacket);


              initialRootScope.HandleSolicit(outerSolicitPacket, propertyResolverMock.Object);

              var firstLease = initialRootScope.GetRootScopes().First().Leases.GetAllLeases().First();

              await engine.Save(initialRootScope);

              initialRootScope = await engine.GetRootScope();

              DHCPv6Packet releasePacket = DHCPv6Packet.AsInner(14, DHCPv6PacketTypes.RELEASE, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(15, TimeSpan.FromSeconds(random.Next()), TimeSpan.FromSeconds(random.Next()), new DHCPv6PacketSuboption[]
                    {
                     new DHCPv6PacketIdentityAssociationAddressSuboption(firstLease.Address,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                    })
            });

              DHCPv6RelayPacket outerreleasePacket = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(random.GetIPv6Address(), random.GetIPv6Address()),
               true, 0, IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::2"), new DHCPv6PacketOption[]
               {
                        new DHCPv6PacketRemoteIdentifierOption(random.NextUInt16(),new byte[]{ 1,2,3,4,5,6,7,8,9 }),
               }, releasePacket);

              initialRootScope.HandleRelease(outerreleasePacket, propertyResolverMock.Object);

              firstLease = initialRootScope.GetRootScopes().First().Leases.GetAllLeases().First();
              Assert.Equal(LeaseStates.Released, firstLease.State);

              await engine.Save(initialRootScope);

              await Task.Delay(1000);

              initialRootScope.DropUnusedLeasesOlderThan(DateTime.Now);

              Assert.Empty(initialRootScope.GetRootScopes().First().Leases.GetAllLeases());

              await engine.Save(initialRootScope);

              var rehydratedRootScope = await engine.GetRootScope();

              Assert.Empty(rehydratedRootScope.GetRootScopes().First().Leases.GetAllLeases());
          });
        }

        private static void PrepareEngine(Random random, EventStoreBasedStore store, out Mock<IDHCPv6ServerPropertiesResolver> propertyResolverMock, out DHCPv6StorageEngine engine)
        {
            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.Project(It.IsAny<IEnumerable<DomainEvent>>())).ReturnsAsync(true).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scopeResolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>();
            scopeResolverMock.Setup(x => x.PacketMeetsCondition(It.IsAny<DHCPv6Packet>())).Returns(true);

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> resolverManagerMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            resolverManagerMock.Setup(x => x.InitializeResolver(It.IsAny<CreateScopeResolverInformation>())).Returns(scopeResolverMock.Object);
            resolverManagerMock.Setup(x => x.IsResolverInformationValid(It.IsAny<CreateScopeResolverInformation>())).Returns(true);

            propertyResolverMock = new Mock<IDHCPv6ServerPropertiesResolver>(MockBehavior.Strict);
            propertyResolverMock.Setup(x => x.GetServerDuid()).Returns(new UUIDDUID(random.NextGuid()));
            propertyResolverMock.Setup(x => x.GetLeaseLifeTime()).Returns(TimeSpan.FromMinutes(2));

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6ReadStore))).Returns(readStoreMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(factoryMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6EventStore))).Returns(store);
            serviceProviderMock.Setup(x => x.GetService(typeof(IScopeResolverManager<DHCPv6Packet, IPv6Address>))).Returns(resolverManagerMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6ServerPropertiesResolver))).Returns(propertyResolverMock.Object);


            engine = new DHCPv6StorageEngine(serviceProviderMock.Object);
        }
    }
}

using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Infrastructure.FilterEngines.DHCPv4;
using Beer.DaAPI.Infrastructure.InterfaceEngines;
using Beer.DaAPI.Infrastructure.LeaseEngines.DHCPv4;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.MessageHandler;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.Services;
using Beer.DaAPI.Infrastructure.StorageEngine;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using Beer.DaAPI.Service.Infrastructure.StorageEngine;
using Beer.DaAPI.Service.IntegrationTests;
using Beer.TestHelper;
using DaAPI.IntegrationTests.Host;
using EventStore.Client;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Beer.DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;
using static Beer.DaAPI.Shared.Responses.DHCPv4InterfaceResponses.V1;

namespace DaAPI.IntegrationTests.DHCPv4Pipeline
{
    public class DHCPv4DiscoveryRequestTester : ControllerTesterBase,
    IClassFixture<CustomWebApplicationFactory<FakeStartup>>
    {
        private readonly CustomWebApplicationFactory<FakeStartup> _factory;

        public DHCPv4DiscoveryRequestTester(CustomWebApplicationFactory<FakeStartup> factory)
        {
            _factory = factory;
        }

        private HttpClient GetTestClient(String dbfileName, String eventStorePrefix)
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                String currentPath = System.IO.Path.GetFullPath(".");
                Int32 startIndex = currentPath.IndexOf("Beer.DaAPI.Service.IntegrationTests");
                String basePath = currentPath.Substring(0, startIndex) + "Beer.DaAPI.Service.API";


                builder.UseStartup<FakeStartup>();
                builder.UseContentRoot(basePath);
                builder.ConfigureTestServices(services =>
                {
                    AddFakeAuthentication(services, "Bearer");
                    AddDatabase(services, dbfileName);

                    var settings = EventStoreClientSettings.Create("esdb://127.0.0.1:2113?tls=false");
                    var client = new EventStoreClient(settings);

                    ReplaceService(services, new EventStoreBasedStoreConnenctionOptions(client, eventStorePrefix));
                });

            }).CreateClient();

            return client;
        }

        private DHCPv4InterfaceEntry FindValidInterface(DHCPv4InterfaceOverview interfaces)
        {
            foreach (var item in interfaces.Entries)
            {
                IPv4Address address = IPv4Address.FromString(item.IPv4Address);
                var bytes = address.GetBytes();

                if (bytes[0] == 127 || (bytes[0] == 169 && bytes[1] == 254))
                {
                    continue;
                }

                return item;
            }

            return null;
        }

        [Fact]
        public async Task SendAndReceive()
        {
            Random random = new Random();

            String dbName = $"{random.Next()}";
            String eventStorePrefix = random.GetAlphanumericString();

            var serviceInteractions = GetTestClient(dbName, eventStorePrefix);

            Byte[] opt82Value = random.NextBytes(10);

            try
            {
                //Get and Add Interface
                var interfacesResult = await serviceInteractions.GetAsync("/api/interfaces/dhcpv4/");
                var interfaces = await IsObjectResult<DHCPv4InterfaceOverview>(interfacesResult);

                DHCPv4InterfaceEntry usedInterface = null;
                foreach (var item in interfaces.Entries)
                {
                    try
                    {
                        var createInterfaceResult = await serviceInteractions.PostAsync("/api/interfaces/dhcpv4/", GetContent(new
                        {
                            name = "my test interface",
                            ipv4Address = item.IPv4Address,
                            interfaceId = item.PhysicalInterfaceId
                        }));

                        Guid interfaceDaAPIId = await IsObjectResult<Guid>(createInterfaceResult);
                        Assert.NotEqual(Guid.Empty, interfaceDaAPIId);
                        usedInterface = item;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                Assert.NotNull(usedInterface);

                var interfaceAddress = IPv4Address.FromString(usedInterface.IPv4Address);

                //Create scope

                var createScopeResult = await serviceInteractions.PostAsync("/api/scopes/dhcpv4/", GetContent(new
                {
                    name = "Testscope",
                    id = Guid.NewGuid(),
                    addressProperties = new
                    {
                        start = "192.168.10.1",
                        end = "192.168.10.254",
                        excludedAddresses = new[] { "192.168.10.1" },
                        preferredLifetime = TimeSpan.FromDays(0.8),
                        leaseTime = TimeSpan.FromDays(1),
                        renewalTime = TimeSpan.FromDays(0.5),
                        maskLength = 24,
                        supportDirectUnicast = true,
                        acceptDecline = true,
                        informsAreAllowd = true,
                        reuseAddressIfPossible = true,
                        addressAllocationStrategy = Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1.DHCPv4ScopeAddressPropertyReqest.AddressAllocationStrategies.Next
                    },
                    resolver = new
                    {
                        typename = nameof(DHCPv4Option82Resolver),
                        propertiesAndValues = new Dictionary<String, String>
                        {
                            { nameof(DHCPv4Option82Resolver.Value), System.Text.Json.JsonSerializer.Serialize(opt82Value)  },
                        }
                    }
                }));

                Guid scopeId = await IsObjectResult<Guid>(createScopeResult);
                Assert.NotEqual(Guid.Empty, scopeId);

                IPAddress address = new IPAddress(interfaceAddress.GetBytes());
                IPEndPoint ownEndPoint = new IPEndPoint(address, 68);
                IPEndPoint serverEndPoint = new IPEndPoint(address, 67);

                UdpClient client = new UdpClient(ownEndPoint);

                IPv4HeaderInformation headerInformation =
                    new IPv4HeaderInformation(IPv4Address.FromString(address.ToString()), IPv4Address.Broadcast);

                DHCPv4Packet packet = new DHCPv4Packet(
                headerInformation, random.NextBytes(6), (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty, DHCPv4PacketFlags.Broadcast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover),
                new DHCPv4PacketRawByteOption(82, opt82Value),
                new DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier.FromIdentifierValue("my custom client")));

                byte[] packetStream = packet.GetAsStream();
                await client.SendAsync(packetStream, packetStream.Length, serverEndPoint);

                await Task.Delay(2000);

                var content = await client.ReceiveAsync();
                Byte[] receivedBytes = content.Buffer;
                DHCPv4Packet response = DHCPv4Packet.FromByteArray(receivedBytes, new IPv4HeaderInformation(interfaceAddress, interfaceAddress));

                Assert.NotNull(response);
                Assert.True(response.IsValid);

                var serverIdentifierOption = response.GetOptionByIdentifier(DHCPv4OptionTypes.ServerIdentifier) as DHCPv4PacketAddressOption;
                Assert.NotNull(serverIdentifierOption);
                Assert.Equal(interfaceAddress, serverIdentifierOption.Address);

                var subnetOption = response.GetOptionByIdentifier(DHCPv4OptionTypes.SubnetMask) as DHCPv4PacketAddressOption;
                Assert.NotNull(subnetOption);
                Assert.Equal(IPv4Address.FromString("255.255.255.0"), subnetOption.Address);

                var clientIdentifierOption = response.GetOptionByIdentifier(DHCPv4OptionTypes.ClientIdentifier) as DHCPv4PacketClientIdentifierOption;
                Assert.NotNull(clientIdentifierOption);
                Assert.Equal("my custom client", clientIdentifierOption.Identifier.IdentifierValue);
                Assert.Equal(DUID.Empty, clientIdentifierOption.Identifier.DUID);
                Assert.Equal((UInt32)0, clientIdentifierOption.Identifier.IaId);
                Assert.Empty(clientIdentifierOption.Identifier.HwAddress);

                Assert.Equal(DHCPv4PacketFlags.Broadcast, response.Flags & DHCPv4PacketFlags.Broadcast);
            }
            finally
            {
                await EventStoreClientDisposer.CleanUp(eventStorePrefix, null);
                await DatabaseTestingUtility.DeleteDatabase(dbName);
            }
        }
    }
}

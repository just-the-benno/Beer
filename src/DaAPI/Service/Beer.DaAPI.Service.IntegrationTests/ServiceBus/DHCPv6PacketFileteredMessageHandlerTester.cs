using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using DaAPI.Host;
using Beer.DaAPI.Infrastructure.FilterEngines.DHCPv6;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.StorageEngine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Beer.DaAPI.Service.API;
using Beer.DaAPI.Service.IntegrationTests;

namespace DaAPI.IntegrationTests.ServiceBus
{
    public class DHCPv6PacketFileteredMessageHandlerTester : ServiceBusTesterBase
    {
        public DHCPv6PacketFileteredMessageHandlerTester(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task HandleInvalidPacket()
        {
            Random random = new Random();

            String dbName = $"{random.Next()}";
            var (client, serviceBus) = GetTestClient(dbName);

            try
            {
                IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                    IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1"));

                DHCPv6Packet packet = DHCPv6RelayPacket.AsOuterRelay(headerInformation,true,1, IPv6Address.FromString("fe80::3"), IPv6Address.FromString("fe80::4"),Array.Empty<DHCPv6PacketOption>(),
                    DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.ADVERTISE, new List<DHCPv6PacketOption>()));

                var message = new DHCPv6PacketArrivedMessage(packet);

                await serviceBus.Publish(message);

                await Task.Delay(2000);

                StorageContext initicalContext = DatabaseTestingUtility.GetTestDatabaseContext(dbName);

                Int32 tries = 10;
                while (tries-- > 0)
                {
                    if(initicalContext.DHCPv6PacketEntries.Count() == 1)
                    {
                        break;
                    }

                    await Task.Delay(1000);
                }

                Assert.Equal(1, await initicalContext.DHCPv6PacketEntries.AsQueryable().CountAsync());

                var firstEntry = await initicalContext.DHCPv6PacketEntries.AsQueryable().FirstAsync();
                Assert.NotEqual(Guid.Empty, firstEntry.Id);
                Assert.False(firstEntry.InvalidRequest);
                Assert.Equal(packet.GetSize(), firstEntry.RequestSize);
                Assert.Equal(DHCPv6PacketTypes.ADVERTISE, firstEntry.RequestType);
                Assert.Equal(nameof(DHCPv6PacketConsistencyFilter), firstEntry.FilteredBy);

                Assert.True((DateTime.UtcNow - firstEntry.Timestamp).TotalSeconds < 20);
                Assert.NotEqual(DateTime.MinValue, firstEntry.TimestampDay);
                Assert.NotEqual(DateTime.MinValue, firstEntry.TimestampWeek);
                Assert.NotEqual(DateTime.MinValue, firstEntry.TimestampMonth);
            }
            finally
            {
                await DatabaseTestingUtility.DeleteDatabase(dbName);
            }
        }
    }
}

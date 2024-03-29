﻿using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Infrastructure.LeaseEngines.DHCPv6;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.MessageHandler;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Beer.DaAPI.UnitTests.Infrastructure.ServiceBus.MessageHandler
{
    public class InvalidDHCPv6PacketArrivedMessageHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean storageResult)
        {
            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1"));

            DHCPv6Packet packet = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, IPv6Address.FromString("fe80::3"), IPv6Address.FromString("fe80::4"), Array.Empty<DHCPv6PacketOption>(),
                DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Unkown, new List<DHCPv6PacketOption>()));

            Mock<IDHCPv6StorageEngine> storageEngineMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageEngineMock.Setup(x => x.LogInvalidDHCPv6Packet(packet)).ReturnsAsync(storageResult);

            InvalidDHCPv6PacketArrivedMessageHandler handler = new InvalidDHCPv6PacketArrivedMessageHandler(
                storageEngineMock.Object,
                Mock.Of<ILogger<InvalidDHCPv6PacketArrivedMessageHandler>>());

            await handler.Handle(new InvalidDHCPv6PacketArrivedMessage(packet), CancellationToken.None);

            storageEngineMock.Verify();
        }
    }
}

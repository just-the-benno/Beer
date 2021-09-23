using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Infrastructure.FilterEngines.DHCPv4;
using Beer.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Beer.DaAPI.UnitTests.Infrastructure.FilterEngines.DHCPv4
{
    public class UnkownDHCPv4RequestTypePaketFilterTester
    {
        [Theory]
        [InlineData(DHCPv4MessagesTypes.Discover)]
        [InlineData(DHCPv4MessagesTypes.Decline)]
        [InlineData(DHCPv4MessagesTypes.Release)]
        [InlineData(DHCPv4MessagesTypes.Inform)]
        public async Task Filter_NotFiltered_NotRequestPaket(DHCPv4MessagesTypes messagesType)
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.FromString("10.10.10.11"), IPv4Address.Empty, IPv4Address.Empty, DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(messagesType));

            UnkownDHCPv4RequestTypePaketFilter filter = new UnkownDHCPv4RequestTypePaketFilter();

            var result = await filter.ShouldPacketBeFiltered(packet);
            Assert.False(result);
        }

        [Fact]
        public async Task Filter_NotFiltered_SelectingState()
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty, DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.ServerIdentifier, IPv4Address.FromString("10.10.10.10")),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, IPv4Address.FromString("10.10.10.11")));

            UnkownDHCPv4RequestTypePaketFilter filter = new UnkownDHCPv4RequestTypePaketFilter();

            var result = await filter.ShouldPacketBeFiltered(packet);
            Assert.False(result);
        }

        [Fact]
        public async Task Filter_NotFiltered_InitRebootState()
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty, DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, IPv4Address.FromString("10.10.10.11")));

            UnkownDHCPv4RequestTypePaketFilter filter = new UnkownDHCPv4RequestTypePaketFilter();

            var result = await filter.ShouldPacketBeFiltered(packet);
            Assert.False(result);
        }

        [Fact]
        public async Task Filter_NotFiltered_RenewingState()
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.FromString("10.10.10.11"), DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request));

            UnkownDHCPv4RequestTypePaketFilter filter = new UnkownDHCPv4RequestTypePaketFilter();

            var result = await filter.ShouldPacketBeFiltered(packet);
            Assert.False(result);
        }

        [Fact]
        public async Task Filter_NotFiltered_RebindingState()
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.Empty, IPv4Address.FromString("10.10.10.12"), IPv4Address.FromString("10.10.10.11"), DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request));

            UnkownDHCPv4RequestTypePaketFilter filter = new UnkownDHCPv4RequestTypePaketFilter();

            var result = await filter.ShouldPacketBeFiltered(packet);
            Assert.False(result);
        }

        [Fact]
        public async Task Filter_Filtered_UnknowState()
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.FromString("10.10.10.11"), DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, IPv4Address.FromString("10.10.10.11")));

            UnkownDHCPv4RequestTypePaketFilter filter = new UnkownDHCPv4RequestTypePaketFilter();

            var result = await filter.ShouldPacketBeFiltered(packet);
            Assert.True(result);
        }

    }
}

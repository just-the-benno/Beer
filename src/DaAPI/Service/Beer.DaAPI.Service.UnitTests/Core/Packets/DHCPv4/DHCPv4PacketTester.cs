using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using Xunit;
using Xunit.Sdk;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;

namespace Beer.DaAPI.UnitTests.Core.Packets.DHCPv6
{
    public class DHCPv4PacketTester
    {

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        [Fact]
        public void Blub()
        {
            String input = "0101060115430d42062c0000000000000000000000000000c2229c0202a0574ff12f00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000638253633501013d070102a0574ff12f390205003204c23763440c13524f555445524c414e434f4d313739335641573c0e4c414e434f4d20313739335641573707017903060f2bd4520d0103312f320206082697683201ff0000000000000000";
            Byte[] content = StringToByteArray(input);

            DHCPv4Packet packet = DHCPv4Packet.FromByteArray(content, new IPv4HeaderInformation(IPv4Address.FromString("192.168.10.0"), IPv4Address.FromString("192.168.10.10")));
            Assert.True(packet.IsValid);
        }

        [Fact]
        public void SameIdentifier()
        {
            String firstInput = "01010601bd5830910dc30000c2229c230000000000000000c2229c0302a0571ea5ac00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000638253633501033d070102a0571ea5ac390205000c204c414e434f4d2d53797374656d732d30303a61303a35373a31653a61353a61633c1a4c414e434f4d203137383156415720286f766572204953444e2937060103060f2bd4521201060004006f010202080006f4bd9e219dd6ff";
            Byte[] firstContent = StringToByteArray(firstInput);

            String secondInput = "0101060187b5fbe20dc40000000000000000000000000000c2229c0302a0571ea5ac00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000638253633501033d070102a0571ea5ac3902050036040ac801663204c2229c230c204c414e434f4d2d53797374656d732d30303a61303a35373a31653a61353a61633c1a4c414e434f4d203137383156415720286f766572204953444e2937060103060f2bd4521201060004006f010202080006f4bd9e219dd6ff";
            Byte[] secondContent = StringToByteArray(secondInput);

            DHCPv4Packet firstPacket = DHCPv4Packet.FromByteArray(firstContent, new IPv4HeaderInformation(IPv4Address.FromString("192.168.10.0"), IPv4Address.FromString("192.168.10.10")));
            DHCPv4Packet secondPacket = DHCPv4Packet.FromByteArray(secondContent, new IPv4HeaderInformation(IPv4Address.FromString("192.168.10.0"), IPv4Address.FromString("192.168.10.10")));
            Assert.True(firstPacket.GetClientIdentifier() == secondPacket.GetClientIdentifier());
        }

        [Fact]
        public void SameIdentifier_NoIdentifierInPacket()
        {
            String firstInput = "010106016d4ec97200000000000000000000000000000000c2229c03001a8cf0abe60000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000063825363350101370c011c02030f060c7728292a1a521201060004006f010502080006f4bd9e219dd6ff000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
            Byte[] firstContent = StringToByteArray(firstInput);

            String secondInput = "010106016d4ec9720f1a0000c2229c250000000000000000c2229c03001a8cf0abe60000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000063825363350103370c011c02030f060c7728292a1a521201060004006f010502080006f4bd9e219dd6ff000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
            Byte[] secondContent = StringToByteArray(secondInput);

            DHCPv4Packet firstPacket = DHCPv4Packet.FromByteArray(firstContent, new IPv4HeaderInformation(IPv4Address.FromString("192.168.10.0"), IPv4Address.FromString("192.168.10.10")));
            DHCPv4Packet secondPacket = DHCPv4Packet.FromByteArray(secondContent, new IPv4HeaderInformation(IPv4Address.FromString("192.168.10.0"), IPv4Address.FromString("192.168.10.10")));

            var firstIdentifier = firstPacket.GetClientIdentifier();
            var secondIdentifier = secondPacket.GetClientIdentifier();

            Assert.True(firstIdentifier == secondIdentifier);
        }

        [Fact]
        public void GetRequestType_SelectingState()
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

            var type = packet.GetRequestType();
            Assert.Equal(DHCPv4Packet.DHCPv4PacketRequestType.AnswerToOffer, type);
        }

        [Fact]
        public void GetRequestType_InitRebootState()
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty, DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, IPv4Address.FromString("10.10.10.11")));

            var type = packet.GetRequestType();
            Assert.Equal(DHCPv4Packet.DHCPv4PacketRequestType.Initializing, type);
        }

        [Fact]
        public void GetRequestType_RenewingState()
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.FromString("10.10.10.11"), DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request));

            var type = packet.GetRequestType();
            Assert.Equal(DHCPv4Packet.DHCPv4PacketRequestType.Renewing, type);
        }

        [Fact]
        public void GetRequestType_RebindingState()
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.Empty, IPv4Address.FromString("10.10.10.12"), IPv4Address.FromString("10.10.10.11"), DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request));

            var type = packet.GetRequestType();
            Assert.Equal(DHCPv4Packet.DHCPv4PacketRequestType.Rebinding, type);
        }

        [Fact]
        public void GetRequestType_UnknowState()
        {
            Random random = new Random();

            IPv4HeaderInformation header = new IPv4HeaderInformation(
                IPv4Address.FromString("10.10.10.10"),
                IPv4Address.FromString("10.10.10.11"));

            DHCPv4Packet packet = new DHCPv4Packet(header, random.NextBytes(6), random.NextUInt32(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.FromString("10.10.10.11"), DHCPv4Packet.DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, IPv4Address.FromString("10.10.10.11")));

            Assert.ThrowsAny<InvalidOperationException>(() => packet.GetRequestType());
        }

    }
}

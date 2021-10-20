using FluentAssertions;
using Moq;
using SharpShark;
using SharpShark.Packets;
using Xunit;

namespace SharpShark.UnitTests;

public class PpcapParserTester
{
    private Byte[] ParseHexStream(String input)
    {
        Byte[] result = new byte[input.Length / 2];

        for (int i = 0, j = 0; i < input.Length; i += 2, j++)
        {
            result[j] = Byte.Parse(new Char[] { input[i], input[i + 1] }, System.Globalization.NumberStyles.HexNumber);
        }

        return result;

    }

    [Fact]
    public async Task ReadFile()
    {
        Byte[] input = await File.ReadAllBytesAsync("./Assets/svr1-outer.pcap");
        PcapFile file = PcapFile.FromStream(input, "My reading", PacketStackBuilder.Default);
    }

    [Fact]
    public async Task ReadFile_DetectGenerelStructure()
    {
        Byte[] input = await File.ReadAllBytesAsync("./Assets/demo.pcap");

        var stackBuilderMock = new Mock<IPacketStackBuilder>();
        stackBuilderMock.Setup(x => x.BuildPacketStack(It.IsAny<Packet>())).Returns(Array.Empty<PacketStackInfo>());
        PcapFile file = PcapFile.FromStream(input, "My reading", stackBuilderMock.Object);
        file.Name.Should().Be("My reading");
        file.Packets.Should().HaveCount(7);

        var expectedPackets = new[]
        {
            (637680718281779840,86,"b08bd09b8100bce712658480810000678847000000ff0001c1ff8cec4b8ead69a0510b5202ba8100000b080045000026c2df00008011f3e5c0a80164c0a8014de86d0d3d00128219a101c0e805398ce0100000000000"),
            (637680718281889850,90,"b08bd09b8100bce712658480810000678847000000ff0001c1ffffffffffffffe063daec65ea8100000b08060001080006040001e063daec65eac0a801e9000000000000c0a801e6000000000000000000000000000000000000"),
            (637680718282189820,142,"01005e0000050c812625f0878100c067080045c0007cbcf30000015911000a67000ae000000502010034ac1b0b46000000040000000200000110614d9794ffff0000000512960000000f0a6700020a670003c23760fdc23760ffd893219eed2ab7e581129b5ea904f71000000009000100040000000100020014614d9794cdd471eee781b3763b1ce1151528430f"),
            (637680718283529780,80,"01005e000002bce7126584808100006708004500003e000000000111cf390a67000de000000202860286002a1f790001001ec0a833010000010000140000000004000004000f000004010004c0a83301"),
            (637680718283659780,158,"01005e000005b08bd09b81008100006708004500008c4def0000015980b20a67000ce000000502010030ac1b0b58000000040000000200000110614b2e71ffff0000000112960000000a0a67000d0a67000cac1b0b56f32b8a5148f27b4b04854d6abc5d446f0000000e0001000400000001001200040000001480000008000000090000001400020014614b2e711a36439a4d7f8334267a26ec27db6f24"),
            (637680718287029510,90,"b08bd09b8100bce712658480810000678847000000ff0001c1ff0180c2000000245ebe5416848100000a002642420300000000008000245ebe541684000000008000245ebe54168480010000140002000f000000000000000000"),
            (637680718288879390,90,"bce712658480b08bd09b8100810000678847000000ff0001b1ff000c298fa7d38cec4b8ead698100000b080600010800060400018cec4b8ead69c0a8014d000c298fa7d3c0a801f1000000000000000000000000000000000000"),
        };

        for (int i = 0; i < expectedPackets.Length; i++)
        {
            var expectedPacket = expectedPackets[i];

            file.Packets[i].Should().BeEquivalentTo(new Packet(new DateTime(expectedPacket.Item1), (UInt32)expectedPacket.Item2, (UInt32)LinkLayerHeaderTypes.Ethernet, ParseHexStream(expectedPacket.Item3)), options => options
                 .Using<Memory<Byte>>(ctx => ctx.Subject.ToArray().Should().BeEquivalentTo(ctx.Expectation.ToArray()))
                 .WhenTypeIs<Memory<Byte>>());
        }
    }

    [Fact]
    public async Task ReadFile_ParseEhternetFrame()
    {
        var builder = new PacketStackBuilder();
        builder.AddDataLinkParser(new EthernetParser());

        PcapFile file = await PcapFile.FromFile("./Assets/demo.pcap", builder);

        var firstPacket = file.Packets[2];
        firstPacket.Stack.Should().HaveCount(3);

        firstPacket.Stack[0].Should().BeAssignableTo<EthernetFrame>();

        var ethernetFrame = (EthernetFrame)firstPacket.Stack[0];
        ethernetFrame.DestionationAddress.Should().BeEquivalentTo(new[] { 0x01, 0x00, 0x5e, 0x00, 0x00, 0x05 });
        ethernetFrame.SourceAddress.Should().BeEquivalentTo(new[] { 0x0c, 0x81, 0x26, 0x25, 0xf0, 0x87 });
        ethernetFrame.Type.Should().Be(0x8100);
        ethernetFrame.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(14).ToArray());
        ethernetFrame.LowerProtocols.Should().BeEmpty();
        ethernetFrame.HigherProtocols.Should().HaveCount(2);

        firstPacket.Stack[1].Should().BeAssignableTo<EthernetDot1QFrame>();

        var dot1QFrame = (EthernetDot1QFrame)firstPacket.Stack[1];
        dot1QFrame.VlanId.Should().Be(103);
        dot1QFrame.Cos.Should().Be(6);
        dot1QFrame.Type.Should().Be(0x0800);

        dot1QFrame.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(14 + 2).ToArray());
        dot1QFrame.LowerProtocols.Should().ContainSingle();
        dot1QFrame.HigherProtocols.Should().ContainSingle();

        firstPacket.Stack[2].Should().BeAssignableTo<UnknownPacketStackInfo>();

        var dataContent = (UnknownPacketStackInfo)firstPacket.Stack[2];
        dataContent.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(6 + 6 + 2 + 2 + 2).ToArray());
        dataContent.LowerProtocols.Should().HaveCount(2);
        dataContent.HigherProtocols.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadFile_L2MPLS_WithoutComtrolWord()
    {
        var builder = new PacketStackBuilder();
        builder.AddDataLinkParser(new EthernetParser());
        builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithoutControlWord));

        PcapFile file = await PcapFile.FromFile("./Assets/demo.pcap", builder);

        var firstPacket = file.Packets[0];
        firstPacket.Stack.Should().HaveCount(7);

        //skip "outer" ethernet

        //first label explicit null
        firstPacket.Stack[2].Should().BeAssignableTo<MPLSPacket>();

        var firstLabel = (MPLSPacket)firstPacket.Stack[2];
        firstLabel.Label.Should().Be(0);
        firstLabel.ExperimentalBits.Should().Be(0);
        firstLabel.IsBottomOfStack.Should().BeFalse();
        firstLabel.TTL.Should().Be(255);

        firstLabel.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(18).ToArray());
        firstLabel.LowerProtocols.Should().HaveCount(2);
        firstLabel.HigherProtocols.Should().HaveCount(4);

        // second label 28
        firstPacket.Stack[3].Should().BeAssignableTo<MPLSPacket>();

        var secondLabel = (MPLSPacket)firstPacket.Stack[3];
        secondLabel.Label.Should().Be(28);
        secondLabel.ExperimentalBits.Should().Be(0);
        secondLabel.IsBottomOfStack.Should().BeTrue();
        secondLabel.TTL.Should().Be(255);

        secondLabel.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(22).ToArray());
        secondLabel.LowerProtocols.Should().HaveCount(3);
        secondLabel.HigherProtocols.Should().HaveCount(3);

        // inner ethernet frame
        var ethernetFrame = (EthernetFrame)firstPacket.Stack[4];
        ethernetFrame.DestionationAddress.Should().BeEquivalentTo(new[] { 0x8c, 0xec, 0x4b, 0x8e, 0xad, 0x69 });
        ethernetFrame.SourceAddress.Should().BeEquivalentTo(new[] { 0xa0, 0x51, 0x0b, 0x52, 0x02, 0xba });
        ethernetFrame.Type.Should().Be((UInt16)EthnernetPayloadTypes.Dot1q);
        ethernetFrame.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(26).ToArray());
        ethernetFrame.LowerProtocols.Should().HaveCount(4);
        ethernetFrame.HigherProtocols.Should().HaveCount(2);

        firstPacket.Stack[5].Should().BeAssignableTo<EthernetDot1QFrame>();

        var dot1QFrame = (EthernetDot1QFrame)firstPacket.Stack[5];
        dot1QFrame.VlanId.Should().Be(11);
        dot1QFrame.Cos.Should().Be(0);
        dot1QFrame.Type.Should().Be((UInt16)EthnernetPayloadTypes.IPv4);

        dot1QFrame.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(26 + 14).ToArray());
        dot1QFrame.LowerProtocols.Should().HaveCount(5);
        dot1QFrame.HigherProtocols.Should().ContainSingle();

        firstPacket.Stack[6].Should().BeAssignableTo<UnknownPacketStackInfo>();

        var dataContent = (UnknownPacketStackInfo)firstPacket.Stack[6];
        dataContent.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(26 + 14 + 4).ToArray());
        dataContent.LowerProtocols.Should().HaveCount(6);
        dataContent.HigherProtocols.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadFile_ParseIPv4Packet()
    {

        var builder = new PacketStackBuilder();
        builder.AddDataLinkParser(new EthernetParser());
        builder.AddParser(new IPv4PacketParser());

        PcapFile file = await PcapFile.FromFile("./Assets/demo.pcap", builder);

        var firstPacket = file.Packets[2];
        firstPacket.Stack.Should().HaveCount(4);

        firstPacket.Stack[0].Should().BeAssignableTo<EthernetFrame>();
        firstPacket.Stack[1].Should().BeAssignableTo<EthernetDot1QFrame>();
        firstPacket.Stack[2].Should().BeAssignableTo<IPv4Packet>();

        var ipv4Packet = (IPv4Packet)firstPacket.Stack[2];
        ipv4Packet.HeaderLenght.Should().Be(20);
        ipv4Packet.TypeOfService.Should().Be(0xc0);
        ipv4Packet.TotalLength.Should().Be(124);
        ipv4Packet.Identification.Should().Be(0xbcf3);
        ipv4Packet.DontFragement.Should().BeFalse();
        ipv4Packet.MoreFragments.Should().BeFalse();
        ipv4Packet.FragmentOffset.Should().Be(0);
        ipv4Packet.TimeToLive.Should().Be(1);
        ipv4Packet.Protocol.Should().Be(89);
        ipv4Packet.Source.ToString().Should().Be("10.103.0.10");
        ipv4Packet.Destionation.ToString().Should().Be("224.0.0.5");


        ipv4Packet.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(14 + 4).ToArray());
        ipv4Packet.LowerProtocols.Should().HaveCount(2);
        ipv4Packet.HigherProtocols.Should().ContainSingle();

        firstPacket.Stack[3].Should().BeAssignableTo<UnknownPacketStackInfo>();

        var dataContent = (UnknownPacketStackInfo)firstPacket.Stack[3];
        dataContent.Content.ToArray().Should().ContainInOrder(firstPacket.Content.Slice(14 + 4 + 20).ToArray());
        dataContent.LowerProtocols.Should().HaveCount(3);
        dataContent.HigherProtocols.Should().BeEmpty();
    }
}
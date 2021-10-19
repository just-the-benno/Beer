using FluentAssertions;
using Moq;
using SharpShark;
using SharpShark.Packets;
using Xunit;

namespace SharpShark.UnitTests;

public class PacketCollectionTester
{
    [Fact]
    public async Task Find()
    {
        var builder = new PacketStackBuilder();
        builder.AddDataLinkParser(new EthernetParser());
        builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithoutControlWord));
        builder.AddParser(new IPv4PacketParser());

        var file = await PcapFile.FromFile("./Assets/demo.pcap", builder);
        var sameFile = await PcapFile.FromFile("./Assets/demo.pcap", builder);
        var otherFile = await PcapFile.FromFile("./Assets/mpls-basic.pcap", builder);

        var foundIndex = file.Packets.Find<EthernetFrame>(sameFile.Packets[2], false, PacketCollection.PacketMatchMode.InnerMost);
        var notFoundIndex = file.Packets.Find<EthernetFrame>(otherFile.Packets[3], false, PacketCollection.PacketMatchMode.InnerMost);

        foundIndex.Should().Be(2);
        notFoundIndex.Should().BeNegative();
    }

    [Fact]
    public async Task Find_WithTrailer()
    {
        var fileWithoutTrailer = await PcapFile.FromFile("./Assets/ethernet-without-trailer.pcap", PacketStackBuilder.Default);
        var fileWithTrailer = await PcapFile.FromFile("./Assets/ethernet-with-trailer.pcap", PacketStackBuilder.Default);

        var foundIndex1 = fileWithoutTrailer.Packets.Find<EthernetFrame>(fileWithTrailer.Packets[0], false, PacketCollection.PacketMatchMode.InnerMost);
        var foundIndex2 = fileWithoutTrailer.Packets.Find<EthernetFrame>(fileWithTrailer.Packets[1], false, PacketCollection.PacketMatchMode.InnerMost);
        var foundIndex3 = fileWithoutTrailer.Packets.Find<EthernetFrame>(fileWithTrailer.Packets[2], false, PacketCollection.PacketMatchMode.InnerMost);
        var foundIndex4 = fileWithoutTrailer.Packets.Find<EthernetFrame>(fileWithTrailer.Packets[3], false, PacketCollection.PacketMatchMode.InnerMost);

        var foundIndex1_1 = fileWithTrailer.Packets.Find<EthernetFrame>(fileWithoutTrailer.Packets[0], false, PacketCollection.PacketMatchMode.InnerMost);
        var foundIndex2_1 = fileWithTrailer.Packets.Find<EthernetFrame>(fileWithoutTrailer.Packets[1], false, PacketCollection.PacketMatchMode.InnerMost);
        var foundIndex3_1 = fileWithTrailer.Packets.Find<EthernetFrame>(fileWithoutTrailer.Packets[2], false, PacketCollection.PacketMatchMode.InnerMost);
        var foundIndex4_1 = fileWithTrailer.Packets.Find<EthernetFrame>(fileWithoutTrailer.Packets[3], false, PacketCollection.PacketMatchMode.InnerMost);

        foundIndex1.Should().Be(0);
        foundIndex2.Should().Be(1);
        foundIndex3.Should().Be(2);
        foundIndex4.Should().Be(3);

        foundIndex1_1.Should().Be(0);
        foundIndex2_1.Should().Be(1);
        foundIndex3_1.Should().Be(2);
        foundIndex4_1.Should().Be(3);
    }

    [Fact]
    public async Task Find_WithIndexManipulation()
    {
        var builder = new PacketStackBuilder();
        builder.AddDataLinkParser(new EthernetParser());
        builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithoutControlWord));
        builder.AddParser(new IPv4PacketParser());

        var file = await PcapFile.FromFile("./Assets/demo.pcap", builder);
        var sameFile = await PcapFile.FromFile("./Assets/demo.pcap", builder);

        var foundIndex = file.Packets.Find<EthernetFrame>(sameFile.Packets[2], true, PacketCollection.PacketMatchMode.InnerMost);
        var notFoundIndex = file.Packets.Find<EthernetFrame>(sameFile.Packets[0], true, PacketCollection.PacketMatchMode.InnerMost);

        foundIndex.Should().Be(2);
        notFoundIndex.Should().BeNegative();

        file.Packets.ResetSearchIndex();
        var notFoundIndexSecondTry = file.Packets.Find<EthernetFrame>(sameFile.Packets[1], true, PacketCollection.PacketMatchMode.InnerMost);
        notFoundIndexSecondTry.Should().Be(1);
    }

    [Fact]
    public async Task FindLast()
    {
        var builder = new PacketStackBuilder();
        builder.AddDataLinkParser(new EthernetParser());
        builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithoutControlWord));
        builder.AddParser(new IPv4PacketParser());

        var file = await PcapFile.FromFile("./Assets/demo.pcap", builder);
        var sameFile = await PcapFile.FromFile("./Assets/demo.pcap", builder);
        var otherFile = await PcapFile.FromFile("./Assets/mpls-basic.pcap", builder);

        var foundIndex = file.Packets.FindLast<EthernetFrame>(sameFile.Packets[5], false, PacketCollection.PacketMatchMode.InnerMost);
        var notFoundIndex = file.Packets.FindLast<EthernetFrame>(otherFile.Packets[10], false, PacketCollection.PacketMatchMode.InnerMost);

        foundIndex.Should().Be(5);
        notFoundIndex.Should().BeNegative();
    }

    [Fact]
    public async Task Remove()
    {
        var builder = new PacketStackBuilder();
        builder.AddDataLinkParser(new EthernetParser());
        builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithoutControlWord));
        builder.AddParser(new IPv4PacketParser());

        var file = await PcapFile.FromFile("./Assets/demo.pcap", builder);
        var sameFile = await PcapFile.FromFile("./Assets/demo.pcap", builder);

        for (int i = 0; i < sameFile.Packets.Count; i++)
        {
            var result = file.Packets.Remove<EthernetFrame>(sameFile.Packets[0], PacketCollection.PacketMatchMode.InnerMost);
            result.Should().ContainInOrder(file.Packets.Skip(1 + i));
        }
    }

    [Fact]
    public async Task GetBetween()
    {
        var builder = new PacketStackBuilder();
        builder.AddDataLinkParser(new EthernetParser());
        builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithoutControlWord));
        builder.AddParser(new IPv4PacketParser());

        var file = await PcapFile.FromFile("./Assets/demo.pcap", builder);
        var sameFile = await PcapFile.FromFile("./Assets/demo.pcap", builder);

        var result = file.Packets.GetBetween<EthernetFrame>(sameFile.Packets[3], sameFile.Packets[5], PacketCollection.PacketMatchMode.InnerMost);
        result.Should().NotBeNull().And.HaveCount(3).And.ContainInOrder(file.Packets[3], file.Packets[4], file.Packets[5]);
    }


}
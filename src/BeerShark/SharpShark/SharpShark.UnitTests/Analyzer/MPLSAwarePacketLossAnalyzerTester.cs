using FluentAssertions;
using SharpShark;
using SharpShark.Analyzer;
using SharpShark.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SharpShark.UnitTests.Analyzer
{
    public class MPLSAwarePacketLossAnalyzerTester
    {

        [Fact]
        public async Task Process()
        {
            var builder = new PacketStackBuilder();
            builder.AddDataLinkParser(new EthernetParser());
            builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithControlWord));
            builder.AddParser(new IPv4PacketParser());

            PcapFile file1 = await PcapFile.FromFile("./Assets/packet-stream-file1.pcap", builder);
            PcapFile file2 = await PcapFile.FromFile("./Assets/packet-stream-file2.pcap", builder);
            PcapFile file3 = await PcapFile.FromFile("./Assets/packet-stream-file3.pcap", builder);

            MPLSAwarePacketLossAnalyzer analyzer = new();
            var result = analyzer.Process(new[] { file1, file2, file3 });

            result.UniquePackets.Should().Be(7);
            result.TotalSkipped.Should().Be(2);
            result.TotalLost.Should().Be(4);

            result.Traces.Should().NotBeEmpty().And.HaveCount(7);

            result.Traces.Should().BeEquivalentTo(new[]
            {
                new PacketTrace(new[] { 0,1,0}),
                new PacketTrace(new[] { 1,-1,-1}),
                new PacketTrace(new[] { 2,3,3}),
                new PacketTrace(new[] { 3,4,4}),
                new PacketTrace(new[] { 4,5,-1}),
                new PacketTrace(new[] { -1,2,1},true),
                new PacketTrace(new[] { -1,-1,2},true),
            });

            result.Files.Should().BeEquivalentTo(new[]
            {
                new PacketTraceFileInfo(file1,0,5),
                new PacketTraceFileInfo(file2,1,6),
                new PacketTraceFileInfo(file3,0,5),
            });

        }
    }
}

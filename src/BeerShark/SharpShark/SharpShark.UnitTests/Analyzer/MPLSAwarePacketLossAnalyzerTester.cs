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
        public async Task Find()
        {
            var builder = new PacketStackBuilder();
            builder.AddDataLinkParser(new EthernetParser());
            builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithoutControlWord));
            builder.AddParser(new IPv4PacketParser());

            PcapFile file1 = await PcapFile.FromFile("./Assets/AFZ-SVR1-MPLS.pcap", builder);
            PcapFile file4 = await PcapFile.FromFile("./Assets/AFZ-SVR2-MPLS.pcap", builder);

            var foundIndex = file1.Packets.Find<EthernetFrame>(file4.Packets[5], false, PacketCollection.PacketMatchMode.InnerMost);

            foundIndex.Should().Be(291);
        }

        [Fact]
        public async Task Process23()
        {
            var builder = new PacketStackBuilder();
            builder.AddDataLinkParser(new EthernetParser());
            builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithControlWord));
            builder.AddParser(new IPv4PacketParser());

            PcapFile file1 = await PcapFile.FromFile("./Assets/AFZ-SVR1-MPLS-short.pcap", builder);
            //PcapFile file2 = await PcapFile.FromFile("./Assets/AFZ-SVR1-MPLSO.pcap", builder);
            //PcapFile file3 = await PcapFile.FromFile("./Assets/AFZ-SVR2-MPLSO.pcap", builder);
            PcapFile file4 = await PcapFile.FromFile("./Assets/AFZ-SVR2-MPLS-short.pcap", builder);

            MPLSAwarePacketLossAnalyzer analyzer = new(false);
            var result = analyzer.Process(new[] { file4, file1 });

            List<String> results = new List<string>();
            for (int i = 0; i < result.Traces.Count; i++)
            {
                var trace = result.Traces[i];
                if(trace.IsTransmitted == false)
                {
                    Int64 index = i + result.Files[0].SkippedOffset + 1;
                    Int64 otherIndex = i + result.Files[1].SkippedOffset + 1;

                    results.Add($"{index} | {otherIndex}");
                }
            }

        }

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

            result.UniquePackets.Should().Be(6);
            result.TotalSkipped.Should().Be(3);
            result.TotalLost.Should().Be(3);

            result.Traces.Should().NotBeEmpty().And.HaveCount(6);

            result.Traces.Should().BeEquivalentTo(new[]
            {
                new PacketTrace(new[] { 0,1,0}),
                new PacketTrace(new[] { 1,-1,-1}),
                new PacketTrace(new[] { 2,3,3}),
                new PacketTrace(new[] { 3,4,4}),
                new PacketTrace(new[] { -1,2,1},false),
                new PacketTrace(new[] { -1,-1,2},false),
            });

            result.Files.Should().BeEquivalentTo(new[]
            {
                new PacketTraceFileInfo(file1,0,4),
                new PacketTraceFileInfo(file2,1,5),
                new PacketTraceFileInfo(file3,0,5),
            });

        }
    }
}

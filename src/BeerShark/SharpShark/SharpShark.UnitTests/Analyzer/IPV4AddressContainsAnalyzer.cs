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
    public class IPV4AddressContainsAnalyzerTester
    {
        [Fact]
        public async Task ExtractIPAddresses()
        {
            var builder = new PacketStackBuilder();
            builder.AddDataLinkParser(new EthernetParser());
            builder.AddParser(new IPv4PacketParser());
            builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithControlWord));

            PcapFile file = await PcapFile.FromFile("./Assets/demo.pcap", builder);

            var analyzer = new IPV4AddressContainsAnalyzer();

            var addresses = analyzer.Process(file.Packets);

            addresses.Should().NotBeNull();

            addresses.Should().ContainInOrder(new[]
            {
                new IPv4Address("192.168.1.100"),
                new IPv4Address("192.168.1.77"),
                new IPv4Address("10.103.0.10"),
                new IPv4Address("224.0.0.5"),
                new IPv4Address("10.103.0.13"),
                new IPv4Address("224.0.0.2"),
                new IPv4Address("10.103.0.12")
            });

        }

    }
}

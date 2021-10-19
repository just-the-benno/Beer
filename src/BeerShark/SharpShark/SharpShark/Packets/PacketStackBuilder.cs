using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class PacketStackBuilder : IPacketStackBuilder
    {
        private static PacketStackBuilder _default;

        static PacketStackBuilder()
        {
            var builder = new PacketStackBuilder();
            builder.AddDataLinkParser(new EthernetParser());
            builder.AddParser(new MPLSParser(MPLSParser.MPLSParserOptions.Layer2WithControlWord));
            builder.AddParser(new IPv4PacketParser());

            _default = builder;
        }

        public static PacketStackBuilder Default => _default;

        private readonly HashSet<IPacketParser> _parsers = new();
        private readonly HashSet<IDatalinkPacketParser> _datalinkParsers = new();

        public void AddDataLinkParser(IDatalinkPacketParser parser)
        {
            _datalinkParsers.Add(parser);
        }

        public void AddParser(IPacketParser parser)
        {
            _parsers.Add(parser);

        }

        public IReadOnlyList<PacketStackInfo> BuildPacketStack(Packet packet)
        {
            List<PacketStackInfo> packets = new();

            var levelZeroParsers = _datalinkParsers.First(x => x.CanParse(packet.DatalinkIdentifier));

            var firstStackElement = levelZeroParsers.ParsePacket(packet, packet.Content);
            packets.AddRange(firstStackElement.StackInfos);

            Int32 headerIndex = firstStackElement.HeaderLength;
            Int32 trailerIndex = packet.Content.Length;
            while (true)
            {
                var slice = packet.Content[headerIndex..trailerIndex];
                var parsers = _parsers.FirstOrDefault(x => x.CanParse(packets.Last(), slice));
                if (parsers == null)
                {
                    packets.Add(new UnknownPacketStackInfo(packet.Content[headerIndex..], packet));
                    break;
                }
                else
                {
                    var parserResult = parsers.ParsePacket(packets.Last(), slice);
                    if (parserResult.HeaderLength < 0)
                    {
                        UInt16 paddedLength = (UInt16)(-parserResult.HeaderLength);

                        //trailer detected
                        foreach (var item in packets)
                        {
                            item.ReadjustLength(paddedLength);
                        }

                        trailerIndex -= paddedLength;

                        // try parsing again
                        continue;
                    }
                    packets.AddRange(parserResult.StackInfos);

                    headerIndex += parserResult.HeaderLength;

                    if (parserResult.IsLast == true)
                    {
                        break;
                    }
                }
            }

            return packets;
        }
    }
}

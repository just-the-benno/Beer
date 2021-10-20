using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class MPLSParser : IPacketParser
    {
        public enum MPLSParserOptions
        {
            Layer2WithoutControlWord,
            Layer2WithControlWord,
            Layer3
        }

        public MPLSParserOptions Option { get; init; }

        public MPLSParser(MPLSParserOptions option)
        {
            Option = option;
        }

        public Boolean CanParse(PacketStackInfo previousStackElement, Memory<byte> content) =>
            previousStackElement is EthernetFrame ethernetFrame && ethernetFrame.Type == (UInt16)EthnernetPayloadTypes.MPLS;

        public PacketParserOutput ParsePacket(PacketStackInfo previousStackElement, Memory<byte> content)
        {
            var mplsLables = new List<PacketStackInfo>();
            Boolean endOfButton = false;
            Int32 sliceIndex = 0;
            do
            {
                var slice = content.Slice(sliceIndex, 4);
                var span = slice.Span;

                UInt32 value = ByteHelper.ConvertToUInt32(span);
                Byte flagByte = span[2];
                flagByte &= (Byte)0b_0000_1110;
                flagByte >>= 1;

                endOfButton = (span[2] & (Byte)0b_0000_0001) == 1;
                UInt32 labelNumber = (value & (UInt32)0b_1111_1111_1111_1111_1111_0000_0000_0000) >> 12;
                byte ttl = span[3];
                mplsLables.Add(new MPLSPacket(labelNumber, ttl, endOfButton, flagByte, content[sliceIndex..], previousStackElement.Packet));
                sliceIndex += 4;
            } while (endOfButton == false);

            var nextSpan = content.Slice(sliceIndex, 4).Span;
            Byte nextNibble = nextSpan[0];
            nextNibble &= 0b_1111_0000;
            nextNibble >>= 4;

            if (Option == MPLSParserOptions.Layer3)
            {

                if (nextNibble == 4)
                {
                    throw new NotImplementedException();
                }
                else if (nextNibble == 6)
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (Option == MPLSParserOptions.Layer2WithControlWord)
                {
                    if (nextNibble == 0)
                    {
                        var firstPart = nextSpan[0];
                        firstPart &= 0b_0000_1111;

                        UInt16 sequenceNumber = ByteHelper.ConvertToUInt16(nextSpan, 2);

                        UInt16 reservedValue = (UInt16)(firstPart + nextSpan[1]);
                        if (reservedValue != 0)
                        {
                            Byte secondPart = nextSpan[1];
                            Byte fragment = secondPart;
                            fragment &= 0b_11_00_00_00;
                            fragment >>= 6;

                            Byte flag = firstPart;
                            flag &= 0b_00_00_11_11;

                            Byte length = secondPart;
                            length &= 0b_00_11_11_11;

                            mplsLables.Add(new FragmentationBasedEthernetControlWordPacket(flag, fragment, length, sequenceNumber, content[sliceIndex..], previousStackElement.Packet));
                        }
                        else
                        {
                            mplsLables.Add(new EthernetControlWordPacket(sequenceNumber, content[sliceIndex..], previousStackElement.Packet));
                        }

                        sliceIndex += 4;
                    }
                }

                EthernetParser ethernetParser = new();
                var ethernetPackets = ethernetParser.ParsePacket(previousStackElement.Packet, content[sliceIndex..]);
                mplsLables.AddRange(ethernetPackets.StackInfos);

                sliceIndex += ethernetPackets.HeaderLength;
            }

            return new PacketParserOutput(sliceIndex, mplsLables, false);
        }
    }
}

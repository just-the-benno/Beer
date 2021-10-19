using SharpShark.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark
{
    public class PcapFileHeader
    {
        public PcapFileHeader(uint magicNumber, ushort majorVersion, ushort minorVersion, TimeSpan gMTOffeset, uint timezoneAccurancy, uint lengthOfCapturePackets, uint networkType)
        {
            MagicNumber = magicNumber;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            GMTOffeset = gMTOffeset;
            TimezoneAccurancy = timezoneAccurancy;
            LengthOfCapturePackets = lengthOfCapturePackets;
            NetworkType = networkType;
        }

        public UInt32 MagicNumber { get; init; }
        public UInt16 MajorVersion { get; init; }
        public UInt16 MinorVersion { get; init; }
        public TimeSpan GMTOffeset { get; init; }
        public UInt32 TimezoneAccurancy { get; init; }
        public UInt32 LengthOfCapturePackets { get; init; }
        public UInt32 NetworkType { get; init; }
    }

    public class PcapFile
    {
        public PacketCollection Packets { get; init; }
        public String Name { get; init; }


        private PcapFile(String name, IEnumerable<Packet> packets)
        {
            Name = name;
            Packets = new PacketCollection(packets);
        }

        public static async Task<PcapFile> FromFile(String filename, IPacketStackBuilder parser)
        {
            Byte[] input = await File.ReadAllBytesAsync(filename);
            return FromStream(input, filename, parser);
        }

        public static PcapFile FromStream(byte[] input, String name, IPacketStackBuilder parser)
        {

            var magicNumber = ByteHelper.ConvertToUInt32FromByte(input, 0);
            var changeEncoding = !(magicNumber == 0xa1b2c3d4 || magicNumber == 0xa1b23c4d);

            var header = new PcapFileHeader(
                ByteHelper.ConvertToUInt32FromByte(input, 0, changeEncoding),
                ByteHelper.ConvertToUInt16FromByte(input, 4, changeEncoding),
                ByteHelper.ConvertToUInt16FromByte(input, 6, changeEncoding),
                TimeSpan.FromSeconds(ByteHelper.ConvertToInt32FromByte(input, 8, changeEncoding)),
                ByteHelper.ConvertToUInt32FromByte(input, 12, changeEncoding),
                ByteHelper.ConvertToUInt32FromByte(input, 16, changeEncoding),
                ByteHelper.ConvertToUInt32FromByte(input, 20, changeEncoding)
                );


            Int32 packetIndex = 24;

            DateTime utcZero = new DateTime(1970, 1, 1).Add(header.GMTOffeset);

            List<Packet> packets = new();

            while (packetIndex < input.Length)
            {
                UInt32 timestampSeconds = ByteHelper.ConvertToUInt32FromByte(input, packetIndex, changeEncoding);
                UInt32 timestampMicroSeconds = ByteHelper.ConvertToUInt32FromByte(input, packetIndex + 4, changeEncoding);
                UInt32 savedLength = ByteHelper.ConvertToUInt32FromByte(input, packetIndex + 8, changeEncoding);
                UInt32 originalLength = ByteHelper.ConvertToUInt32FromByte(input, packetIndex + 12, changeEncoding);

                TimeSpan deltaSeconds = TimeSpan.FromSeconds(timestampSeconds);
                TimeSpan deltaMicroSeconds = TimeSpan.FromTicks(10 * timestampMicroSeconds);

                packetIndex += 16;

                Int32 endIndex = (Int32)(packetIndex + savedLength);
                Byte[] content = input[packetIndex..endIndex];
                var packet = new Packet(utcZero.Add(deltaSeconds).Add(deltaMicroSeconds), originalLength, header.NetworkType, content);
                packets.Add(packet);

                packetIndex = endIndex;

                packet.BuildStack(parser);
            }

            var result = new PcapFile(name, packets);

            return result;
        }
    }
}

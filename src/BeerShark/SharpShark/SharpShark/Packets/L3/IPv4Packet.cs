using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class IPv4Packet : PacketStackInfo
    {
        public IPv4Packet(byte headerLenght, byte typeOfService, ushort totalLength, ushort identification, bool dontFragement, bool moreFragments, ushort fragmentOffset, byte timeToLive, ushort protocol, ushort checksum, IPv4Address source, IPv4Address destionation, Memory<byte> content, Packet packet) : base(content, packet)
        {
            HeaderLenght = headerLenght;
            TypeOfService = typeOfService;
            TotalLength = totalLength;
            Identification = identification;
            DontFragement = dontFragement;
            MoreFragments = moreFragments;
            FragmentOffset = fragmentOffset;
            TimeToLive = timeToLive;
            Protocol = protocol;
            Checksum = checksum;
            Source = source;
            Destionation = destionation;
        }

        public Byte HeaderLenght { get; init; }
        public Byte TypeOfService { get; init; }
        public UInt16 TotalLength { get; init; }
        public UInt16 Identification { get; init; }
        public Boolean DontFragement { get; init; }
        public Boolean MoreFragments { get; init; }
        public UInt16 FragmentOffset {  get; init; }
        public Byte TimeToLive { get; init; }
        public UInt16 Protocol { get; init; }
        public UInt16 Checksum { get; init; }
        public IPv4Address Source { get; init; }
        public IPv4Address Destionation { get; init; }

    }
}

using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketClientIdentifierOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketClientIdentifierOption>
    {
        #region Properties

        public DHCPv4ClientIdentifier Identifier { get; private set; }

        #endregion

        #region constructor and factories

        public DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier identifier) : 
            base((Byte)DHCPv4OptionTypes.ClientIdentifier, identifier.GetBytes())
        {
            Identifier = identifier;
        }

        public DHCPv4PacketClientIdentifierOption FromByteArray(Byte[] data)
        {
            return FromByteArray(data, 0);
        }

        public static DHCPv4PacketClientIdentifierOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 2)
            {
                throw new ArgumentException(nameof(data));
            }

            if (data[offset] != (Byte)DHCPv4OptionTypes.ClientIdentifier)
            {
                throw new ArgumentException(nameof(data));
            }

            Int32 lenght = data[offset + 1];
            Byte[] identifierValue = ByteHelper.CopyData(data, offset + 2, lenght);

            return new DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier.FromOptionData(identifierValue));
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketClientIdentifierOption other)
        {
            return base.Equals(other);
        }

        #endregion

    }
}

using Beer.DaAPI.Core.Helper;
using Beer.DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Beer.DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;

namespace Beer.DaAPI.Core.Common
{
    public class DHCPv4ClientIdentifier : Value, IEquatable<DHCPv4ClientIdentifier>
    {
        #region Properties

        public Byte[] HwAddress { get; init; } = Array.Empty<Byte>();
        public Byte HardwareAddressType { get; init; } = 0;
        public DUID DUID { get; init; } = DUID.Empty;
        public UInt32 IaId { get; init; } = 0;
        public String IdentifierValue { get; init; } = String.Empty;


        #endregion

        #region Constructor

        private DHCPv4ClientIdentifier()
        {

        }

        public static DHCPv4ClientIdentifier Empty => new();

        public static DHCPv4ClientIdentifier FromDuid(UInt32 iaid, DUID duid)
        {
            if (duid == null || duid == DUID.Empty)
            {
                throw new ArgumentNullException(nameof(duid));
            }

            return new DHCPv4ClientIdentifier
            {
                DUID = duid,
                IaId = iaid,
            };
        }

        public static DHCPv4ClientIdentifier FromDuid(UInt32 iaid, DUID duid, Byte[] hwAddres)
        {
            if (duid == null || duid == DUID.Empty)
            {
                throw new ArgumentNullException(nameof(duid));
            }

            if (hwAddres == null || hwAddres.Length == 0)
            {
                throw new ArgumentNullException(nameof(hwAddres));
            }

            return new DHCPv4ClientIdentifier
            {
                DUID = duid,
                IaId = iaid,
                HardwareAddressType = (Byte)DHCPv4PacketHardwareAddressTypes.Ethernet,
                HwAddress = ByteHelper.CopyData(hwAddres)
            };
        }

        public byte[] GetHwAddress()
        {
            if (DUID != DUID.Empty)
            {
                if (DUID is LinkLayerAddressDUID a)
                {
                    return  ByteHelper.CopyData(a.LinkLayerAddress);
                }
                else if (DUID is LinkLayerAddressAndTimeDUID b)
                {
                    return ByteHelper.CopyData(b.LinkLayerAddress);
                }
            }
            
            if(HwAddress != null && HwAddress.Length > 0)
            {
                 return ByteHelper.CopyData(HwAddress);
            }

            return Array.Empty<Byte>();
        }

        public byte[] GetBytes()
        {
            if (DUID != DUID.Empty)
            {
                return ByteHelper.ConcatBytes(new Byte[] { 255 }, ByteHelper.GetBytes(IaId), DUID.GetAsByteStream());
            }
            else if (String.IsNullOrEmpty(IdentifierValue) == false)
            {
                return ByteHelper.ConcatBytes(new Byte[] { 0 }, ASCIIEncoding.ASCII.GetBytes(IdentifierValue));
            }
            else
            {
                return ByteHelper.ConcatBytes(new Byte[] { HardwareAddressType }, HwAddress);
            }

            throw new NotImplementedException();
        }

        public static DHCPv4ClientIdentifier FromOptionData(byte[] identifierRawVaue)
        {
            Byte subOptionIdentifier = identifierRawVaue[0];
            if (subOptionIdentifier == 0xff)
            {
                return DHCPv4ClientIdentifier.FromDuid(ByteHelper.ConvertToUInt32FromByte(identifierRawVaue, 1),
                  DUIDFactory.GetDUID(ByteHelper.CopyData(identifierRawVaue, 5)));
            }
            else if (subOptionIdentifier == 0)
            {
                String identifierValue = new ASCIIEncoding().GetString(ByteHelper.CopyData(identifierRawVaue, 1));
                return DHCPv4ClientIdentifier.FromIdentifierValue(identifierValue);
            }
            else
            {
                return DHCPv4ClientIdentifier.FromHwAddress(subOptionIdentifier, ByteHelper.CopyData(identifierRawVaue, 1));
            }
        }

        public static DHCPv4ClientIdentifier FromHwAddress(Byte[] hwAddress) => FromHwAddress((Byte)DHCPv4PacketHardwareAddressTypes.Ethernet, hwAddress);

        public static DHCPv4ClientIdentifier FromHwAddress(Byte hwAddressType, Byte[] hwAddres)
        {
            if (hwAddres == null || hwAddres.Length == 0)
            {
                throw new ArgumentNullException(nameof(hwAddres));
            }

            return new DHCPv4ClientIdentifier
            {
                DUID = DUID.Empty,
                HardwareAddressType = hwAddressType,
                HwAddress = ByteHelper.CopyData(hwAddres)
            };
        }

        public DHCPv4ClientIdentifier AddHardwareAddress(byte[] clientHardwareAddress)
        {
            clientHardwareAddress ??= Array.Empty<Byte>();

            return new DHCPv4ClientIdentifier
            {
                DUID = this.DUID,
                IaId = IaId,
                IdentifierValue = IdentifierValue,
                HardwareAddressType = (Byte)DHCPv4PacketHardwareAddressTypes.Ethernet,
                HwAddress = this.HwAddress.Length > 0 ? ByteHelper.CopyData(this.HwAddress) : ByteHelper.CopyData(clientHardwareAddress),
            };
        }

        public static DHCPv4ClientIdentifier FromIdentifierValue(String value) => new DHCPv4ClientIdentifier
        {
            IdentifierValue = value
        };

        #endregion

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other is DHCPv4ClientIdentifier == true)
            {
                return Equals((DHCPv4ClientIdentifier)other);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(DHCPv4ClientIdentifier other)
        {
            if (this.DUID != DUID.Empty)
            {
                return this.DUID.Equals(other.DUID);
            }
            else if (String.IsNullOrEmpty(IdentifierValue) == false)
            {
                return this.IdentifierValue == other.IdentifierValue;
            }
            else
            {
                return ByteHelper.AreEqual(this.HwAddress, other.HwAddress);
            }
        }

        public static bool operator ==(DHCPv4ClientIdentifier left, DHCPv4ClientIdentifier right) => Equals(left, right);
        public static bool operator !=(DHCPv4ClientIdentifier left, DHCPv4ClientIdentifier right) => !Equals(left, right);

        public override int GetHashCode()
        {
            if (DUID != DUID.Empty)
            {
                return DUID.GetHashCode();
            }
            else if (this.HwAddress.Length > 0)
            {
                return this.HwAddress.Sum(x => x);
            }
            else
            {
                return IdentifierValue.GetHashCode();
            }
        }

        public string AsUniqueString()
        {
            String appendix = IdentifierValue;
            if (DUID != DUID.Empty)
            {
                appendix = SimpleByteToStringConverter.Convert(DUID.GetAsByteStream());
            }
            else if (HwAddress.Length > 0)
            {
                appendix = SimpleByteToStringConverter.Convert(HwAddress);
            }

            String result = $"DHCPv4Client-{appendix}";
            return result;
        }

        public Boolean HasHardwareAddress() => HwAddress.Length > 0;
    }
}

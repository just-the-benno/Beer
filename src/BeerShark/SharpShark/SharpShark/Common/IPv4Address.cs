using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark
{
    public class IPv4Address : IComparable<IPv4Address>, IEquatable<IPv4Address>
    {

        private readonly Byte[] _addressBytes;

        public IPv4Address(Byte[] address)
        {
            if (address == null || address.Length != 4)
            {
                throw new ArgumentException(null, nameof(address));
            }

            _addressBytes = new byte[4]
            {
                address[0],
                address[1],
                address[2],
                address[3],
            };
        }

        public IPv4Address(String address)
        {
            if (String.IsNullOrEmpty(address) == true)
            {
                throw new ArgumentNullException(nameof(address));
            }

            String[] parts = address.Trim().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
            {
                throw new ArgumentException("invalid address", nameof(address));
            }

            _addressBytes = new byte[4];
            for (int i = 0; i < parts.Length; i++)
            {
                String part = parts[i];
                try
                {
                    Byte addressByte = Convert.ToByte(part);
                    _addressBytes[i] = addressByte;
                }
                catch (Exception)
                {
                    throw new ArgumentException(null, nameof(address));
                }
            }
        }

        public override string ToString() => $"{_addressBytes[0]}.{_addressBytes[1]}.{_addressBytes[2]}.{_addressBytes[3]}";

        public override bool Equals(object? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other is IPv4Address == false) { return false; }

            IPv4Address realOther = (IPv4Address)other;
            return Equals(realOther);
        }

        public bool Equals(IPv4Address? other)
        {
            if (other is null) { return false; }

            for (int i = 0; i < 4; i++)
            {
                if (this._addressBytes[i] != other._addressBytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        public Byte[] GetBytes() => new[]
            {
                _addressBytes[0],
                _addressBytes[1],
                _addressBytes[2],
                _addressBytes[3],
            };

        private UInt32? _numericValue;
        public UInt32 GetNumericValue() => _numericValue ??= ByteHelper.ConvertToUInt32FromByte(_addressBytes, 0);

        public override int GetHashCode()
        {
            UInt32 numericValue = GetNumericValue();
            return (Int32)numericValue;
        }

        public int CompareTo(IPv4Address? other)
        {
            UInt32 ownValue = GetNumericValue();
            if (other is null) { return -1; }
            UInt32 otherValue = other.GetNumericValue();

            Int64 diff = (Int64)ownValue - (Int64)otherValue;
            if (diff == 0)
            {
                return 0;
            }
            if (diff > 0)
            {
                if (diff > Int32.MaxValue)
                {
                    return Int32.MaxValue;
                }
                else
                {
                    return (Int32)diff;
                }
            }
            else
            {
                if (diff < Int32.MinValue)
                {
                    return Int32.MinValue;
                }
                else
                {
                    return (Int32)diff;
                }
            }
        }

        public static bool operator ==(IPv4Address addr1, IPv4Address addr2) => Equals(addr1, addr2);
        public static bool operator !=(IPv4Address addr1, IPv4Address addr2) => !Equals(addr1, addr2);

        public static bool operator <(IPv4Address addr1, IPv4Address addr2)
        {
            UInt32 addr1Value = addr1.GetNumericValue();
            UInt32 addr2Value = addr2.GetNumericValue();

            return addr1Value < addr2Value;
        }

        public static bool operator <=(IPv4Address addr1, IPv4Address addr2)
        {
            UInt32 addr1Value = addr1.GetNumericValue();
            UInt32 addr2Value = addr2.GetNumericValue();

            return addr1Value <= addr2Value;
        }

        public static bool operator >=(IPv4Address addr1, IPv4Address addr2)
        {
            UInt32 addr1Value = addr1.GetNumericValue();
            UInt32 addr2Value = addr2.GetNumericValue();

            return addr1Value >= addr2Value;
        }

        public static bool operator >(IPv4Address addr1, IPv4Address addr2)
        {
            UInt32 addr1Value = addr1.GetNumericValue();
            UInt32 addr2Value = addr2.GetNumericValue();

            return addr1Value > addr2Value;
        }
    }
}

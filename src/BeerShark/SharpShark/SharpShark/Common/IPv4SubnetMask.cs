using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark
{
    public class IPv4SubnetMask
    {
        private static readonly Dictionary<Int32, Byte[]> _possibleSubnetMasks = new()
        {
            { 0, new byte[4] {0,0,0,0} },
            { 1, new byte[4] {128,0,0,0} },
            { 2, new byte[4] {192,0,0,0} },
            { 3, new byte[4] {224,0,0,0} },
            { 4, new byte[4] {240,0,0,0} },
            { 5, new byte[4] {248,0,0,0} },
            { 6, new byte[4] {252,0,0,0} },
            { 7, new byte[4] {254,0,0,0} },
            { 8, new byte[4] {255,0,0,0} },

            { 9, new byte[4] { 255,128, 0,0} },
            { 10, new byte[4] { 255,192, 0,0} },
            { 11, new byte[4] { 255,224, 0,0} },
            { 12, new byte[4] { 255,240, 0,0} },
            { 13, new byte[4] { 255,248, 0,0} },
            { 14, new byte[4] { 255,252, 0,0} },
            { 15, new byte[4] { 255,254, 0,0} },
            { 16, new byte[4] { 255,255, 0,0} },

            { 17, new byte[4] { 255, 255,128, 0} },
            { 18, new byte[4] { 255, 255,192, 0} },
            { 19, new byte[4] { 255, 255,224, 0} },
            { 20, new byte[4] { 255, 255,240, 0} },
            { 21, new byte[4] { 255, 255,248, 0} },
            { 22, new byte[4] { 255, 255,252, 0} },
            { 23, new byte[4] { 255, 255,254, 0} },
            { 24, new byte[4] { 255, 255,255, 0} },

            { 25, new byte[4] { 255, 255, 255,128} },
            { 26, new byte[4] { 255, 255, 255,192} },
            { 27, new byte[4] { 255, 255, 255,224} },
            { 28, new byte[4] { 255, 255, 255,240} },
            { 29, new byte[4] { 255, 255, 255,248} },
            { 30, new byte[4] { 255, 255, 255,252} },
            { 31, new byte[4] { 255, 255, 255,254} },
            { 32, new byte[4] { 255, 255, 255,255} },
        };

        private readonly Byte[] _maskAsByte;

        public IPv4SubnetMask(Byte lenght)
        {
            if (lenght > 32) { throw new ArgumentOutOfRangeException(nameof(lenght)); }

            _maskAsByte = _possibleSubnetMasks[lenght];
        }

        public IPv4SubnetMask(String rawValue)
        {
            var pseudoAddress = new IPv4Address(rawValue);
            var pseudoAddressBytes = pseudoAddress.GetBytes();

            _maskAsByte = pseudoAddressBytes;
        }

        private UInt32? _numericValue;
        public UInt32 GetNumericValue() => _numericValue ??= ByteHelper.ConvertToUInt32FromByte(_maskAsByte, 0);

        public override string ToString() => $"{_maskAsByte[0]}.{_maskAsByte[1]}.{_maskAsByte[2]}.{_maskAsByte[3]}";
    }
}

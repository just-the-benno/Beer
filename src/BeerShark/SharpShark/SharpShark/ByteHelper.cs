using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark
{
    public class ByteHelper
    {
        public static UInt16 ConvertToUInt16FromByte(Byte[] data, Int32 start, Boolean changeEncoding = false) =>
            changeEncoding == false ? (UInt16)(data[start] << 8 | data[start + 1]) : (UInt16)(data[start + 1] << 8 | data[start]);

        public static UInt32 ConvertToUInt32FromByte(Byte[] data, Int32 start, Boolean changeEncoding = false) =>
             changeEncoding == false ?
                (UInt32)(data[start] << 24 | data[start + 1] << 16 | data[start + 2] << 8 | data[start + 3]) :
                (UInt32)(data[start + 3] << 24 | data[start + 2] << 16 | data[start + 1] << 8 | data[start]);

        public static UInt32 ConvertToUInt32(Span<Byte> data, Int32 start = 0) =>
               (UInt32)(data[start] << 24 | data[start + 1] << 16 | data[start + 2] << 8 | data[start + 3]);

        internal static ushort ConvertToUInt16(Span<Byte> data, Int32 start) =>
            (UInt16)(data[start] << 8 | data[start + 1]);

        public static Int32 ConvertToInt32FromByte(Byte[] data, Int32 start, Boolean changeEncoding = false) =>
             changeEncoding == false ?
                (Int32)(data[start] << 24 | data[start + 1] << 16 | data[start + 2] << 8 | data[start + 3]) :
                (Int32)(data[start + 3] << 24 | data[start + 2] << 16 | data[start + 1] << 8 | data[start]);

        public static bool AreEqual(Span<byte> span1, Span<byte> span2)
        {
            if (span1.Length != span2.Length) { return false; }

            for (int i = 0; i < span1.Length; i++)
            {
                if (span1[i] != span2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}

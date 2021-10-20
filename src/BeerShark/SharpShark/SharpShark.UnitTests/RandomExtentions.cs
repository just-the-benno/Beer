using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.UnitTests
{
    public static class RandomExtentions
    {
        public static Byte[] NextBytes(this Random random, Int32 length)
        {
            Byte[] output = new byte[length];
            random.NextBytes(output);

            return output;
        }
    }
}

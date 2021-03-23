using System;
using System.Collections.Generic;

namespace Beer.TestHelper
{
    public static class RandomExtentions
    {
        public static Byte[] NextBytes(this Random random, Int32 length)
        {
            Byte[] output = new byte[length];
            random.NextBytes(output);

            return output;
        }

        public static IEnumerable<Byte[]> NextByteArrays(
            this Random random, Int32 amount, Int32 minBytesPerArray, Int32 maxBytesPerArray)
        {
            List<Byte[]> result = new List<byte[]>(amount);
            for (int i = 0; i < amount; i++)
            {
                result.Add(random.NextBytes(random.Next(minBytesPerArray, maxBytesPerArray)));
            }

            return result;

        }

        public static String _validChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static String GetAlphanumericString(this Random random, Int32 length)
        {
            Char[] output = new char[length];

            for (int i = 0; i < length; i++)
            {
                output[i] = _validChars[random.Next(0, _validChars.Length)];
            }

            return new String(output);
        }

        public static String GetAlphanumericString(this Random random) => GetAlphanumericString(random, random.Next(10, 20));

        public static Boolean NextBoolean(this Random random)
        {
            return random.NextDouble() > 0.5;
        }

        public static Guid NextGuid(this Random random)
        {
            Byte[] value = random.NextBytes(16);
            Guid result = new Guid(value);

            return result;
        }

        public static IEnumerable<Guid> NextGuids(this Random random, Int32 min = 10, Int32 max = 30)
        {
            Int32 amount = random.Next(min, max);
            List<Guid> result = new List<Guid>(amount);
            for (int i = 0; i < amount; i++)
            {
                result.Add(random.NextGuid());
            }

            return result;
        }

        public static Byte NextByte(this Random random) => (Byte)random.Next(0, Byte.MaxValue + 1);

        public static IEnumerable<UInt16> GetUInt16Values(this Random random, Int32 amoun)
        {
            List<UInt16> values = new List<ushort>(amoun);
            for (int i = 0; i < amoun; i++)
            {
                values.Add(random.NextUInt16());
            }

            return values;
        }

        public static UInt16 NextUInt16(this Random random) => (UInt16)random.Next(0, UInt16.MaxValue);

        public static UInt32 NextUInt32(this Random random) => (UInt32)Int32.MaxValue + (UInt32)random.Next();

        public static String RandomizeUpperAndLowerChars(this Random random, String input)
        {
            Char[] output = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                Char item = input[i];
                if (Char.IsLetter(item) == true)
                {
                    if (random.NextBoolean() == true)
                    {
                        item = Char.ToUpper(item);
                    }
                    else
                    {
                        item = Char.ToLower(item);
                    }
                }

                output[i] = item;
            }

            return new String(output);

        }

        public static T GetEnumValue<T>(this Random rand, params T[] expectedItems)
        {
            List<T> expectedItemsAsList = new List<T>(expectedItems);
            Object preObject = null;

            do
            {
                var values = Enum.GetValues(typeof(T));
                Int32 index = rand.Next(0, values.Length);

                Int32 currIndex = 0;
                foreach (Object item in values)
                {
                    if (currIndex == index)
                    {
                        preObject = item;
                        break;
                    }

                    currIndex++;
                }
            } while (expectedItemsAsList.IndexOf((T)preObject) >= 0);

            return (T)preObject;
        }

        public static T GetRandomElement<T> (this IList<T> list, Random random)
        {
            Int32 index = random.Next(0, list.Count);
            return list[index];
        }
    }
}

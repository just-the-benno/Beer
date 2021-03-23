using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beer.DaAPI.Service.TestHelper
{
    public static class RandomExtentions
    {
        public static IPv4SubnetMask GetSubnetmask(this Random random)
        {
            Int32 maskValue = random.Next(0, 32);

            return new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(maskValue));
        }

        public static IPv4Address GetIPv4Address(this Random random)
        {
            return IPv4Address.FromByteArray(random.NextBytes(4));
        }

        public static IPv6Address GetIPv6Address(this Random random)
        {
            return IPv6Address.FromByteArray(random.NextBytes(16));
        }

        public static List<IPv4Address> GetIPv4Addresses(this Random random, Int32 min = 10, Int32 max = 30)
        {
            Int32 amount = random.Next(min, max);
            List<IPv4Address> result = new List<IPv4Address>(amount);

            for (int i = 0; i < amount; i++)
            {
                result.Add(random.GetIPv4Address());
            }

            return result;
        }

        public static List<IPv6Address> GetIPv6Addresses(this Random random, Int32 min = 10, Int32 max = 30)
        {
            Int32 amount = random.Next(min, max);
            List<IPv6Address> result = new List<IPv6Address>(amount);

            for (int i = 0; i < amount; i++)
            {
                result.Add(random.GetIPv6Address());
            }

            return result;
        }

        public static IPv4Address GetIPv4AddressGreaterThan(this Random random, IPv4Address start, Int32 minium = 100, Int32 maximum = 1000)
        {
            return start + random.Next(minium, maximum);
        }

        public static IPv6Address GetIPv6AddressGreaterThan(this Random random, IPv6Address start, Int32 minium = 100, Int32 maximum = 1000)
        {
            return start + (UInt64)(random.Next(minium, maximum));
        }

        public static List<IPv4Address> GetIPv4AddressesGreaterThan(this Random random, IPv4Address start, Int32 minium = 100, Int32 maximum = 1000, Int32 minAmount = 10, Int32 maxAmount = 30)
        {
            Int32 amount = random.Next(minAmount, maxAmount);
            List<IPv4Address> result = new List<IPv4Address>();
            for (int i = 0; i < amount; i++)
            {
                result.Add(random.GetIPv4AddressGreaterThan(start, minium, maximum));
            }

            return result;
        }

        public static List<IPv6Address> GetIPv6AddressesGreaterThan(this Random random, IPv6Address start, Int32 minium = 100, Int32 maximum = 1000, Int32 minAmount = 10, Int32 maxAmount = 30)
        {
            Int32 amount = random.Next(minAmount, maxAmount);
            List<IPv6Address> result = new List<IPv6Address>();
            for (int i = 0; i < amount; i++)
            {
                result.Add(random.GetIPv6AddressGreaterThan(start, minium, maximum));
            }

            return result;
        }

        public static IPv4Address GetIPv4AddressSmallerThan(this Random random, IPv4Address start, Int32 minium = 100, Int32 maximum = 1000)
        {
            return start - random.Next(minium, maximum);
        }

        public static IPv6Address GetIPv6AddressSmallerThan(this Random random, IPv6Address start, Int32 minium = 100, Int32 maximum = 1000)
        {
            return start - (UInt32)random.Next(minium, maximum);
        }

        public static List<IPv4Address> GetIPv4AddressesSmallerThan(this Random random, IPv4Address start, Int32 minium = 100, Int32 maximum = 1000, Int32 minAmount = 10, Int32 maxAmount = 30)
        {
            Int32 amount = random.Next(minAmount, maxAmount);
            List<IPv4Address> result = new List<IPv4Address>();
            for (int i = 0; i < amount; i++)
            {
                result.Add(random.GetIPv4AddressSmallerThan(start, minium, maximum));
            }

            return result;
        }

        public static List<IPv6Address> GetIPv6AddressesSmallerThan(this Random random, IPv6Address start, Int32 minium = 100, Int32 maximum = 1000, Int32 minAmount = 10, Int32 maxAmount = 30)
        {
            Int32 amount = random.Next(minAmount, maxAmount);
            List<IPv6Address> result = new List<IPv6Address>();
            for (int i = 0; i < amount; i++)
            {
                result.Add(random.GetIPv6AddressSmallerThan(start, minium, maximum));
            }

            return result;
        }

        public static IPv4Address GetIPv4AddressBetween(this Random random, IPv4Address start, IPv4Address end)
        {
            Int64 diff = end - start;
            Int32 realDiff = Int32.MaxValue;
            if (diff < Int32.MaxValue)
            {
                realDiff = (Int32)diff;
            }

            return start + random.Next(0, realDiff);
        }

        public static IPv6Address GetIPv6AddressBetween(this Random random, IPv6Address start, IPv6Address end)
        {
            Double diff = end - start;
            Int32 realDiff = Int32.MaxValue;
            if (diff < Int32.MaxValue)
            {
                realDiff = (Int32)diff;
            }

            return start + (UInt32)random.Next(0, realDiff);
        }

        public static List<IPv4Address> GetIPv4AddressesBetween(this Random random, IPv4Address start, IPv4Address end, Int32 minAmount = 10, Int32 maxAmount = 30)
        {
            Int32 amount = random.Next(minAmount, maxAmount);
            List<IPv4Address> addresses = new List<IPv4Address>(amount);

            for (int i = 0; i < amount; i++)
            {
                IPv4Address address = GetIPv4AddressBetween(random, start, end);
                addresses.Add(address);
            }

            return addresses;
        }

        public static List<IPv6Address> GetIPv6AddressesBetween(this Random random, IPv6Address start, IPv6Address end, Int32 minAmount = 10, Int32 maxAmount = 30)
        {
            Int32 amount = random.Next(minAmount, maxAmount);
            List<IPv6Address> addresses = new List<IPv6Address>(amount);

            for (int i = 0; i < amount; i++)
            {
                IPv6Address address = GetIPv6AddressBetween(random, start, end);
                addresses.Add(address);
            }

            return addresses;
        }

        public static IPv4Address GetIPv4NetworkAddress(this Random random, IPv4SubnetMask mask)
        {
            IPv4Address start = random.GetIPv4Address();

            Byte[] rawResult = ByteHelper.AndArray(start.GetBytes(), mask.GetBytes());

            IPv4Address result = IPv4Address.FromByteArray(rawResult);
            return result;
        }

        public static IPv4Address GetIPv4AddressWithinSubnet(this Random random, IPv4SubnetMask mask, IPv4Address networkAddress)
        {
            Int32 maxAddressInSubnet = mask.GetAmountOfPossibleAddresses();

            IPv4Address result = networkAddress + random.Next(1, maxAddressInSubnet);
            return result;
        }

        public static IPv4Address GetIPv4AddressOutWithSubnet(this Random random, IPv4SubnetMask mask, IPv4Address networkAddress)
        {
            Boolean upperBoundaries = random.NextDouble() > 0.5;

            if (upperBoundaries == true)
            {
                Int32 maxAddressInSubnet = mask.GetAmountOfPossibleAddresses();
                return networkAddress + maxAddressInSubnet + random.Next(10, 300);
            }
            else
            {
                return networkAddress - random.Next(10, 300);
            }
        }

        public static Tuple<IPv4Address, IPv4Address> GenerateUniqueAddressRange(this Random random, ICollection<Tuple<IPv4Address, IPv4Address>> existingRanges)
        {
            Boolean isUnique;
            IPv4Address start;
            IPv4Address end;
            do
            {
                start = random.GetIPv4Address();
                end = random.GetIPv4AddressGreaterThan(start);

                isUnique = true;
                foreach (var item in existingRanges)
                {
                    if (start >= item.Item1 && end <= item.Item2)
                    {
                        isUnique = false;
                        break;
                    }
                }
            } while (isUnique == false);

            Tuple<IPv4Address, IPv4Address> newItem = new Tuple<IPv4Address, IPv4Address>(start, end);
            existingRanges.Add(newItem);

            return newItem;
        }

        private static byte GetRandomIdentifier(Random random, HashSet<byte> usedOptionIdentifier)
        {
            Byte identiifer;
            do
            {
                identiifer = (Byte)random.Next(0, Byte.MaxValue);
            } while (usedOptionIdentifier.Contains(identiifer));

            usedOptionIdentifier.Add(identiifer);

            return identiifer;
        }

        public static List<DHCPv4ScopeProperty> GenerateProperties(this Random random) =>
            random.GenerateProperties(new List<DHCPv4OptionTypes>());

        public static List<DHCPv4ScopeProperty> GenerateProperties(this Random random, ICollection<DHCPv4OptionTypes> excludedOptions)
        {
            HashSet<Byte> usedOptionIdentifier = new HashSet<byte>(excludedOptions.Select(x => (Byte)x));
            List<DHCPv4ScopeProperty> result = new List<DHCPv4ScopeProperty>();

            Int32 addressListPropertieAmount = random.Next(3, 10);
            for (int i = 0; i < addressListPropertieAmount; i++)
            {
                byte identiifer = GetRandomIdentifier(random, usedOptionIdentifier);

                result.Add(new DHCPv4AddressListScopeProperty(
                    identiifer, random.GetIPv4Addresses()));
            }

            Int32 addressPropertieAmount = random.Next(3, 10);
            for (int i = 0; i < addressPropertieAmount; i++)
            {
                byte identiifer = GetRandomIdentifier(random, usedOptionIdentifier);

                result.Add(new DHCPv4AddressScopeProperty(
                    identiifer, random.GetIPv4Address()));
            }

            Int32 booleanPropertieAmount = random.Next(3, 10);
            for (int i = 0; i < booleanPropertieAmount; i++)
            {
                byte identiifer = GetRandomIdentifier(random, usedOptionIdentifier);

                result.Add(new DHCPv4BooleanScopeProperty(
                    identiifer, random.NextBoolean()));
            }

            Int32 textPropertieAmount = random.Next(3, 10);
            for (int i = 0; i < textPropertieAmount; i++)
            {
                byte identiifer = GetRandomIdentifier(random, usedOptionIdentifier);

                result.Add(new DHCPv4TextScopeProperty(
                    identiifer, random.GetAlphanumericString(random.Next(30, 100))));
            }

            Int32 numericPropertieAmount = random.Next(10, 20);
            for (int i = 0; i < textPropertieAmount; i++)
            {
                byte identiifer = GetRandomIdentifier(random, usedOptionIdentifier);
                Int64 value;
                DHCPv4NumericValueTypes numericType;
                Double randomValue = random.NextDouble();
                if (randomValue < 0.33)
                {
                    value = (UInt32)Int32.MaxValue + (UInt32)random.Next();
                    numericType = DHCPv4NumericValueTypes.UInt32;
                }
                else if (randomValue < 0.66)
                {
                    value = random.Next(0, UInt16.MaxValue);
                    numericType = DHCPv4NumericValueTypes.UInt16;
                }
                else
                {
                    value = random.Next(0, Byte.MaxValue);
                    numericType = DHCPv4NumericValueTypes.Byte;
                }

                result.Add(
                    DHCPv4NumericValueScopeProperty.FromRawValue(identiifer, value.ToString(), numericType));
            }

            Int32 timePropertieAmount = random.Next(3, 10);
            for (int i = 0; i < timePropertieAmount; i++)
            {
                byte identiifer = GetRandomIdentifier(random, usedOptionIdentifier);

                result.Add(new DHCPv4TimeScopeProperty(
                    identiifer, random.NextBoolean(), TimeSpan.FromMinutes(random.Next(3, 10000))));
            }

            return result;
        }




    }
}

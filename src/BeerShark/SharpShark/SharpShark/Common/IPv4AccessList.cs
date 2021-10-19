using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark
{
    public class IPv4AccessListEntry
    {
        public IPv4Address Address { get; init; }
        public IPv4SubnetMask Mask { get; init; }

        public IPv4AccessListEntry(IPv4Address address, IPv4SubnetMask mask)
        {
            if ((address.GetNumericValue() & mask.GetNumericValue()) != address.GetNumericValue())
            {
                throw new ArgumentException("address is not a subnet id");
            }

            Address = address;
            Mask = mask;
        }

        public Boolean IsMatch(IPv4Address input) =>
            (input.GetNumericValue() & Mask.GetNumericValue()) == Address.GetNumericValue();

        public override string ToString() => $"{Address} with {Mask}";
    }

    public class IPv4AccessList
    {
        private readonly IEnumerable<IPv4AccessListEntry> _entries;

        public IPv4AccessList(IEnumerable<IPv4AccessListEntry> entries)
        {
            _entries = entries;
        }

        public Boolean IsMatch(IPv4Address address)
        {
            foreach (var item in _entries)
            {
                if(item.IsMatch(address) == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

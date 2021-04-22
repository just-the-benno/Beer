using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4AddressListScopeProperty : DHCPv4ScopeProperty
    {
        #region Properties

        public IEnumerable<IPv4Address> Addresses { get; private set; }

        #endregion

        #region Constructor

        private DHCPv4AddressListScopeProperty()
        {

        }

        public DHCPv4AddressListScopeProperty(DHCPv4OptionTypes optionIdentifier, IEnumerable<IPv4Address> addresses)
            : this((Byte)optionIdentifier, addresses)
        {

        }

        public DHCPv4AddressListScopeProperty(Byte optionIdentifier, IEnumerable<IPv4Address> addresses) : base(
            optionIdentifier,DHCPv4ScopePropertyType.AddressList)
        {
            Addresses = new List<IPv4Address>(addresses);
        }

        #endregion
    }
}

using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public abstract class DHCPv4MacAddressResolverBase : IScopeResolver<DHCPv4Packet, IPv4Address>
    {
        #region Properties

        public Boolean SearchClientIdenfifier { get; protected set; } = false;
        protected abstract Func<Byte[]> MacAddressGetter { get; }

        #endregion

        #region Methods

        public Boolean HasUniqueIdentifier => false;
        public byte[] GetUniqueIdentifier(DHCPv4Packet packet) => throw new NotImplementedException();

        public Boolean PacketMeetsCondition(DHCPv4Packet packet)
        {
            var expectedAddress = MacAddressGetter();
            Byte[] packetMacAddress = packet.ClientHardwareAddress;
            if(ByteHelper.AreEqual(expectedAddress, packetMacAddress) == true)
            {
                return true;
            }

            if(SearchClientIdenfifier == false)
            {
                return false;
            }

            var identifier = packet.GetClientIdentifier();
            if (identifier == null)
            {
                return false;
            }

            Byte[] hwAddressInIdentifier = identifier.GetHwAddress();
            if(hwAddressInIdentifier.Length == 0)
            {
                return false;
            }

            return ByteHelper.AreEqual(expectedAddress, hwAddressInIdentifier);
        }

        public abstract Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer);
        public abstract void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer);
        public abstract ScopeResolverDescription GetDescription();

        public abstract IDictionary<String, String> GetValues();

        #endregion
    }
}

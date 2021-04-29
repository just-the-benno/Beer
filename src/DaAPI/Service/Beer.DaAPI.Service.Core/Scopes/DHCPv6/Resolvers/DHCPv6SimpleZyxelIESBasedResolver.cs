using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public abstract class DHCPv6SimpleZyxelIESBasedResolver : IScopeResolver<DHCPv6Packet, IPv6Address>
    {
        #region Fields

        private const int _macAddressLength = 6;
        private static readonly Encoding _encoding = ASCIIEncoding.ASCII;
        private Byte[] _interfaceIdValueAsByte;

        #endregion

        #region Properties

        public UInt16 Index { get; private set; }
        public UInt16 SlotId { get; private set; }
        public UInt16 PortId { get; private set; }

        #endregion

        private (DHCPv6PacketRemoteIdentifierOption RemoteOption, DHCPv6PacketByteArrayOption InterfaceOption) GetOptions(DHCPv6Packet packet)
        {
            var chain = ((DHCPv6RelayPacket)packet).GetRelayPacketChain();
            DHCPv6RelayPacket relayedPacket = chain[Index];

            var remoteIdentiiferOption = relayedPacket.GetOption<DHCPv6PacketRemoteIdentifierOption>(DHCPv6PacketOptionTypes.RemoteIdentifier);
            var interfaceOption = relayedPacket.GetOption<DHCPv6PacketByteArrayOption>(DHCPv6PacketOptionTypes.InterfaceId);

            return (remoteIdentiiferOption, interfaceOption);
        }

        public Boolean HasUniqueIdentifier => true;

        public Byte[] GetUniqueIdentifier(DHCPv6Packet packet)
        {
            var (RemoteOption, InterfaceOption) = GetOptions(packet);
            Byte[] result = ByteHelper.ConcatBytes(RemoteOption.Data, InterfaceOption.Data);

            return result;
        }

        protected abstract IEnumerable<String> GetAddtionalPropertyKeys();
        protected abstract Boolean ArePropertiesAndValuesValidInternal(IDictionary<String, String> valueMapper, ISerializer serializer);

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKeys(new[] { nameof(Index), nameof(SlotId), nameof(PortId) }.Union(GetAddtionalPropertyKeys())) == false)
            {
                return false;
            }

            var numberValues = new[] { nameof(Index), nameof(SlotId), nameof(PortId) };
            foreach (var item in numberValues)
            {
                String index = serializer.Deserialze<String>(valueMapper[item]);
                if (UInt16.TryParse(index, out UInt16 numberValue) == true)
                {
                    if (item != nameof(Index) && numberValue < 1)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return ArePropertiesAndValuesValidInternal(valueMapper, serializer);
        }

        protected abstract void ApplyValuesInternal(IDictionary<String, String> valueMapper, ISerializer serializer);

        public void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            Index = serializer.Deserialze<UInt16>(valueMapper[nameof(Index)]);
            SlotId = serializer.Deserialze<UInt16>(valueMapper[nameof(SlotId)]);
            PortId = serializer.Deserialze<UInt16>(valueMapper[nameof(PortId)]);

            _interfaceIdValueAsByte = _encoding.GetBytes($"{SlotId}/{PortId}");

            ApplyValuesInternal(valueMapper, serializer);
        }

        protected abstract IEnumerable<ScopeResolverPropertyDescription> GetAddionalProperties();
        protected abstract String GetTypeName();

        public ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
          GetTypeName(),
          new List<ScopeResolverPropertyDescription>
         {
                   new ScopeResolverPropertyDescription(nameof(Index), ScopeResolverPropertyValueTypes.UInt32 ),
                   new ScopeResolverPropertyDescription(nameof(SlotId), ScopeResolverPropertyValueTypes.UInt32 ),
                   new ScopeResolverPropertyDescription(nameof(PortId), ScopeResolverPropertyValueTypes.UInt32 ),
          }.Union(GetAddionalProperties()));

        protected abstract Byte[] GetDeviceMacAddress();

        public Boolean PacketMeetsCondition(DHCPv6Packet packet)
        {
            if (packet is DHCPv6RelayPacket == false) { return false; }

            var chain = ((DHCPv6RelayPacket)packet).GetRelayPacketChain();
            if (chain.Count <= Index) { return false; }

            var (RemoteOption, InterfaceOption) = GetOptions(packet);

            if (InterfaceOption == null || RemoteOption == null)
            {
                return false;
            }

            var remoteIdentifierValueAsByte = ByteHelper.ConcatBytes(new Byte[4], GetDeviceMacAddress());

            Boolean interfaceResult = ByteHelper.AreEqual(_interfaceIdValueAsByte, InterfaceOption.Data);
            Boolean remodeIdentifierResult = ByteHelper.AreEqual(remoteIdentifierValueAsByte, RemoteOption.Data, 4);

            return interfaceResult && remodeIdentifierResult;
        }

        protected abstract IDictionary<String, String> GetAdditonalValues();

        public IDictionary<String, String> GetValues()
        {
            var values = new Dictionary<String, String>
            {
                { nameof(Index), Index.ToString() },
                { nameof(SlotId), SlotId.ToString() },
                { nameof(PortId), PortId.ToString() },
            };

            foreach (var item in GetAdditonalValues())
            {
                values.Add(item.Key, item.Value);
            }

            return values;
        }
    }
}

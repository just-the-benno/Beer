using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public abstract class DHCPv4Option82ResolverBase : IScopeResolver<DHCPv4Packet, IPv4Address>
    {
        #region Fields

        public Byte[] Value { get; protected set; }
        public Func<Byte[]> ValueGetter { get; protected set; } = () => null;

        #endregion

        #region Methods

        public Boolean HasUniqueIdentifier => true;
        public byte[] GetUniqueIdentifier(DHCPv4Packet packet) => packet.GetOptionByIdentifier(82)?.OptionData ?? Array.Empty<Byte>();

        public Boolean PacketMeetsCondition(DHCPv4Packet packet)
        {
            Byte[] rawData = GetUniqueIdentifier(packet);

            return ByteHelper.AreEqual(rawData, ValueGetter() ?? Value);
        }

        public abstract Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer);

        public abstract void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer);
        public abstract ScopeResolverDescription GetDescription();
        public abstract IDictionary<String, String> GetValues();

        #endregion
    }
}

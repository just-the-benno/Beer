using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6.Resolvers;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4PseudoResolver : IPseudoResolver, IScopeResolver<DHCPv4Packet, IPv4Address>
    {
        #region Constructor

        public DHCPv4PseudoResolver()
        {

        }

        #endregion

        #region Methods

        public Boolean HasUniqueIdentifier => false;
        public Byte[] GetUniqueIdentifier(DHCPv4Packet packet) => throw new NotImplementedException();
        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer) => true;

        public void ApplyValues(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
        }

        public bool PacketMeetsCondition(DHCPv4Packet packet) => true;

        public ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
           nameof(DHCPv4PseudoResolver), Array.Empty<ScopeResolverPropertyDescription>());

        public IDictionary<String, String> GetValues() => new Dictionary<String, String>();

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Services;
using Microsoft.Extensions.Logging;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4Option82Resolver : DHCPv4Option82ResolverBase
    {
        #region Methods

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKey(nameof(Value)) == false) { return false; }

            try
            {
                Byte[] result = serializer.Deserialze<Byte[]>(valueMapper[nameof(Value)]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            Value = serializer.Deserialze<Byte[]>(valueMapper[nameof(Value)]);
        }

        public override ScopeResolverDescription GetDescription()
        {
            return new ScopeResolverDescription(
                nameof(DHCPv4Option82Resolver),
                new List<ScopeResolverPropertyDescription>
                {
                   new ScopeResolverPropertyDescription(nameof(Value),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.ByteArray),
                }
                );
        }

        public override IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(Value), System.Text.Json.JsonSerializer.Serialize(Value) },
        };

        #endregion
    }
}

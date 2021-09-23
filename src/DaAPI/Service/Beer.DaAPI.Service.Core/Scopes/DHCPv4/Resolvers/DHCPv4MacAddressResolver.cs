using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4MacAddressResolver : DHCPv4MacAddressResolverBase
    {
        #region Properties

        public Byte[] MacAddress { get; private set; } = Array.Empty<Byte>();

        protected override Func<byte[]> MacAddressGetter => () => MacAddress;

        #endregion

        #region Methods

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            try
            {
                if (valueMapper.ContainsKeys(new[] { nameof(MacAddress), nameof(SearchClientIdenfifier) }) == false)
                {
                    return false;
                }

                String rawDeviceMacAddressValue = serializer.Deserialze<String>(valueMapper[nameof(MacAddress)]);
                var address = ByteHelper.GetBytesFromHexString(rawDeviceMacAddressValue);
                if(address.Length == 0)
                {
                    return false;
                }

                serializer.Deserialze<Boolean>(valueMapper[nameof(SearchClientIdenfifier)]);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            String rawDeviceMacAddressValue = serializer.Deserialze<String>(valueMapper[nameof(MacAddress)]);

            SearchClientIdenfifier = serializer.Deserialze<Boolean>(valueMapper[nameof(SearchClientIdenfifier)]);
            MacAddress = ByteHelper.GetBytesFromHexString(rawDeviceMacAddressValue);
        }

        public override ScopeResolverDescription GetDescription()
        {
            return new ScopeResolverDescription(
                 nameof(DHCPv4MacAddressResolver),
                 new List<ScopeResolverPropertyDescription>
                {
                   new ScopeResolverPropertyDescription (nameof(MacAddress),  ScopeResolverPropertyValueTypes.ByteArray  ),
                   new ScopeResolverPropertyDescription ( nameof(SearchClientIdenfifier),  ScopeResolverPropertyValueTypes.Boolean ),
                }
                );
        }

        public override IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(MacAddress), ByteHelper.ToString(MacAddress,false) },
            { nameof(SearchClientIdenfifier),SearchClientIdenfifier == true ? "true" : "false" },
        };

        #endregion
    }
}

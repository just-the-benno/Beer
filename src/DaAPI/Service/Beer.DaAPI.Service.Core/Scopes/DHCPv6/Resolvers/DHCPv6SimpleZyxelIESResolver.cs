using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6SimpleZyxelIESResolver : DHCPv6SimpleZyxelIESBasedResolver
    {
        #region Fields

        private const Int32 _macAddressLength = 6;

        #endregion

        #region Properties

        public Byte[] DeviceMacAddress { get; private set; }

        #endregion

        protected override IEnumerable<string> GetAddtionalPropertyKeys() => new[] { nameof(DeviceMacAddress) };
        
        protected override Boolean ArePropertiesAndValuesValidInternal(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]);
            if (String.IsNullOrEmpty(value) == true) { return false; }

            try
            {
                var macAddress = ByteHelper.GetBytesFromHexString(value);
                if (macAddress.Length != _macAddressLength)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        protected override void ApplyValuesInternal(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            DeviceMacAddress = ByteHelper.GetBytesFromHexString(serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]));
        }

        protected override IEnumerable<ScopeResolverPropertyDescription> GetAddionalProperties() => new[]
        {
            new ScopeResolverPropertyDescription(nameof(DeviceMacAddress),ScopeResolverPropertyValueTypes.ByteArray)
        };

        protected override byte[] GetDeviceMacAddress() => DeviceMacAddress;

        protected override string GetTypeName() => nameof(DHCPv6SimpleZyxelIESResolver);

        protected override IDictionary<string, string> GetAdditonalValues() => new Dictionary<String, String>
        {
            { nameof(DeviceMacAddress), ByteHelper.ToString(DeviceMacAddress,false) },
        };
    }
}

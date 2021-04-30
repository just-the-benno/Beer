using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4SimpleCiscoSGSeriesResolver : DHCPv4SimpleCiscoSGSeriesBasedResolver
    {
        #region Properties

        public Byte[] DeviceMacAddress { get; set; }

        #endregion

        #region Methods

        protected override IEnumerable<string> GetAddtionalPropertyKeys() => new[] { nameof(DeviceMacAddress) };

        protected override bool ArePropertiesAndValuesValidInternal(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            try
            {
                String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]);
                if (String.IsNullOrEmpty(value) == true) { return false; }

                var macAddress = ByteHelper.GetBytesFromHexString(value);
                if (macAddress.Length != 6) { return false; }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override void ApplyValuesInternal(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            String rawDeviceMacAddressValue = serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]);
            DeviceMacAddress = ByteHelper.GetBytesFromHexString(rawDeviceMacAddressValue);

            Value = GetOptionValue(DeviceMacAddress);
        }

        protected override string GetTypeName() => nameof(DHCPv4SimpleCiscoSGSeriesResolver);

        protected override IEnumerable<ScopeResolverPropertyDescription> GetAddionalProperties() => new[]
        {
          new ScopeResolverPropertyDescription(nameof(DeviceMacAddress),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.ByteArray),
        };

        protected override IDictionary<string, string> GetAdditonalValues() => new Dictionary<String, String>
        {
            { nameof(DeviceMacAddress), ByteHelper.ToString(DeviceMacAddress,false) },
        };

        #endregion
    }
}

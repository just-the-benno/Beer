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
    public class DHCPv4SimpleZyxelIESResolver : DHCPv4Option82ResolverBase
    {
        #region Properties

        public Byte PortNumber { get; set; }
        public Byte LinecardNumber { get; set; }
        public Byte[] DeviceMacAddress { get; set; }

        #endregion

        #region Methods

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if(valueMapper == null) { return false; }

            if (valueMapper.ContainsKeys(new[] { nameof(PortNumber), nameof(LinecardNumber), nameof(DeviceMacAddress) }) == false) { return false; }

            try
            {
                String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]);
                if (String.IsNullOrEmpty(value) == true) { return false; }

                var macAddress = ByteHelper.GetBytesFromHexString(value);
                if (macAddress.Length != 6) { return false; }

                serializer.Deserialze<Byte>(valueMapper[nameof(PortNumber)]);
                serializer.Deserialze<Byte>(valueMapper[nameof(LinecardNumber)]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            ASCIIEncoding encoding = new();

            PortNumber = serializer.Deserialze<Byte>(valueMapper[nameof(PortNumber)]);
            LinecardNumber = serializer.Deserialze<Byte>(valueMapper[nameof(LinecardNumber)]);
            String rawDeviceMacAddressValue = serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]);
            DeviceMacAddress = ByteHelper.GetBytesFromHexString(rawDeviceMacAddressValue);

            Byte[] remoteId = encoding.GetBytes($"{LinecardNumber}/{PortNumber}");

            Byte[] result = new byte[1 + 1 + remoteId.Length + 1 + 1 + DeviceMacAddress.Length];
            result[0] = 0x01; //remote id suboption value
            result[1] = (Byte)remoteId.Length; // lengthOfSuboption;
            remoteId.CopyTo(result, 2);
            result[2 + remoteId.Length] = 0x02; // circuit id suboption identiifer
            result[2 + remoteId.Length + 1] = (Byte)DeviceMacAddress.Length; // length of the address
            DeviceMacAddress.CopyTo(result, 2 + 2 +  remoteId.Length);

            Value = result;
        }

    public override ScopeResolverDescription GetDescription()
    {
        return new ScopeResolverDescription(
            nameof(DHCPv4SimpleZyxelIESResolver),
            new List<ScopeResolverPropertyDescription>
            {
                   new ScopeResolverPropertyDescription(nameof(LinecardNumber),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Byte),
                   new ScopeResolverPropertyDescription(nameof(PortNumber),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Byte),
                   new ScopeResolverPropertyDescription(nameof(DeviceMacAddress),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.ByteArray),
            }
            );
    }

        public override IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(LinecardNumber), LinecardNumber.ToString() },
            { nameof(PortNumber), PortNumber.ToString() },
            { nameof(DeviceMacAddress), ByteHelper.ToString(DeviceMacAddress,false) },
        };

    #endregion
}
}

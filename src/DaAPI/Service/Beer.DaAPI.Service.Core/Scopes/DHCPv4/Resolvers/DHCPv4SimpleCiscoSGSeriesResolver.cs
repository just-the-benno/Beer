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
    public class DHCPv4SimpleCiscoSGSeriesResolver : DHCPv4Option82ResolverBase
    {
        #region Properties

        public Byte PortNumber { get; set; }
        public UInt16 VlanNumber { get; set; }
        public Byte[] DeviceMacAddress { get; set; }

        #endregion

        #region Methods

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if(valueMapper == null) { return false; }

            if (valueMapper.ContainsKeys(new[] { nameof(PortNumber), nameof(VlanNumber), nameof(DeviceMacAddress) }) == false) { return false; }

            try
            {
                String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]);
                if (String.IsNullOrEmpty(value) == true) { return false; }

                var macAddress = ByteHelper.GetBytesFromHexString(value);
                if (macAddress.Length != 6) { return false; }

                serializer.Deserialze<Byte>(valueMapper[nameof(PortNumber)]);
                serializer.Deserialze<UInt16>(valueMapper[nameof(VlanNumber)]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            PortNumber = serializer.Deserialze<Byte>(valueMapper[nameof(PortNumber)]);
            VlanNumber = serializer.Deserialze<UInt16>(valueMapper[nameof(VlanNumber)]);
            String rawDeviceMacAddressValue = serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]);
            DeviceMacAddress = ByteHelper.GetBytesFromHexString(rawDeviceMacAddressValue);

            Byte[] vlanIdAsByte = ByteHelper.GetBytes(VlanNumber);

            Byte[] result = new byte[18];
            result[0] = 0x01; //circuit id 
            result[1] = 0x06; //remote id suboption value
            result[2] = 0x00; // mabye suboption of circuit id?
            result[3] = 0x04; // length of the suboption?
            vlanIdAsByte.CopyTo(result, 4);
            result[6] = 0x01; // maybe module in a stack?
            result[7] = PortNumber;
            result[8] = 0x02; //agent id suboption value
            result[9] = 0x08; //agent id suboption value
            result[10] = 0x00; //mabye suption type?
            result[11] = 0x06; // length of suboption type. 6 because of mac address?
            DeviceMacAddress.CopyTo(result, 12);

            Value = result;
        }

    public override ScopeResolverDescription GetDescription()
    {
        return new ScopeResolverDescription(
            nameof(DHCPv4SimpleCiscoSGSeriesResolver),
            new List<ScopeResolverPropertyDescription>
            {
                   new ScopeResolverPropertyDescription(nameof(VlanNumber),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.VLANId),
                   new ScopeResolverPropertyDescription(nameof(PortNumber),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Byte),
                   new ScopeResolverPropertyDescription(nameof(DeviceMacAddress),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.ByteArray),
            }
            );
    }

        public override IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(VlanNumber), VlanNumber.ToString() },
            { nameof(PortNumber), PortNumber.ToString() },
            { nameof(DeviceMacAddress), ByteHelper.ToString(DeviceMacAddress,false) },
        };

    #endregion
    }
}

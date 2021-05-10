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
    public abstract class DHCPv4SimpleCiscoSGSeriesBasedResolver : DHCPv4Option82ResolverBase
    {
        #region Properties

        public Byte PortNumber { get; set; }
        public UInt16 VlanNumber { get; set; }

        #endregion

        #region Methods

        protected abstract IEnumerable<String> GetAddtionalPropertyKeys();
        protected abstract Boolean ArePropertiesAndValuesValidInternal(IDictionary<String, String> valueMapper, ISerializer serializer);

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper == null) { return false; }

            if (valueMapper.ContainsKeys(new[] { nameof(PortNumber), nameof(VlanNumber) }.Union(GetAddtionalPropertyKeys())) == false) { return false; }

            try
            {
                serializer.Deserialze<Byte>(valueMapper[nameof(PortNumber)]);
                serializer.Deserialze<UInt16>(valueMapper[nameof(VlanNumber)]);
            }
            catch (Exception)
            {
                return false;
            }

            return ArePropertiesAndValuesValidInternal(valueMapper, serializer);
        }


        protected byte[] GetOptionValue(byte[] deviceMacAddress)
        {
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
            deviceMacAddress.CopyTo(result, 12);
            return result;
        }

        protected abstract void ApplyValuesInternal(IDictionary<String, String> valueMapper, ISerializer serializer);

        public override void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            PortNumber = serializer.Deserialze<Byte>(valueMapper[nameof(PortNumber)]);
            VlanNumber = serializer.Deserialze<UInt16>(valueMapper[nameof(VlanNumber)]);

            ApplyValuesInternal(valueMapper, serializer);
        }


        protected abstract IEnumerable<ScopeResolverPropertyDescription> GetAddionalProperties();
        protected abstract String GetTypeName();

        public override ScopeResolverDescription GetDescription()
        {
            return new ScopeResolverDescription(
                GetTypeName(),
                new List<ScopeResolverPropertyDescription>
                {
                   new ScopeResolverPropertyDescription(nameof(VlanNumber),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.VLANId),
                   new ScopeResolverPropertyDescription(nameof(PortNumber),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Byte),
                }.Union(GetAddionalProperties())
                );
        }

        protected abstract IDictionary<String, String> GetAdditonalValues();

        public override IDictionary<String, String> GetValues()
        {
            var values = new Dictionary<String, String>
            {
                { nameof(VlanNumber), VlanNumber.ToString() },
                { nameof(PortNumber), PortNumber.ToString() },
            };

            foreach (var item in GetAdditonalValues())
            {
                values.Add(item.Key, item.Value);
            }

            return values;
        }

        #endregion
    }
}

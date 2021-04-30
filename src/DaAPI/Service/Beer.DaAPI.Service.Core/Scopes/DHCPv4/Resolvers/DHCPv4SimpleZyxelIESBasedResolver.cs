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
    public abstract class DHCPv4SimpleZyxelIESBasedResolver : DHCPv4Option82ResolverBase
    {
        #region Properties

        public Byte PortNumber { get; set; }
        public Byte LinecardNumber { get; set; }

        #endregion

        #region Methods

        protected abstract IEnumerable<String> GetAddtionalPropertyKeys();
        protected abstract Boolean ArePropertiesAndValuesValidInternal(IDictionary<String, String> valueMapper, ISerializer serializer);

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper == null) { return false; }

            if (valueMapper.ContainsKeys(new[] { nameof(PortNumber), nameof(LinecardNumber) }.Union(GetAddtionalPropertyKeys())) == false) { return false; }

            try
            {
                serializer.Deserialze<Byte>(valueMapper[nameof(PortNumber)]);
                serializer.Deserialze<Byte>(valueMapper[nameof(LinecardNumber)]);
            }
            catch (Exception)
            {
                return false;
            }

            return ArePropertiesAndValuesValidInternal(valueMapper, serializer);
        }

        protected byte[] GetOptionValue(byte[] deviceMacAddress)
        {
            ASCIIEncoding encoding = new();

            Byte[] remoteId = encoding.GetBytes($"{LinecardNumber}/{PortNumber}");

            Byte[] result = new byte[1 + 1 + remoteId.Length + 1 + 1 + deviceMacAddress.Length];
            result[0] = 0x01; //remote id suboption value
            result[1] = (Byte)remoteId.Length; // lengthOfSuboption;
            remoteId.CopyTo(result, 2);
            result[2 + remoteId.Length] = 0x02; // circuit id suboption identiifer
            result[2 + remoteId.Length + 1] = (Byte)deviceMacAddress.Length; // length of the address
            deviceMacAddress.CopyTo(result, 2 + 2 + remoteId.Length);

            return result;
        }

        protected abstract void ApplyValuesInternal(IDictionary<String, String> valueMapper, ISerializer serializer);

        public override void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            PortNumber = serializer.Deserialze<Byte>(valueMapper[nameof(PortNumber)]);
            LinecardNumber = serializer.Deserialze<Byte>(valueMapper[nameof(LinecardNumber)]);

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
                   new ScopeResolverPropertyDescription(nameof(LinecardNumber),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Byte),
                   new ScopeResolverPropertyDescription(nameof(PortNumber),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Byte),
                }.Union(GetAddionalProperties())
                );
        }

        protected abstract IDictionary<String, String> GetAdditonalValues();

        public override IDictionary<String, String> GetValues()
        {
            var values = new Dictionary<String, String>
            {
                { nameof(LinecardNumber), LinecardNumber.ToString() },
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

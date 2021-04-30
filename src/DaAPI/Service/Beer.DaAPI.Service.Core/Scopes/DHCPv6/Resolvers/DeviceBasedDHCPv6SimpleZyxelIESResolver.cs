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
    public class DeviceBasedDHCPv6SimpleZyxelIESResolver : DHCPv6SimpleZyxelIESBasedResolver
    {
        #region Fields

        private readonly IDeviceService _deviceService;

        #endregion

        #region Properties

        public Guid DeviceId { get; private set; }

        #endregion

        public DeviceBasedDHCPv6SimpleZyxelIESResolver(IDeviceService deviceService)
        {
            this._deviceService = deviceService;
        }

        protected override IEnumerable<string> GetAddtionalPropertyKeys() => new[] { nameof(DeviceId) };

        protected override Boolean ArePropertiesAndValuesValidInternal(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceId)]);
            return String.IsNullOrEmpty(value) == false && Guid.TryParse(value, out Guid _) == true;
        }

        protected override void ApplyValuesInternal(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            DeviceId = serializer.Deserialze<Guid>(valueMapper[nameof(DeviceId)]);
        }

        protected override string GetTypeName() => nameof(DeviceBasedDHCPv6SimpleZyxelIESResolver);

        protected override IEnumerable<ScopeResolverPropertyDescription> GetAddionalProperties() => new[]
        {
            new ScopeResolverPropertyDescription(nameof(DeviceId),ScopeResolverPropertyValueTypes.Device)
        };

        protected override byte[] GetDeviceMacAddress() => _deviceService.GetMacAddressFromDevice(DeviceId);

        protected override IDictionary<string, string> GetAdditonalValues() => new Dictionary<String, String>
        {
            { nameof(DeviceId), DeviceId.ToString() },
        };
    }
}

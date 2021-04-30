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
    public class DeviceBasedDHCPv4SimpleZyxelIESResolver : DHCPv4SimpleZyxelIESBasedResolver
    {

        #region Fields

        private readonly IDeviceService _deviceService;

        #endregion
        
        #region Properties

        public Guid DeviceId { get; set; }

        #endregion

        #region Constructor

        public DeviceBasedDHCPv4SimpleZyxelIESResolver(IDeviceService deviceService)
        {
            this._deviceService = deviceService;
        }

        #endregion

        #region Methods

        protected override IEnumerable<String> GetAddtionalPropertyKeys() => new[] { nameof(DeviceId) };

        protected override bool ArePropertiesAndValuesValidInternal(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceId)]);
            return String.IsNullOrEmpty(value) == false && Guid.TryParse(value, out Guid _) == true;
        }

        protected override IEnumerable<ScopeResolverPropertyDescription> GetAddionalProperties() => new List<ScopeResolverPropertyDescription>
        {
            new ScopeResolverPropertyDescription(nameof(DeviceId),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Device),
        };

        protected override void ApplyValuesInternal(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            DeviceId = serializer.Deserialze<Guid>(valueMapper[nameof(DeviceId)]);

            ValueGetter = () => GetOptionValue(_deviceService.GetMacAddressFromDevice(DeviceId));
        }

        protected override string GetTypeName() => nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver);

        protected override IDictionary<string, string> GetAdditonalValues() => new Dictionary<String, String>
        {
            { nameof(DeviceId), DeviceId.ToString() },
        };

        #endregion
    }
}

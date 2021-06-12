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
    public class NonUniqueDeviceBasedDHCPv4SimpleCiscoSGSeriesResolver : DHCPv4SimpleCiscoSGSeriesBasedResolver
    {
        #region Fields

        private IDeviceService _deviceService;

        #endregion

        #region Properties

        public override bool HasUniqueIdentifier => false;
        public Guid DeviceId { get; set; }

        #endregion

        #region Constructor

        public DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        #endregion

        #region Methods

        protected override IEnumerable<string> GetAddtionalPropertyKeys() => new[] { nameof(DeviceId) };

        protected override bool ArePropertiesAndValuesValidInternal(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceId)]);
            return String.IsNullOrEmpty(value) == false && Guid.TryParse(value, out Guid _) == true;
        }

        protected override void ApplyValuesInternal(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            DeviceId = serializer.Deserialze<Guid>(valueMapper[nameof(DeviceId)]);

            ValueGetter = () => GetOptionValue(_deviceService.GetMacAddressFromDevice(DeviceId));
        }

        protected override string GetTypeName() => nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver);

        protected override IEnumerable<ScopeResolverPropertyDescription> GetAddionalProperties() => new[]
        {
          new ScopeResolverPropertyDescription(nameof(DeviceId),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Device),
        };

        protected override IDictionary<string, string> GetAdditonalValues() => new Dictionary<String, String>
        {
            { nameof(DeviceId), DeviceId.ToString() },
        };

        #endregion
    }
}

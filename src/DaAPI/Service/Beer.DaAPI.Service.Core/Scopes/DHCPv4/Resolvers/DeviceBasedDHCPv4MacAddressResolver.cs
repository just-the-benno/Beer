using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public class DeviceBasedDHCPv4MacAddressResolver : DHCPv4MacAddressResolverBase
    {
        private readonly IDeviceService _deviceService;

        #region Properties

        public Guid DeviceId { get; private set; }

        protected override Func<byte[]> MacAddressGetter => () => _deviceService.GetMacAddressFromDevice(DeviceId);

        #endregion

        #region Constructor

        public DeviceBasedDHCPv4MacAddressResolver(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        #endregion

        #region Methods

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            try
            {
                if (valueMapper.ContainsKeys(new[] { nameof(DeviceId), nameof(SearchClientIdenfifier) }) == false)
                {
                    return false;
                }

                serializer.Deserialze<Boolean>(valueMapper[nameof(SearchClientIdenfifier)]);
                serializer.Deserialze<Guid>(valueMapper[nameof(DeviceId)]);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            SearchClientIdenfifier = serializer.Deserialze<Boolean>(valueMapper[nameof(SearchClientIdenfifier)]);
            DeviceId = serializer.Deserialze<Guid>(valueMapper[nameof(DeviceId)]);
        }

        public override ScopeResolverDescription GetDescription()
        {
            return new ScopeResolverDescription(
                 nameof(DeviceBasedDHCPv4MacAddressResolver),
                 new List<ScopeResolverPropertyDescription>
                {
                   new ScopeResolverPropertyDescription (nameof(DeviceId),  ScopeResolverPropertyValueTypes.Device  ),
                   new ScopeResolverPropertyDescription ( nameof(SearchClientIdenfifier),  ScopeResolverPropertyValueTypes.Boolean ),
                }
                );
        }

        public override IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(DeviceId), DeviceId.ToString() },
            { nameof(SearchClientIdenfifier),SearchClientIdenfifier == true ? "true" : "false" },
        };

        #endregion
    }
}

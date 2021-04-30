using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Beer.DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DeviceBasedDHCPv6PeerAddressResolver : SimpleDHCPv6RelayPacketResolver
    {
        #region Fields

        private IDeviceService _deviceService;

        #endregion

        #region Properties

        public Guid DeviceId { get; private set; }

        #endregion

        #region Constructor

        public DeviceBasedDHCPv6PeerAddressResolver(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        #endregion

        #region Methods

        public override Boolean HasUniqueIdentifier => true;
        public override Byte[] GetUniqueIdentifier(DHCPv6Packet packet) => _deviceService.GetIPv6LinkLocalAddressFromDevice(DeviceId).GetBytes();

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKey(nameof(DeviceId)) == false)
            {
                return false;
            }

            String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceId)]);
            return String.IsNullOrEmpty(value) == false && Guid.TryParse(value, out Guid _) == true;
        }

        public override void ApplyValues(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            DeviceId = serializer.Deserialze<Guid>(valueMapper[nameof(DeviceId)]);
        }

        public override bool PacketMeetsCondition(DHCPv6Packet packet) =>
            PacketMeetsCondition(packet, (input) => input.PeerAddress == _deviceService.GetIPv6LinkLocalAddressFromDevice(DeviceId));

        public override ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
           nameof(DeviceBasedDHCPv6PeerAddressResolver), new[] {
             new ScopeResolverPropertyDescription(nameof(DeviceId),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Device),
           });

        public override IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(DeviceId), DeviceId.ToString().ToLower() },
        };

        #endregion
    }
}

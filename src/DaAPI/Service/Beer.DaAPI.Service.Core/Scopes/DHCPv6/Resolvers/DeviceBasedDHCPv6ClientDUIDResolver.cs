using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DeviceBasedDHCPv6ClientDUIDResolver : IScopeResolver<DHCPv6Packet, IPv6Address>
    {
        #region Fields

        private readonly IDeviceService _deviceService;

        #endregion

        #region Properties

        public Guid DeviceId { get; private set; }

        public Boolean HasUniqueIdentifier => true;

        #endregion

        #region Constructor

        public DeviceBasedDHCPv6ClientDUIDResolver(IDeviceService deviceService)
        {
            this._deviceService = deviceService;
        }

        #endregion

        #region Methods

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKey(nameof(DeviceId)) == false)
            {
                return false;
            }

            String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceId)]);
            return String.IsNullOrEmpty(value) == false && Guid.TryParse(value, out Guid _) == true;
        }

        public void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            DeviceId = serializer.Deserialze<Guid>(valueMapper[nameof(DeviceId)]);
        }

        public Boolean PacketMeetsCondition(DHCPv6Packet packet)
        {
            DHCPv6Packet innerPacket = packet.GetInnerPacket();
            DUID clientDuid = innerPacket.GetClientIdentifer();

            return clientDuid == _deviceService.GetDuidFromDevice(DeviceId);
        }

        public ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
           nameof(DeviceBasedDHCPv6ClientDUIDResolver), new[] {
             new ScopeResolverPropertyDescription(nameof(DeviceId),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Device)
           });

        public byte[] GetUniqueIdentifier(DHCPv6Packet packet) => packet.GetInnerPacket().GetClientIdentifer().GetAsByteStream();

        public IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(DeviceId), DeviceId.ToString() }
        };

        #endregion
    }
}

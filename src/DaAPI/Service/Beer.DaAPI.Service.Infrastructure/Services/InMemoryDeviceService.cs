using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.Services
{
    public class Device
    {
        public Guid Id { get; init; }
        public String Name { get; init; }

        public Byte[] MacAddress { get; init; }
        public IPv6Address LinkLocalAddress { get; init; }
        public DUID DUID { get; init; }
    }

    public class InMemoryDeviceService : IManagleableDeviceService
    {
        private Dictionary<Guid, Device> _devices;

        public InMemoryDeviceService(IEnumerable<Device> devices)
        {
            if (devices == null) { _devices = new(); return; }

            _devices = devices.ToDictionary(x => x.Id, x => x);
        }

        public void AddDevices(IEnumerable<Device> devices)
        {
            foreach (var item in devices)
            {
                _devices.Add(item.Id, item);
            }
        }

        private Device GetDeviceOrNull(Guid id)
        {
            if (_devices.ContainsKey(id) == false) { return null; }

            return _devices[id];
        }

        public DUID GetDuidFromDevice(Guid deviceId) => GetDeviceOrNull(deviceId)?.DUID ?? new UUIDDUID(Guid.Empty);
        public IPv6Address GetIPv6LinkLocalAddressFromDevice(Guid deviceId) => GetDeviceOrNull(deviceId)?.LinkLocalAddress ?? IPv6Address.Empty;
        public Byte[] GetMacAddressFromDevice(Guid deviceId) => GetDeviceOrNull(deviceId)?.MacAddress ?? Array.Empty<Byte>();
    }
}

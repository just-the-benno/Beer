using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Services
{
    public interface IDeviceService
    {
        byte[] GetMacAddressFromDevice(Guid deviceId);
        DUID GetDuidFromDevice(Guid deviceId);
        IPv6Address GetIPv6LinkLocalAddressFromDevice(Guid deviceId);
    }
}

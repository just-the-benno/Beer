using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.Services
{
    public interface IManagleableDeviceService : IDeviceService
    {
        void AddDevices(IEnumerable<Device> devices);
    }
}

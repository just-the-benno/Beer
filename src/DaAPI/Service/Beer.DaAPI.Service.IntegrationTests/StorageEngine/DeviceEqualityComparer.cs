using Beer.DaAPI.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.IntegrationTests.StorageEngine
{
    public class DeviceEqualityComparer : IEqualityComparer<Device>
    {
        public bool Equals(Device x, Device y) =>
            x.Name == y.Name &&
            x.LinkLocalAddress == y.LinkLocalAddress &&
            x.MacAddress == y.MacAddress &&
            x.DUID == y.DUID;

        public int GetHashCode([DisallowNull] Device obj) => obj.Id.GetHashCode();
    }
}

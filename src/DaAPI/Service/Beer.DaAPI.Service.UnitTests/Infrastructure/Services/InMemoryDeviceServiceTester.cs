using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Infrastructure.Services;
using Beer.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Beer.DaAPI.Service.UnitTests.Infrastructure.Services
{
    public class InMemoryDeviceServiceTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetDuidFromDevice_DeviceFound(Boolean devicesAddedLater)
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            UUIDDUID duid = new UUIDDUID(random.NextGuid());

            Device device = new Device { Id = id, DUID = duid };

            var service = new InMemoryDeviceService(devicesAddedLater == true ? null : new[] { device });
            if(devicesAddedLater == true)
            {
                service.AddDevices(new[] { device });
            }

            var actual = service.GetDuidFromDevice(id);
            Assert.IsAssignableFrom<UUIDDUID>(actual);
            Assert.Equal(duid, (UUIDDUID)actual);
        }


        [Fact]
        public void GetDuidFromDevice_DeviceNotFound()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            UUIDDUID duid = new UUIDDUID(random.NextGuid());

            Device device = new Device { Id = id, DUID = duid };

            var service = new InMemoryDeviceService(new[] { device });

            var actual = service.GetDuidFromDevice(random.NextGuid());
            Assert.IsAssignableFrom<UUIDDUID>(actual);
            Assert.Equal(Guid.Empty, ((UUIDDUID)actual).UUID);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetMacAddressFromDevice_DeviceFound(Boolean devicesAddedLater)
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            Byte[] macAddress = random.NextBytes(6);

            Device device = new Device { Id = id, MacAddress = macAddress };

            var service = new InMemoryDeviceService(devicesAddedLater == true ? null : new[] { device });
            if (devicesAddedLater == true)
            {
                service.AddDevices(new[] { device });
            }

            var actual = service.GetMacAddressFromDevice(id);
            Assert.Equal(macAddress, actual);
        }

        [Fact]
        public void GetMacAddressFromDevice_DeviceNotFound()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            Byte[] macAddress = random.NextBytes(6);

            Device device = new Device { Id = id, MacAddress = macAddress };

            var service = new InMemoryDeviceService(new[] { device });

            var actual = service.GetMacAddressFromDevice(random.NextGuid());
            Assert.NotNull(actual);
            Assert.Empty(actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetIPv6LinkLocalAddressFromDevice_DeviceFound(Boolean devicesAddedLater)
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            IPv6Address linkLocalAddress = IPv6Address.FromString("fe80::2314");

            Device device = new Device { Id = id, LinkLocalAddress = linkLocalAddress };

            var service = new InMemoryDeviceService(devicesAddedLater == true ? null : new[] { device });
            if (devicesAddedLater == true)
            {
                service.AddDevices(new[] { device });
            }

            var actual = service.GetIPv6LinkLocalAddressFromDevice(id);
            Assert.Equal(linkLocalAddress, actual);
        }

        [Fact]
        public void GetIPv6LinkLocalAddressFromDevice_DeviceNotFound()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            IPv6Address linkLocalAddress = IPv6Address.FromString("fe80::2314");

            Device device = new Device { Id = id, LinkLocalAddress = linkLocalAddress };

            var service = new InMemoryDeviceService(new[] { device });

            var actual = service.GetIPv6LinkLocalAddressFromDevice(random.NextGuid());
            Assert.NotNull(actual);
            Assert.Equal(IPv6Address.Empty,actual);
        }

    }
}

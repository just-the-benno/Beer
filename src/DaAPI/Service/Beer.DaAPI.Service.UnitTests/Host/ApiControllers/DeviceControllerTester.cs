using Beer.DaAPI.Service.API.ApiControllers;
using Beer.TestHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Infrastructure.Services;
using static Beer.DaAPI.Shared.Responses.DeviceResponses.V1;
using System.Diagnostics.CodeAnalysis;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;

namespace Beer.DaAPI.UnitTests.Host.ApiControllers
{
    public class DeviceControllerTester
    {
        public class DeviceOverviewComparer : IEqualityComparer<DeviceOverviewResponse>
        {
            public bool Equals(DeviceOverviewResponse x, DeviceOverviewResponse y) =>
                x.Name == y.Name &&
                x.Id == y.Id;

            public int GetHashCode([DisallowNull] DeviceOverviewResponse obj) => obj.Id.GetHashCode();
        }

        [Fact]
        public void GetAllDevices()
        {
            Random random = new Random();

            var devices = new List<Device> {
                 new Device
                 {
                     Id = random.NextGuid(),
                     DUID = new UUIDDUID(random.NextGuid()),
                     LinkLocalAddress = IPv6Address.FromString("fe80::8e21:d9ff:fecd:e2a"),
                     MacAddress = new byte[] { 0x8C, 0x21, 0xD9, 0xCD, 0x0E, 0x2A },
                     Name = "My first test device",
                 },
                new Device
                {
                    Id = random.NextGuid(),
                    DUID = new UUIDDUID(random.NextGuid()),
                    LinkLocalAddress = IPv6Address.FromString("fe80::449c:35ff:fec0:a1bc"),
                    MacAddress = new byte[] { 0x46, 0x9C, 0x35, 0xC0, 0xA1, 0xBC },
                    Name = "a device",
                }
            };

            Mock<IDHCPv6ReadStore> storeMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            storeMock.Setup(x => x.GetAllDevices()).Returns(devices).Verifiable();

            var controller = new DeviceController(
                storeMock.Object,
                Mock.Of<ILogger<DeviceController>>());

            var actionResult = controller.GetAllDevices();
            var result = actionResult.EnsureOkObjectResult<IEnumerable<DeviceOverviewResponse>>(true);

            Assert.Equal(devices.Select(x => new DeviceOverviewResponse { Name = x.Name, Id = x.Id }), result, new DeviceOverviewComparer());

            storeMock.Verify();
        }
    }
}

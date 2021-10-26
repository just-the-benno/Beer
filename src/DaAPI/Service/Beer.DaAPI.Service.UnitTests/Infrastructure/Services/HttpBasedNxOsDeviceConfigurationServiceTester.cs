using Beer.DaAPI.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Beer.DaAPI.Service.UnitTests.Infrastructure.Services
{
    public class HttpBasedNxOsDeviceConfigurationServiceTester
    {
        [Fact]
        public async void TestDeserialzation()
        {
            var content = await File.ReadAllTextAsync("./Assets/something.json");

            var service = new HttpBasedNxOsDeviceConfigurationService(
                new System.Net.Http.HttpClient(),
                Mock.Of<ILogger<HttpBasedNxOsDeviceConfigurationService>>());

            service.ParseIPv6RouteJson(content, true);

        }
    }
}

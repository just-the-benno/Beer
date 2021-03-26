using Beer.ControlCenter.Service.API.Infrastrucutre;
using Beer.TestHelper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Beer.ControlCenter.Service.IntegrationTests.APITester
{
    public class AppsControllerTester : ControllerTesterBase,
        IClassFixture<CustomWebApplicationFactory<FakeStartup>>
    {
        private readonly CustomWebApplicationFactory<FakeStartup> _factory;

        public AppsControllerTester(CustomWebApplicationFactory<FakeStartup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAppUrls()
        {
            var expectedUrls = new Dictionary<string, string>
                    {
                        { "DaAPI-Blazor","https://localhost:52010" },
                        { "ControlCenter-BlazorApp","https://localhost:52014" }
                    };

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot(GetBasePath());
                builder.UseStartup<FakeStartup>();
                builder.ConfigureTestServices(services =>
                {
                    AddFakeAuthentication(services, "Bearer");
                    ReplaceService(services, new AppSettings
                    {
                        AppURIs = expectedUrls,
                        JwtTokenAuthenticationOptions = new JwtTokenAuthenticationOptions
                        {
                            AuthorityUrl = "https://localhost:5005",
                        }
                    });
                });

            }).CreateClient();

            var rawResponse = await client.GetAsync("api/Apps/Urls");

            var response = await IsObjectResult<Dictionary<String, String>>(rawResponse);

            Assert.Equal(expectedUrls, response, new NonStrictDictionaryComparer<String, String>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetAppUrls_NotAuthorized(Boolean shouldBeAuthenticated)
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot(GetBasePath());
                builder.UseStartup<FakeStartup>();
                builder.ConfigureTestServices(services =>
                {
                    AddFakeAuthentication(services, "Bearer", "something", shouldBeAuthenticated);
                });

            }).CreateClient();

            var rawResponse = await client.GetAsync("api/Apps/Urls");

            if(shouldBeAuthenticated == true)
            {
                Assert.Equal(HttpStatusCode.Forbidden, rawResponse.StatusCode);
            }
            else
            {
                Assert.Equal(HttpStatusCode.Unauthorized, rawResponse.StatusCode);
            }
        }
    }
}

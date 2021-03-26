using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Beer.TestHelper
{
    public abstract class WebApplicationFactoryBase
    {
        protected static void RemoveServiceFromCollection(IServiceCollection services, Type type)
        {
            var descriptor = services.SingleOrDefault(
            d => d.ServiceType == type);

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
        }

        protected static void ReplaceService<T>(IServiceCollection services, T implementation) where T : class
        {
            RemoveServiceFromCollection(services, typeof(T));
            services.AddSingleton(implementation);
        }

        protected static void ReplaceServiceWithMock<T>(IServiceCollection services) where T : class
        {
            RemoveServiceFromCollection(services, typeof(T));
            services.AddSingleton(Mock.Of<T>());
        }

        protected static void AddFakeAuthentication(IServiceCollection services, String authenticaionSchema, String scopeName) => AddFakeAuthentication(services, authenticaionSchema, scopeName, true);

        protected static void AddFakeAuthentication(IServiceCollection services, String authenticaionSchema, String scopeName, Boolean shouldBeAuthenticated)
        {
            services.AddAuthentication(authenticaionSchema)
                .AddScheme<FakeAuthenticationSchemeOptions, FakeAuthenticationHandler>(
                    authenticaionSchema, options =>
                    {
                        options.ShouldBeAuthenticated = shouldBeAuthenticated;
                        options.AddClaim("scope", scopeName);
                    });
        }

        protected static async Task IsEmptyResult(HttpResponseMessage responseMessage)
        {
            Assert.True(responseMessage.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, responseMessage.StatusCode);

            String content = await responseMessage.Content.ReadAsStringAsync();
            Assert.True(String.IsNullOrEmpty(content));
        }

        protected static async Task<T> IsObjectResult<T>(HttpResponseMessage responseMessage)
        {
            Assert.True(responseMessage.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

            String rawContent = await responseMessage.Content.ReadAsStringAsync();
            T result = JsonConvert.DeserializeObject<T>(rawContent);

            return result;
        }

        protected static async Task<T> IsObjectResult<T>(HttpResponseMessage responseMessage, T expected)
        {
            T result = await IsObjectResult<T>(responseMessage);

            Assert.Equal(expected, result);
            return result;
        }
    }
}

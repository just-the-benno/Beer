using Microsoft.Extensions.DependencyInjection;
using Moq;
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
    public abstract class IntergrationControllerTeserBase
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

        protected static async Task<T> GetResponseContent<T>(System.Net.Http.HttpResponseMessage response)
        {
            Assert.True(response.IsSuccessStatusCode);

            String content = await response.Content.ReadAsStringAsync();
            T value = System.Text.Json.JsonSerializer.Deserialize<T>(content,new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            });
            return value;
        }

        protected static void AddFakeAuthentication(IServiceCollection services, String authenticaionShema = "Bearer", Boolean shouldBeAuthenticated = false)
        {
            services.AddAuthentication(authenticaionShema)
                .AddScheme<FakeAuthenticationSchemeOptions, FakeAuthenticationHandler>(
                    authenticaionShema, options =>
                    {
                        options.ShouldBeAuthenticated = shouldBeAuthenticated;
                    });
        }

        protected static void AddFakeAuthentication(IServiceCollection services, Action<FakeAuthenticationSchemeOptions> builder, String authenticationScheme = "Bearer")
        {
            services.AddAuthentication(authenticationScheme)
                .AddScheme<FakeAuthenticationSchemeOptions, FakeAuthenticationHandler>(authenticationScheme, builder);
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
            T result = System.Text.Json.JsonSerializer.Deserialize<T>(rawContent);

            return result;
        }

        protected static async Task<T> IsObjectResult<T>(HttpResponseMessage responseMessage, T expected)
        {
            T result = await IsObjectResult<T>(responseMessage);

            Assert.Equal(expected, result);
            return result;
        }

        protected static void IsBadRequest(HttpResponseMessage responseMessage)
        {
            Assert.False(responseMessage.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        protected static async Task IsBadRequest(HttpResponseMessage responseMessage, String expected)
        {
            Assert.False(responseMessage.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);

            String rawContent = await responseMessage.Content.ReadAsStringAsync();

            Assert.Equal(expected, rawContent);
        }


    }
}

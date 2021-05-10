using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorApp.Services
{
    public abstract class HttpClientBase
    {
        protected HttpClient Client { get; private set; }

        protected HttpClientBase(HttpClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        protected async Task<Boolean> PostAsJsonAsync<T>(String url, T input)
        {
            var response = await Client.PostAsJsonAsync(url, input);
            return response.IsSuccessStatusCode;
        }

        protected async Task<Boolean> PutAsJsonAsync<T>(String url, T input)
        {
            var response = await Client.PutAsJsonAsync(url, input);
            return response.IsSuccessStatusCode;
        }

    }
}

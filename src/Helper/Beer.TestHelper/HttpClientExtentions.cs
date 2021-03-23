using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Beer.TestHelper
{
    public static class HttpClientExtentions
    {
        public static Task<HttpResponseMessage> PutAsync<TInput>(this HttpClient client, String url, TInput input)
        {
            String serilizedContent = System.Text.Json.JsonSerializer.Serialize(input);
            return client.PutAsync(url, new StringContent(serilizedContent, Encoding.UTF8, "application/json"));
        }

        public static Task<HttpResponseMessage> PostAsync<TInput>(this HttpClient client, String url, TInput input)
        {
            String serilizedContent = System.Text.Json.JsonSerializer.Serialize(input);
            return client.PostAsync(url, new StringContent(serilizedContent, Encoding.UTF8, "application/json"));
        }

    }
}

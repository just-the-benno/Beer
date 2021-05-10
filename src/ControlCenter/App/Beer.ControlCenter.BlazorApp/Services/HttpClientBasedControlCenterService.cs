using Beer.ControlCenter.BlazorApp.Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static Beer.ControlCenter.BlazorApp.Services.Requests.BeerUserRequests.V1;
using static Beer.ControlCenter.BlazorApp.Services.Responses.BeerUserResponses.V1;

namespace Beer.ControlCenter.BlazorApp.Services
{
    public class HttpClientBasedControlCenterService : HttpClientBase, IControlCenterService
    {
        public HttpClientBasedControlCenterService(HttpClient client) : base(client)
        {
        }

        public async Task<IDictionary<String,String>> GetAppUrls()
        {
            var response = await Client.GetFromJsonAsync<IDictionary<String, String>>("/api/Apps/Urls");
            return response;
        }
    }
}

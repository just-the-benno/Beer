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
    public class HttpClientBasedBeerUserService : IBeerUserService
    {
        private HttpClient _client;

        public HttpClientBasedBeerUserService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<Boolean> DeleteUser(string userId)
        {
            var response = await _client.DeleteAsync($"/api/LocalUsers/{userId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<BeerUserOverview>> GetLocalUsers() => await _client.GetFromJsonAsync<IEnumerable<BeerUserOverview>>("/api/LocalUsers/");
        public async Task<Boolean> CheckIfUsernameExists(String name) => await _client.GetFromJsonAsync<Boolean>($"/api/LocalUsers/Exists/{name ?? String.Empty}");
        public async Task<IEnumerable<String>> GetAvailableAvatars() => await _client.GetFromJsonAsync<IEnumerable<String>>("/api/LocalUsers/Avatars/");

        private async Task<Boolean> PostAsJsonAsync<T>(String url,T input)
        {
            var response = await _client.PostAsJsonAsync(url, input);
            return response.IsSuccessStatusCode;
        }

        private async Task<Boolean> PutAsJsonAsync<T>(String url, T input)
        {
            var response = await _client.PutAsJsonAsync(url, input);
            return response.IsSuccessStatusCode;
        }

        public async  Task<Boolean> ResetPassword(String userId, String password) => await PutAsJsonAsync($"/api/LocalUsers/ChangePassword/{userId}", new ResetPasswordRequest
        {
            Password = password
        });

        public async Task<Boolean> CreateUser(CreateBeerUserRequest request) => await PostAsJsonAsync("/api/LocalUsers/", request);

    }
}

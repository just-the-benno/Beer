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
    public class HttpClientBasedBeerUserService : HttpClientBase, IBeerUserService
    {

        public HttpClientBasedBeerUserService(HttpClient client) : base(client)
        {
        }

        public async Task<Boolean> DeleteUser(string userId)
        {
            var response = await Client.DeleteAsync($"/api/LocalUsers/{userId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<BeerUserOverview>> GetLocalUsers() => await Client.GetFromJsonAsync<IEnumerable<BeerUserOverview>>("/api/LocalUsers/");
        public async Task<Boolean> CheckIfUsernameExists(String name) => await Client.GetFromJsonAsync<Boolean>($"/api/LocalUsers/Exists/{name ?? String.Empty}");
        public async Task<IEnumerable<String>> GetAvailableAvatars() => await Client.GetFromJsonAsync<IEnumerable<String>>("/api/LocalUsers/Avatars/");

        public async  Task<Boolean> ResetPassword(String userId, String password) => await PutAsJsonAsync($"/api/LocalUsers/ChangePassword/{userId}", new ResetPasswordRequest
        {
            Password = password
        });

        public async Task<Boolean> CreateUser(CreateBeerUserRequest request) => await PostAsJsonAsync("/api/LocalUsers/", request);

    }
}

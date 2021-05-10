using Beer.ControlCenter.BlazorApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.ControlCenter.BlazorApp.Services.Requests.BeerClientRequests.V1;

namespace Beer.ControlCenter.BlazorApp.Pages.Clients
{
    public class CreateClientViewModel
    {
        public String DisplayName { get; set; }
        public String ClientId { get; set; }
        public String Password { get; set; }
        public IList<SimpleValue<String>> RedirectUris { get; private set; } = new List<SimpleValue<String>>();
        public IList<SimpleValue<String>> AllowedCorsOrigins { get; set; } = new List<SimpleValue<String>>();

        public String FrontChannelLogoutUri { get; set; }
        public IList<SimpleValue<String>> PostLogoutRedirectUris { get; private set; } = new List<SimpleValue<String>>();
        public IList<SimpleValue<String>> AllowedScopes { get; private set; } = new List<SimpleValue<String>>();

        public Boolean RequirePkce { get; set; } = true;


        public void AddEmptyScope() => AllowedScopes.Add(new SimpleValue<string>(String.Empty));
        public void AddEmptyRedirectUrl() => RedirectUris.Add(new SimpleValue<string>(String.Empty));
        public void AddEmptyCORSUrl() => AllowedCorsOrigins.Add(new SimpleValue<string>(String.Empty));
        public void AddEmptyPostRedirectUrl() => PostLogoutRedirectUris.Add(new SimpleValue<string>(String.Empty));

        public CreateBeerOpenIdClientRequest ToRequest() => new()
        {
            DisplayName = DisplayName,
            ClientId = ClientId,
            AllowedCorsOrigins = AllowedCorsOrigins.Select(x => x.Value).ToArray(),
            AllowedScopes = AllowedScopes.Select(x => x.Value).ToArray(),
            FrontChannelLogoutUri = FrontChannelLogoutUri,
            Password = Password,
            PostLogoutRedirectUris = PostLogoutRedirectUris.Select(x => x.Value).ToArray(),
            RedirectUris = RedirectUris.Select(x => x.Value).ToArray(),
            RequirePkce = RequirePkce,
        };
    }
}

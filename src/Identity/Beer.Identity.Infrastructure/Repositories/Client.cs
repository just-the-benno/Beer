using Beer.Identity.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.Identity.Infrastructure.Repositories
{
    public class BeerClient
    {
        public Guid Id { get; set; }
        public String DisplayName { get; set; }
        public String ClientId { get; set; }
        public String HashedPassword { get; set; }
        public IEnumerable<String> RedirectUris { get; set; }
        public IEnumerable<String> AllowedCorsOrigins { get; set; }
        public String FrontChannelLogoutUri { get; set; }
        public IEnumerable<String> PostLogoutRedirectUris { get; set; }
        public IEnumerable<String> AllowedScopes { get; set; }
        public Boolean RequirePkce { get; set; }

        public void ParseUrls()
        {
            RedirectUris = RedirectUris.Select(x => x.RemoveTrailingSlashIfNeeded()).ToArray();
            AllowedCorsOrigins = AllowedCorsOrigins.Select(x => x.RemoveTrailingSlashIfNeeded()).ToArray();
            FrontChannelLogoutUri = FrontChannelLogoutUri.RemoveTrailingSlashIfNeeded();
            PostLogoutRedirectUris = PostLogoutRedirectUris.Select(x => x.RemoveTrailingSlashIfNeeded()).ToArray();
        }
    }
}

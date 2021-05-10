using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Shared.Responses
{
    public static class ClientResponses
    {
        public static class V1
        {
            public class ClientOverview
            {
                public Guid SystemId { get; set; }
                public String DisplayName { get; set; }
                public String ClientId { get; set; }
                public IEnumerable<String> RedirectUris { get; set; }
                public IEnumerable<String> AllowedCorsOrigins { get; set; }
                public String FrontChannelLogoutUri { get; set; }
                public IEnumerable<String> PostLogoutRedirectUris { get; set; }
                public IEnumerable<String> AllowedScopes { get; set; }
                public Boolean RequirePkce { get; set; }

            }
        }
    }
}

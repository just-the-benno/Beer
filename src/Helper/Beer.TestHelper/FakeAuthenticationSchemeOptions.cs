using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Beer.TestHelper
{
    public class FakeAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public Boolean ShouldBeAuthenticated { get; set; } = true;
        public ICollection<Claim> Claims { get; } = new List<Claim>();
        public ICollection<String> Scopes { get; } = new HashSet<String>();

        public void AddClaim(String type, String value) =>
            Claims.Add(new Claim(type, value));

        public void AddClaims(ICollection<Claim> claims)
        {
            foreach (var item in claims)
            {
                Claims.Add(item);
            }
        }

        public void AddScopes(IEnumerable<string> scopes)
        {
            foreach (var item in scopes)
            {
                Scopes.Add(item);
            }
        }
    }
}

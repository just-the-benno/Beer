using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Beer.TestHelper
{
    public class FakeAuthenticationHandler : AuthenticationHandler<FakeAuthenticationSchemeOptions>
    {
        public FakeAuthenticationHandler(IOptionsMonitor<FakeAuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Options.ShouldBeAuthenticated == false)
            {
                return Task.FromResult(AuthenticateResult.Fail("user not authenticated"));
            }

            String scopesAsList = String.Empty;

            foreach (var item in Options.Scopes)
            {
                scopesAsList += $"{item} ";
            }

            List<Claim> claims = new() {
                new Claim(ClaimTypes.Name, "Test user"),
            };

            if(String.IsNullOrEmpty(scopesAsList) == false)
            {
                claims.Add(new Claim("scope", scopesAsList.Trim()));
            }

            claims.AddRange(Options.Claims);

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}

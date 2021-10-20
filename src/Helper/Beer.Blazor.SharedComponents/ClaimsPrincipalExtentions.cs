using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Beer.Blazor.SharedComponents
{
    public static class ClaimsPrincipalExtentions
    {
        public static String GetClaimsValue(this ClaimsPrincipal principal, String type) =>
                 principal.Claims.FirstOrDefault(x => x.Type == type)?.Value;
    }
}

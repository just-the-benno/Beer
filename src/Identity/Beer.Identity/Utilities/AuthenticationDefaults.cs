using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Utilities
{
    public static class AuthenticationDefaults
    {
        public const String BearerSchemaName = "Bearer";
        public const String DefaultChallengeScheme = "oidc";
        public const String DefaultAuthenticationScheme = "Cookies";
        public const String LocalUserPolicyName = "LocalUsersPolicy";

        public const String BeerManageUserApiScopeName = "beer-users";

        public const String BeerUserListScope = "beer-users.list";
        public const String BeerUserCreateScope = "beer-users.create";
        public const String BeerUserDeleteScope = "beer-users.delete";
        public const String BeerUserResetPasswordScope = "beer-users.reset-password";

        public const String BeerAppClientId = "control-center-app";
        public const String DaAPIAppClientId = "daapi-app";

        public const String DaAPIMangeScope = "daapi.manage";
        public const String DaAPIScopeName = "daapi";
    }
}

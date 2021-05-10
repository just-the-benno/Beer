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

        public const String ClientPolicyName = "ClientPolicy";

        public const String BeerManageClientApiScopeName = "beer-clients";
        
        public const String BeerClientListScope = "beer-clients.list";
        public const String BeerClientModifyScope = "beer-clients.modify";
        public const String BeerClientDeleteScope = "beer-clients.delete";

        public const String BeerAppClientId = "control-center-app";
        public const String DaAPIAppClientId = "daapi-app";

        public const String DaAPIMangeScope = "daapi.manage";
        public const String DaAPIScopeName = "daapi";

        public const String ControlCenterManageScope = "controlcenter.user";
        public const String ControlCenterApiScopeName = "control-center";

        
    }
}

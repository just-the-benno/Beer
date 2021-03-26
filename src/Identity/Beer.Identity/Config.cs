// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Beer.Identity.Configuration;
using Beer.Identity.Utilities;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beer.Identity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                   };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope(AuthenticationDefaults.BeerUserListScope,"Read local password users"),
                new ApiScope(AuthenticationDefaults.BeerUserCreateScope,"Creates local users"),
                new ApiScope(AuthenticationDefaults.BeerUserDeleteScope,"Delete local users"),
                new ApiScope(AuthenticationDefaults.BeerUserResetPasswordScope,"Reset password of local users"),
                new ApiScope(AuthenticationDefaults.ControlCenterManageScope,"Reset password of local users"),
                new ApiScope(AuthenticationDefaults.DaAPIMangeScope,"manage entities within DaAPI"),
            };

        public static IEnumerable<ApiResource> GetApiResources =>
            new ApiResource[]
            {
                new ApiResource(AuthenticationDefaults.BeerManageUserApiScopeName, "Administration users that can use beer")
                {
                    Scopes = { AuthenticationDefaults.BeerUserListScope, AuthenticationDefaults.BeerUserCreateScope, AuthenticationDefaults.BeerUserDeleteScope, AuthenticationDefaults.BeerUserResetPasswordScope }
                },
                new ApiResource(AuthenticationDefaults.ControlCenterApiScopeName, "Basic access to the control center")
                {
                    Scopes = { AuthenticationDefaults.ControlCenterManageScope }
                },
                 new ApiResource(AuthenticationDefaults.DaAPIScopeName, "to manage DHCP and related stuff")
                {
                    Scopes = { AuthenticationDefaults.DaAPIMangeScope }
                },
            };

        public static Client GetBlazorWasmClient(BeerAuthenticationClient clientConfig)
        {
            var client = new Client
            {
                ClientId = clientConfig.ClientId,
                ClientSecrets = { new Secret(clientConfig.Password.Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RequireConsent = false,
                AllowOfflineAccess = true,
                
                AllowedScopes = (new[] { "openid", "profile" }).Union(clientConfig.Scopes).ToList(),
            };

            foreach (var item in clientConfig.Urls)
            {
                var parsedItem = item;
                if (parsedItem.EndsWith('/') == true)
                {
                    parsedItem = parsedItem.Remove(parsedItem.Length - 1);
                }

                client.RedirectUris.Add($"{parsedItem}/authentication/login-callback");
                client.AllowedCorsOrigins.Add(parsedItem);
                client.FrontChannelLogoutUri = $"{parsedItem}/authentication/logout-callback";
                client.PostLogoutRedirectUris.Add($"{parsedItem}/authentication/logout-callback");
            }

            return client;
        }


        //public static IEnumerable<Client> Clients =>
        //    new Client[]
        //    {
        //// m2m client credentials flow client
        //new Client
        //{
        //    ClientId = "m2m.client",
        //    ClientName = "Client Credentials Client",

        //    AllowedGrantTypes = GrantTypes.ClientCredentials,
        //    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

        //    AllowedScopes = { "scope1" }
        //},

        // interactive client using code flow + pkce
        //new Client
        //{
        //    ClientId = AuthenticationDefaults.BeerAppClientId,
        //    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

        //    AllowedGrantTypes = GrantTypes.Code,

        //    RedirectUris = { "https://localhost:44300/signin-oidc" },
        //    FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
        //    PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

        //    AllowOfflineAccess = true,
        //    AllowedScopes = { "openid", "profile", AuthenticationDefaults.BeerUserListScope, AuthenticationDefaults.BeerUserCreateScope, AuthenticationDefaults.BeerUserDeleteScope, AuthenticationDefaults.BeerUserResetPasswordScope, }
        //},
        //};
    }
}

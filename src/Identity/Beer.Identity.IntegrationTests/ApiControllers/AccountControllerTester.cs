using AngleSharp.Html.Dom;
using Beer.Identity.Services;
using Beer.TestHelper;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Beer.Identity.IntegrationTests.ApiControllers
{
    public class AccountControllerTester : BeerIdentityControllerTeserBase,
        IClassFixture<CustomWebApplicationFactory<FakeStartup>>
    {

        public AccountControllerTester(CustomWebApplicationFactory<FakeStartup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GET_CreateFirstUserOnlyPossibleIfThereIsNoUser()
        {
            Random random = new Random();

            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                context.Users.Add(new Infrastructure.Data.BeerUser
                {
                    UserName = "Name",
                    NormalizedUserName = "name",
                    PasswordHash = random.GetAlphanumericString(),
                });

                await context.SaveChangesAsync();

                var client = GetClientWithAnonymousUser(builder, new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                });

                var response = await client.GetAsync("/Account/CreateFirstUser");
                CheckRedirectResult(response, new Uri($"/Account/Login", UriKind.Relative));
            });
        }

        [Fact]
        public async Task Post_CreateFirstUserOnlyPossibleIfThereIsNoUser()
        {
            Random random = new Random();

            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                String username = "FirstUser";
                String password = "Awt124!!_235aq";

                context.Users.Add(new Infrastructure.Data.BeerUser
                {
                    UserName = "Name",
                    NormalizedUserName = "name",
                    PasswordHash = random.GetAlphanumericString(),
                });

                await context.SaveChangesAsync();

                var client = GetClientWithAnonymousUser(builder, new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                });

                FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<String, String>
                {
                    { nameof(CreateFirstIUserInputModel.Username), username },
                    { nameof(CreateFirstIUserInputModel.Password), password },
                    { nameof(CreateFirstIUserInputModel.PasswordConfirmation), password },
                });

                var response = await client.PostAsync("/Account/CreateFirstUser", content);
                IsBadRequest(response);
                
                //Need to find a way to bypass Antiforgery token/cookie
                //CheckRedirectResult(response, new Uri($"/Account/Login", UriKind.Relative));
            });
        }

        [Fact]
        public async Task FirstUserLoginCycle()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                String redirectUrl = "/BlubAction/242525?test=true";
                String username = "FirstUser";
                String password = "Awt124!!_235aq";
                String displayname = "A new user";
                String profilePictureUrl = "/img/pp/dogface-6.png";

                var client = GetClientWithAnonymousUser(builder, new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                });

                var firstLoginGetResponse = await client.GetAsync($"/Account/Login?returnUrl={redirectUrl}");
                Uri locationHeader = CheckRedirectResult(firstLoginGetResponse,new Uri($"/Account/CreateFirstUser?returnUrl={WebUtility.UrlEncode(redirectUrl)}",UriKind.Relative));

                var createFirstUserResponse = await client.GetAsync(locationHeader);
                Assert.True(createFirstUserResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.OK, createFirstUserResponse.StatusCode);

                var createFirstUserContent = await HtmlHelpers.GetDocumentAsync(createFirstUserResponse);
                var createFirstUserForm = (IHtmlFormElement)createFirstUserContent.QuerySelector("form[id='create-first-user']");
                Assert.NotNull(createFirstUserForm);
                //var pictureUrlValue = 
                //((IHtmlSelectElement)createFirstUserContent.QuerySelector("select[id='ProfilePictureUrl']")).Options.ElementAt(3).Value; 
                

                var createFirstUserPostResponse = await client.SendAsync(createFirstUserForm, new Dictionary<String, String>
                {
                    { nameof(CreateFirstIUserInputModel.Username), username },
                    { nameof(CreateFirstIUserInputModel.Password), password },
                    { nameof(CreateFirstIUserInputModel.PasswordConfirmation), password },
                    { nameof(CreateFirstIUserInputModel.DisplayName), displayname },
                    { nameof(CreateFirstIUserInputModel.ProfilePictureUrl), profilePictureUrl },
                });

                Uri createFistUserResponseHeaderLocation = CheckRedirectResult(createFirstUserPostResponse, new Uri($"/Account/Login?returnUrl={WebUtility.UrlEncode(redirectUrl)}", UriKind.Relative));
                var loginGetGetResponse = await client.GetAsync(createFistUserResponseHeaderLocation);

                var loginContent = await HtmlHelpers.GetDocumentAsync(loginGetGetResponse);
                var loginForm = (IHtmlFormElement)loginContent.QuerySelector("form[id='login']");
                Assert.NotNull(loginForm);

                var loginPostResponse = await client.SendAsync(loginForm, new Dictionary<String, String>
                {
                    { nameof(LoginViewModel.Username), username },
                    { nameof(LoginViewModel.Password), password },
                    { nameof(LoginViewModel.RememberLogin), "true" },
                });

                Uri finalRedirect = CheckRedirectResult(loginPostResponse, new Uri(redirectUrl, UriKind.Relative));

                var user = context.Users.FirstOrDefault(x => x.UserName == username);
                
                Assert.NotNull(user);

                var claims = await context.UserClaims.Where(x => x.UserId == user.Id).ToListAsync();
                Assert.NotEmpty(claims);

                Assert.Equal(displayname, claims.First(x => x.ClaimType == CustomClaimTypes.DisplayClaimsType).ClaimValue);
                Assert.Equal(profilePictureUrl,claims.First(x => x.ClaimType == CustomClaimTypes.ProfilePictureUrl).ClaimValue);
            });
        }

        private static Uri CheckRedirectResult(System.Net.Http.HttpResponseMessage response,Uri expectedUrl)
        {
            //Assert.True(firstLoginGetResponse.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            var locationHeader = response.Headers.Location;
            Assert.NotNull(locationHeader);
            Assert.Equal(expectedUrl, locationHeader);
            return locationHeader;
        }

    }
}

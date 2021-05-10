using Beer.Identity.Infrastructure.Repositories;
using Beer.TestHelper;
using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Beer.Identity.IntegrationTests.Infrastrucutre
{
    public class MartenBasedClientRepositoryTester
    {
        protected static async Task ExecuteDatabaseAwareTest(Func<DocumentStore, MartenBasedClientRepository, Task> executor) =>
            await MartenDbHelperUtility.ExecuteDatabaseAwareTest(executor);

        [Fact]
        public async Task AddClient()
        {
            Random random = new();

            await ExecuteDatabaseAwareTest(async (store, repo) =>
           {
               BeerClient client = new()
               {
                   ClientId = random.GetAlphanumericString(),
                   AllowedCorsOrigins = new[] { "https://myapp.com/" },
                   AllowedScopes = new[] { "test", "testus" },
                   DisplayName = random.GetAlphanumericString(),
                   HashedPassword = "asfw1424",
                   FrontChannelLogoutUri = "http://mylogout.com/logout/",
                   PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                   RedirectUris = new[] { "https://myapp2.com/login-callback/" },
                   RequirePkce = random.NextBoolean(),
               };

               var result = await repo.AddClient(client);
               Assert.True(result);

               Assert.NotEqual(Guid.Empty, client.Id);

               using (var querySessionn = store.QuerySession())
               {
                   var storedResult = await querySessionn.Query<BeerClient>().FirstOrDefaultAsync(x => x.Id == client.Id);

                   Assert.Equal(client.Id, storedResult.Id);
                   Assert.Equal(client.ClientId, storedResult.ClientId);
                   Assert.Equal(client.AllowedCorsOrigins, storedResult.AllowedCorsOrigins);
                   Assert.Equal(client.AllowedScopes, storedResult.AllowedScopes);
                   Assert.Equal(client.DisplayName, storedResult.DisplayName);
                   Assert.Equal(client.HashedPassword, storedResult.HashedPassword);
                   Assert.Equal(client.FrontChannelLogoutUri, storedResult.FrontChannelLogoutUri);
                   Assert.Equal(client.PostLogoutRedirectUris, storedResult.PostLogoutRedirectUris);
                   Assert.Equal(client.RedirectUris, storedResult.RedirectUris);
               }
           });
        }

        [Fact]
        public async Task CheckIfClientExists()
        {
            Random random = new();

            Guid existingId = random.NextGuid();
            Guid nonExistingId = random.NextGuid();

            await ExecuteDatabaseAwareTest(async (store, repo) =>
            {
                using (var session = store.LightweightSession())
                {
                    session.Store(new BeerClient
                    {
                        Id = existingId,
                        ClientId = random.GetAlphanumericString(),
                        AllowedCorsOrigins = new[] { "https://myapp.com/" },
                        AllowedScopes = new[] { "test", "testus" },
                        DisplayName = random.GetAlphanumericString(),
                        HashedPassword = "asfw1424",
                        FrontChannelLogoutUri = "http://mylogout.com/logout/",
                        PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                        RedirectUris = new[] { "https://myapp2.com/login-callback/" },
                    });

                    await session.SaveChangesAsync();
                }

                var existingResult = await repo.CheckIfClientExists(existingId);
                Assert.True(existingResult);

                var nonexistingResult = await repo.CheckIfClientExists(nonExistingId);
                Assert.False(nonexistingResult);
            });
        }

        [Fact]
        public async Task CheckIfClientIdExists()
        {
            Random random = new();

            String existingId = random.GetAlphanumericString();
            String nonExistingId = random.GetAlphanumericString();

            await ExecuteDatabaseAwareTest(async (store, repo) =>
            {
                using (var session = store.LightweightSession())
                {
                    session.Store(new BeerClient
                    {
                        Id = random.NextGuid(),
                        ClientId = existingId,
                        AllowedCorsOrigins = new[] { "https://myapp.com/" },
                        AllowedScopes = new[] { "test", "testus" },
                        DisplayName = random.GetAlphanumericString(),
                        HashedPassword = "asfw1424",
                        FrontChannelLogoutUri = "http://mylogout.com/logout/",
                        PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                        RedirectUris = new[] { "https://myapp2.com/login-callback/" },
                    });

                    await session.SaveChangesAsync();
                }

                var existingResult = await repo.CheckIfClientIdExists(existingId);
                Assert.True(existingResult);

                var nonexistingResult = await repo.CheckIfClientIdExists(nonExistingId);
                Assert.False(nonexistingResult);
            });
        }

        [Fact]
        public async Task DeleteClient()
        {
            Random random = new();

            Guid existingId = random.NextGuid();

            await ExecuteDatabaseAwareTest(async (store, repo) =>
            {
                using (var session = store.LightweightSession())
                {
                    session.Store(new BeerClient
                    {
                        Id = existingId,
                        ClientId = random.GetAlphanumericString(),
                        AllowedCorsOrigins = new[] { "https://myapp.com/" },
                        AllowedScopes = new[] { "test", "testus" },
                        DisplayName = random.GetAlphanumericString(),
                        HashedPassword = "asfw1424",
                        FrontChannelLogoutUri = "http://mylogout.com/logout/",
                        PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                        RedirectUris = new[] { "https://myapp2.com/login-callback/" },
                    });

                    await session.SaveChangesAsync();
                }

                var existingResult = await repo.DeleteClient(existingId);
                Assert.True(existingResult);

                using (var querySessionn = store.QuerySession())
                {
                    var storedResult = await querySessionn.Query<BeerClient>().FirstOrDefaultAsync(x => x.Id == existingId);
                    Assert.Null(storedResult);
                }
            });
        }

        [Fact]
        public async Task GetAllClientsSortedByName()
        {
            Random random = new();

            Guid firstId = random.NextGuid();
            Guid secondId = random.NextGuid();

            var firstBeerClient = new BeerClient
            {
                Id = firstId,
                DisplayName = "my first client",

                ClientId = random.GetAlphanumericString(),
                AllowedCorsOrigins = new[] { $"https://{random.GetAlphanumericString()}.com/" },
                AllowedScopes = new[] { $"test-{random.GetAlphanumericString()}", "testus" },
                HashedPassword = "asfw1424",
                FrontChannelLogoutUri = $"http://{random.GetAlphanumericString()}.com/logout/",
                PostLogoutRedirectUris = new[] { $"https://{random.GetAlphanumericString()}.com/logout-callback/" },
                RedirectUris = new[] { $"https://{random.GetAlphanumericString()}.com/login-callback/" },
            };

            var secondBeerClient = new BeerClient
            {
                Id = secondId,
                DisplayName = "a client",

                ClientId = random.GetAlphanumericString(),
                AllowedCorsOrigins = new[] { $"https://{random.GetAlphanumericString()}.com/" },
                AllowedScopes = new[] { $"test-{random.GetAlphanumericString()}", "testus" },
                HashedPassword = "asfw1424",
                FrontChannelLogoutUri = $"http://{random.GetAlphanumericString()}.com/logout/",
                PostLogoutRedirectUris = new[] { $"https://{random.GetAlphanumericString()}.com/logout-callback/" },
                RedirectUris = new[] { $"https://{random.GetAlphanumericString()}.com/login-callback/" },
            };

            await ExecuteDatabaseAwareTest(async (store, repo) =>
            {
                using (var session = store.LightweightSession())
                {
                    session.Store(firstBeerClient);
                    session.Store(secondBeerClient);

                    await session.SaveChangesAsync();
                }

                var existingResult = await repo.GetAllClientsSortedByName();
                Assert.NotNull(existingResult);
                Assert.NotEmpty(existingResult);
                Assert.Equal(2, existingResult.Count());

                {
                    var firstItem = existingResult.ElementAt(0);

                    Assert.Equal(secondBeerClient.Id, firstItem.SystemId);
                    Assert.Equal(secondBeerClient.DisplayName, firstItem.DisplayName);
                    Assert.Equal(secondBeerClient.ClientId, firstItem.ClientId);
                    Assert.Equal(secondBeerClient.AllowedCorsOrigins, firstItem.AllowedCorsOrigins);
                    Assert.Equal(secondBeerClient.AllowedScopes, firstItem.AllowedScopes);
                    Assert.Equal(secondBeerClient.FrontChannelLogoutUri, firstItem.FrontChannelLogoutUri);
                    Assert.Equal(secondBeerClient.PostLogoutRedirectUris, firstItem.PostLogoutRedirectUris);
                    Assert.Equal(secondBeerClient.RedirectUris, firstItem.RedirectUris);
                }

                {
                    var secomdItem = existingResult.ElementAt(1);

                    Assert.Equal(firstBeerClient.Id, secomdItem.SystemId);
                    Assert.Equal(firstBeerClient.DisplayName, secomdItem.DisplayName);
                    Assert.Equal(firstBeerClient.ClientId, secomdItem.ClientId);
                    Assert.Equal(firstBeerClient.AllowedCorsOrigins, secomdItem.AllowedCorsOrigins);
                    Assert.Equal(firstBeerClient.AllowedScopes, secomdItem.AllowedScopes);
                    Assert.Equal(firstBeerClient.FrontChannelLogoutUri, secomdItem.FrontChannelLogoutUri);
                    Assert.Equal(firstBeerClient.PostLogoutRedirectUris, secomdItem.PostLogoutRedirectUris);
                    Assert.Equal(firstBeerClient.RedirectUris, secomdItem.RedirectUris);
                }
            });
        }

        [Fact]
        public async Task UpdateClient()
        {
            Random random = new();

            Guid existingId = random.NextGuid();

            var exisitngBeerClient = new BeerClient
            {
                Id = existingId,
                ClientId = random.GetAlphanumericString(),
                AllowedCorsOrigins = new[] { "https://myapp.com/" },
                AllowedScopes = new[] { "test", "testus" },
                DisplayName = random.GetAlphanumericString(),
                HashedPassword = "asfw1424",
                FrontChannelLogoutUri = "http://mylogout.com/logout/",
                PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                RedirectUris = new[] { "https://myapp2.com/login-callback/" },
                RequirePkce = true,
            };

            await ExecuteDatabaseAwareTest(async (store, repo) =>
            {
                using (var session = store.LightweightSession())
                {
                    session.Store(exisitngBeerClient);

                    await session.SaveChangesAsync();
                }

                var clientToUpdate = new BeerClient
                {
                    Id = existingId,
                    ClientId = random.GetAlphanumericString(),
                    AllowedCorsOrigins = new[] { "https://myapp.com/" },
                    AllowedScopes = new[] { "test", "testus" },
                    DisplayName = random.GetAlphanumericString(),
                    HashedPassword = exisitngBeerClient.HashedPassword,
                    FrontChannelLogoutUri = "http://mylogout.com/logout/",
                    PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                    RedirectUris = new[] { "https://myapp2.com/login-callback/" },
                };

                var result = await repo.UpdateClient(clientToUpdate);
                Assert.True(result);

                using (var querySessionn = store.QuerySession())
                {
                    var storedResult = await querySessionn.Query<BeerClient>().FirstOrDefaultAsync(x => x.Id == existingId);

                    Assert.Equal(clientToUpdate.Id, storedResult.Id);
                    Assert.Equal(clientToUpdate.DisplayName, storedResult.DisplayName);
                    Assert.Equal(clientToUpdate.ClientId, storedResult.ClientId);
                    Assert.Equal(clientToUpdate.HashedPassword, storedResult.HashedPassword);

                    Assert.Equal(clientToUpdate.AllowedCorsOrigins, storedResult.AllowedCorsOrigins);
                    Assert.Equal(clientToUpdate.AllowedScopes, storedResult.AllowedScopes);
                    Assert.Equal(clientToUpdate.FrontChannelLogoutUri, storedResult.FrontChannelLogoutUri);
                    Assert.Equal(clientToUpdate.PostLogoutRedirectUris, storedResult.PostLogoutRedirectUris);
                    Assert.Equal(clientToUpdate.RedirectUris, storedResult.RedirectUris);

                    Assert.False(storedResult.RequirePkce);
                }
            });
        }

        [Fact]
        public async Task GetClientById_SystemId()
        {
            Random random = new();

            Guid existingId = random.NextGuid();

            var beerClien = new BeerClient
            {
                Id = existingId,
                ClientId = random.GetAlphanumericString(),
                AllowedCorsOrigins = new[] { "https://myapp.com/" },
                AllowedScopes = new[] { "test", "testus" },
                DisplayName = random.GetAlphanumericString(),
                HashedPassword = "asfw1424",
                FrontChannelLogoutUri = "http://mylogout.com/logout/",
                PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                RedirectUris = new[] { "https://myapp2.com/login-callback/" },
            };

            await ExecuteDatabaseAwareTest(async (store, repo) =>
            {
                using (var session = store.LightweightSession())
                {
                    session.Store(beerClien);

                    await session.SaveChangesAsync();
                }

                var result = await repo.GetClientById(existingId);
                Assert.NotNull(result);

                Assert.Equal(result.Id, result.Id);
                Assert.Equal(result.DisplayName, result.DisplayName);
                Assert.Equal(result.ClientId, result.ClientId);
                Assert.Equal(result.HashedPassword, result.HashedPassword);

                Assert.Equal(result.AllowedCorsOrigins, result.AllowedCorsOrigins);
                Assert.Equal(result.AllowedScopes, result.AllowedScopes);
                Assert.Equal(result.FrontChannelLogoutUri, result.FrontChannelLogoutUri);
                Assert.Equal(result.PostLogoutRedirectUris, result.PostLogoutRedirectUris);
                Assert.Equal(result.RedirectUris, result.RedirectUris);
            });
        }

        [Fact]
        public async Task GetClientById_ClientId()
        {
            Random random = new();

            String existingId = random.GetAlphanumericString();

            var beerClien = new BeerClient
            {
                Id = random.NextGuid(),
                ClientId = existingId,
                AllowedCorsOrigins = new[] { "https://myapp.com/" },
                AllowedScopes = new[] { "test", "testus" },
                DisplayName = random.GetAlphanumericString(),
                HashedPassword = "asfw1424",
                FrontChannelLogoutUri = "http://mylogout.com/logout/",
                PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                RedirectUris = new[] { "https://myapp2.com/login-callback/" },
            };

            await ExecuteDatabaseAwareTest(async (store, repo) =>
            {
                using (var session = store.LightweightSession())
                {
                    session.Store(beerClien);

                    await session.SaveChangesAsync();
                }

                var result = await repo.GetClientById(existingId);
                Assert.NotNull(result);

                Assert.Equal(result.Id, result.Id);
                Assert.Equal(result.DisplayName, result.DisplayName);
                Assert.Equal(result.ClientId, result.ClientId);
                Assert.Equal(result.HashedPassword, result.HashedPassword);

                Assert.Equal(result.AllowedCorsOrigins, result.AllowedCorsOrigins);
                Assert.Equal(result.AllowedScopes, result.AllowedScopes);
                Assert.Equal(result.FrontChannelLogoutUri, result.FrontChannelLogoutUri);
                Assert.Equal(result.PostLogoutRedirectUris, result.PostLogoutRedirectUris);
                Assert.Equal(result.RedirectUris, result.RedirectUris);
            });
        }

    }
}

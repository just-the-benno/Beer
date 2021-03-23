using Beer.Identity.Infrastructure.Data;
using Beer.TestHelper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beer.Identity.IntegrationTests.ApiControllers
{
    public abstract class BeerIdentityControllerTeserBase : IntergrationControllerTeserBase
    {
        protected CustomWebApplicationFactory<FakeStartup> Factory { get; private set; }
        protected const String _rootDir = "./Beer.Identity/";

        public BeerIdentityControllerTeserBase(CustomWebApplicationFactory<FakeStartup> factory)
        {
            Factory = factory;
        }

        protected HttpClient GetClientWithAnonymousUserAndSpecifiedBeerDb(DbContextOptionsBuilder<BeerIdentityContext> contextBuilder) =>
              GetClientWithSpecifiedBeerDb(contextBuilder, (services) => AddFakeAuthentication(services, "Bearer", false));

        protected HttpClient GetClientWithAuthenticatedUserAndSpecifiedBeerDb(DbContextOptionsBuilder<BeerIdentityContext> contextBuilder, Guid id, params String[] scopes) =>
            GetClientWithSpecifiedBeerDb(contextBuilder, (services) => AddFakeAuthentication(services, (ob) =>
            {
                ob.ShouldBeAuthenticated = true;
                ob.AddClaim("sub", id.ToString());
                ob.AddScopes(scopes ?? Array.Empty<String>());
            }));


        protected HttpClient GetClientWithSpecifiedBeerDb(DbContextOptionsBuilder<BeerIdentityContext> contextBuilder, Action<IServiceCollection> collectionModifier)
        {
            var factory = Factory.WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot(_rootDir);

                builder.ConfigureTestServices(services =>
                {
                    RemoveServiceFromCollection(services, typeof(DbContextOptions<BeerIdentityContext>));
                    services.Add(new ServiceDescriptor(
                        typeof(DbContextOptions<BeerIdentityContext>),
                        (pr) => contextBuilder.Options,
                        ServiceLifetime.Scoped
                        ));

                    collectionModifier(services);

                });
            });

            return factory.CreateClient();
        }

        protected HttpClient GetClientWithAnonymousUser(DbContextOptionsBuilder<BeerIdentityContext> contextBuilder, WebApplicationFactoryClientOptions options = null)
        {
            var factory = Factory.WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot(_rootDir);

                builder.ConfigureTestServices(services =>
                {
                    AddFakeAuthentication(services);
                    
                    RemoveServiceFromCollection(services, typeof(DbContextOptions<BeerIdentityContext>));
                    services.Add(new ServiceDescriptor(
                        typeof(DbContextOptions<BeerIdentityContext>),
                        (pr) => contextBuilder.Options,
                        ServiceLifetime.Scoped
                        ));
                });
            });

            return factory.CreateClient(options ?? new());
        }

        protected static async Task ExecuteDatabaseAwareTest(Func<DbContextOptionsBuilder<BeerIdentityContext>, BeerIdentityContext, Task> executor)
        {
            Random random1 = new Random();

            var config = ConfigurationUtility.GetConfig();

            String connectionString = $"{ConfigurationUtility.GetConnectionString("PostgresTest")};Database={random1.GetAlphanumericString()};";

            DbContextOptionsBuilder<BeerIdentityContext> dbContextOptionsBuilder = new();

            dbContextOptionsBuilder.UseNpgsql(connectionString, (ib) => ib.MigrationsAssembly(typeof(BeerIdentityContext).Assembly.FullName));
            DbContextOptions<BeerIdentityContext> contextOptions = dbContextOptionsBuilder.Options;

            BeerIdentityContext context = new BeerIdentityContext(contextOptions);

            try
            {
                await context.Database.MigrateAsync();
                await executor(dbContextOptionsBuilder, context);
            }
            finally
            {
                await context.Database.EnsureDeletedAsync();
            }
        }
    }
}

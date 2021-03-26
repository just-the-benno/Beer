using DaAPI.Host;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Beer.DaAPI.Service.API;
using Beer.TestHelper;
using Beer.DaAPI.Service.IntegrationTests;
using Microsoft.EntityFrameworkCore;
using Beer.DaAPI.Infrastructure.StorageEngine;

namespace DaAPI.IntegrationTests.ServiceBus
{
    public abstract class ServiceBusTesterBase : WebApplicationFactoryBase, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        protected ServiceBusTesterBase(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        protected static void AddDatabase(IServiceCollection services, String dbName)
        {
            DbContextOptions<StorageContext> contextOptions = DatabaseTestingUtility.GetTestDbContextOptions(dbName);
            ReplaceService(services, contextOptions);
        }

        protected (HttpClient client, IServiceBus serviceBus) GetTestClient(String dbfileName)
        {
            IServiceBus serviceBus = null;
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    AddDatabase(services, dbfileName);

                    services.AddSingleton<IServiceBus, MediaRBasedServiceBus>(sp =>
                    {
                        var bus = new MediaRBasedServiceBus(sp.GetService);
                        serviceBus = bus;

                        return bus;
                    }); ;
                });

            }).CreateClient();

            return (client, serviceBus);
        }
    }
}

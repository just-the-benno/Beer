using Beer.DaAPI.Infrastructure.StorageEngine;
using Beer.DaAPI.Service.IntegrationTests;
using Beer.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.IntegrationTests.Host
{
    public abstract class ControllerTesterBase : WebApplicationFactoryBase
    {
        protected static void AddFakeAuthentication(IServiceCollection services, String authenticaionSchema) => AddFakeAuthentication(services, authenticaionSchema, "daapi.manage");

        protected static void AddDatabase(IServiceCollection services, String dbName)
        {
            DbContextOptions<StorageContext> contextOptions = DatabaseTestingUtility.GetTestDbContextOptions(dbName);
            ReplaceService(services, contextOptions);
        }

        protected static StringContent GetContent<T>(T input) => new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");

    }


}

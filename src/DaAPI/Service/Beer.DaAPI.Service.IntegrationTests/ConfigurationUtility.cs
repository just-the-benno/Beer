using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.IntegrationTests
{
    public static class ConfigurationUtility
    {
        public static IConfiguration GetConfig()
        {
            var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<UnusedDummyClass>(true)
            .Build();

            return config;
        }

        public static String GetConnectionString(String name) => GetConfig().GetConnectionString(name);
    }
}

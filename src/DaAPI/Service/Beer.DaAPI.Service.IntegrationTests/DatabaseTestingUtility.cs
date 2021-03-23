using Beer.DaAPI.Infrastructure.StorageEngine;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.IntegrationTests
{
    public static class DatabaseTestingUtility
    {
        public static DbContextOptions<StorageContext> GetTestDbContextOptions(String dbName)
        {
            String dbConnectionString = $"{ConfigurationUtility.GetConnectionString("DaAPIDatabaseTestConnection")};Database={dbName};";

            DbContextOptionsBuilder<StorageContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<StorageContext>();
            dbContextOptionsBuilder.UseNpgsql(dbConnectionString, options =>
            {
                options.MigrationsAssembly(typeof(StorageContext).Assembly.FullName);
            });

            DbContextOptions<StorageContext> contextOptions = dbContextOptionsBuilder.Options;
            return contextOptions;
        }

        public static StorageContext GetTestDatabaseContext(String dbName)
        {
            DbContextOptions<StorageContext> contextOptions = GetTestDbContextOptions(dbName);
            StorageContext context = new StorageContext(contextOptions);

            return context;
        }

        public static async Task DeleteDatabase(string dbName)
        {
            var context = DatabaseTestingUtility.GetTestDatabaseContext(dbName);
            await context.Database.EnsureDeletedAsync();
        }
    }
}

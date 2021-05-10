using Beer.Identity.Infrastructure.Repositories;
using Beer.TestHelper;
using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.IntegrationTests
{
    public static class MartenDbHelperUtility
    {
        public static async Task ExecuteDatabaseAwareTest(Func<DocumentStore, MartenBasedClientRepository, Task> executor)
        {
            Random random1 = new();

            String dbName = $"MartenBased-{random1.GetAlphanumericString()}";

            String connectionString = $"{ConfigurationUtility.GetConnectionString("PostgresTest")};Database={dbName};";

            var store = DocumentStore.For((storeOptions) =>
            {
                storeOptions.CreateDatabasesForTenants(c =>
                {
                    c.ForTenant()
                       .WithEncoding("UTF-8")
                       .ConnectionLimit(-1)
                       .OnDatabaseCreated(_ =>
                       {

                       });
                });
                storeOptions.Connection(connectionString);
                storeOptions.AutoCreateSchemaObjects = AutoCreate.All;
            });

            MartenBasedClientRepository repo = new(store);

            try
            {
                await executor(store, repo);
            }
            finally
            {
                store.Advanced.Clean.CompletelyRemoveAll();
                store.Dispose();

                using (Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection($"{ConfigurationUtility.GetConnectionString("PostgresTest")};Database=postgres;"))
                {
                    await connection.OpenAsync();
                    Npgsql.NpgsqlCommand dropCmd = new Npgsql.NpgsqlCommand($"DROP DATABASE \"{dbName}\" WITH (FORCE);", connection);
                    await dropCmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}

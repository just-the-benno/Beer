﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Linq;

namespace Beer.DaAPI.Service.API
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
#if DEBUG
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Information)
#else
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
#endif
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                // uncomment to write to Azure diagnostics stream
                //.WriteTo.File(
                //    @"D:\home\LogFiles\Application\identityserver.txt",
                //    fileSizeLimitBytes: 1_000_000,
                //    rollOnFileSizeLimit: true,
                //    shared: true,
                //    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                Log.Information("Starting host...");
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

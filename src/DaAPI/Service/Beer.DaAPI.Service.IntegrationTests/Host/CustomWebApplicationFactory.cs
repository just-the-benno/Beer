using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beer.DaAPI.Service.API;
using DaAPI.Host;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace DaAPI.IntegrationTests.Host
{
    public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder(null)
                          .UseStartup<TEntryPoint>();
        }
    }
}


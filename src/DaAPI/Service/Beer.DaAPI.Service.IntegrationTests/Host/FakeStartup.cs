using Beer.DaAPI.Service.API;
using Beer.DaAPI.Service.API.Infrastrucutre;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DaAPI.IntegrationTests.Host
{
    public class FakeStartup : Startup
    {
        public FakeStartup(IWebHostEnvironment environment, IConfiguration configuration) : base(environment, configuration)
        {
        }

        protected override void ConfigureAuthentication(IServiceCollection services, AppSettings appSettings)
        {

        }
    }
}

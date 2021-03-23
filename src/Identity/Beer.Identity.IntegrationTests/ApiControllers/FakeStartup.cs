using Beer.Identity.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.IntegrationTests.ApiControllers
{
    public class FakeStartup : Startup
    {
        public FakeStartup(IWebHostEnvironment environment, IConfiguration configuration) : base(environment, configuration)
        {
        }

        protected override void ConfigureAuthentication(IServiceCollection services, AppConfiguration appSettings)
        {
            return;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
        }

        public override void Configure(IApplicationBuilder app)
        {
            base.Configure(app);
        }
    }
}

using Beer.ControlCenter.Service.API;
using Beer.ControlCenter.Service.API.Infrastrucutre;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.ControlCenter.Service.IntegrationTests.APITester
{
    public class FakeStartup : Startup
    {
        public FakeStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void ConfigureAuthentication(IServiceCollection services, AppSettings appSettings)
        {
        }
    }
}

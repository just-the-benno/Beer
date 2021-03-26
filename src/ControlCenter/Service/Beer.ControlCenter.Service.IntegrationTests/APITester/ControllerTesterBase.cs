using Beer.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.ControlCenter.Service.IntegrationTests.APITester
{
    public abstract class ControllerTesterBase : WebApplicationFactoryBase
    {
        protected static String GetBasePath()
        {
            String currentPath = System.IO.Path.GetFullPath(".");
            Int32 startIndex = currentPath.IndexOf("Beer.ControlCenter.Service.IntegrationTests");
            String basePath = currentPath.Substring(0, startIndex) + "Beer.ControlCenter.Service.API";

            return basePath;
        }

        protected static void AddFakeAuthentication(IServiceCollection services, String authenticaionSchema) => AddFakeAuthentication(services, authenticaionSchema, "controlcenter.user");
    }
}

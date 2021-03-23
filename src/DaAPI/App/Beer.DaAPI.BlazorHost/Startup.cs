using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beer.DaAPI.BlazorHost.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beer.DaAPI.BlazorHost
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected virtual void SetUrlsByTyeConfig(AppConfiguration config)
        {
            var authorityUrl = Configuration.GetServiceUri("identity", "https");
            if(authorityUrl != null)
            {
                config.OpenIdConnection.Authority = authorityUrl.ToString();
            }

            var ownUrl = Configuration.GetServiceUri("DaAPIApp", "https");
            if (ownUrl != null)
            {
                config.OpenIdConnection.RedirectUri =  $"{ownUrl}authentication/login-callback";
                config.OpenIdConnection.PostLogoutRedirectUri = $"{ownUrl}authentication/logout-callback";
            }

            var url = Configuration.GetServiceUri("DaAPIService", "https");
            if (url != null)
            {
                config.APIUrls["DaAPI"] = url.ToString();
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            AppConfiguration config = Configuration.GetSection("AppConfiguration").Get<AppConfiguration>();
#if DEBUG
            SetUrlsByTyeConfig(config);
#endif
            services.AddSingleton(config);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}

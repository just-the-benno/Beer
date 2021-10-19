using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beer.ControlCenter.Service.API.Infrastrucutre;
using Beer.Helper.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Beer.ControlCenter.Service.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        protected virtual void SetUrlsByTyeConfig(AppSettings settings)
        {
            var replacements = new[] { 
                ("DaAPIApp", "DaAPI-Blazor"),
                ("ControlCenterApp", "ControlCenter-BlazorApp"),
                ("BeerShark", "BeerShark-BlazorApp"),
            };

            var authorityUrl = Configuration.GetServiceUri("identity", "https");
            if (authorityUrl != null)
            {
                settings.JwtTokenAuthenticationOptions.AuthorityUrl = authorityUrl.RemoveTrailingSlash();
            }

            foreach (var item in replacements)
            {
                var tyeUrl = Configuration.GetServiceUri(item.Item1, "https");
                Console.WriteLine(tyeUrl.ToString());
                if (tyeUrl != null)
                {
                    settings.AppURIs[item.Item2] = tyeUrl.RemoveTrailingSlash();
                }
            }
        }

        private AppSettings GetApplicationConfiguration(IServiceCollection services)
        {
            AppSettings settings = Configuration.GetSection("AppSettings").Get<AppSettings>();
#if DEBUG
            SetUrlsByTyeConfig(settings);
#endif
            if (settings != null)
            {
                services.AddSingleton(settings);
                services.AddSingleton(settings.JwtTokenAuthenticationOptions);
            }

            return settings;
        }

        protected virtual void ConfigureAuthentication(IServiceCollection services, AppSettings appSettings)
        {
            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            {
                options.Authority = $"{appSettings.JwtTokenAuthenticationOptions.AuthorityUrl}";
                options.Audience = "control-center";
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = GetApplicationConfiguration(services);

            ConfigureAuthentication(services, appSettings);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ControlCenter-API", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "controlcenter.user");
                });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Beer.ControlCenter.Service.API", Version = "v1" });
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            services.AddCors((builder) =>
            {
                builder.AddPolicy("clientApps", (pb) =>
                {
                    pb.AllowAnyHeader().AllowAnyMethod().WithOrigins(appSettings.AppURIs["ControlCenter-BlazorApp"]);
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Beer.ControlCenter.Service.API v1"));
            }

            app.UseCors("clientApps");

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

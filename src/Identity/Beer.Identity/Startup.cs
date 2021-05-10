// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using IdentityServerHost.Quickstart.UI;
using Beer.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Reflection;
using Beer.Identity.Services;
using Beer.Identity.Utilities;
using MediatR;
using Beer.Identity.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Collections.Generic;
using System;
using Beer.Helper.API;
using System.Linq;
using Marten;
using Beer.Identity.Infrastructure.Repositories;

namespace Beer.Identity
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        protected virtual void ConfigureAuthentication(IServiceCollection services, AppConfiguration appSettings)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = AuthenticationDefaults.DefaultAuthenticationScheme;
                options.DefaultChallengeScheme = AuthenticationDefaults.DefaultChallengeScheme;
            }).AddJwtBearer(AuthenticationDefaults.BearerSchemaName, options =>
            {
                options.Authority = $"{appSettings.OpenIdConnectConfiguration.AuthorityUrl}";
                options.RequireHttpsMetadata = true;
                options.Audience = AuthenticationDefaults.BeerManageUserApiScopeName;
            });
        }

        protected virtual void SetUrlsByTyeConfig(AppConfiguration config)
        {
            config.BeerAuthenticationClients["ControlCenter"].SetUrlIfNotNull(Configuration.GetServiceUri("ControlCenterApp", "https"));
            config.BeerAuthenticationClients["DaAPI"].SetUrlIfNotNull(Configuration.GetServiceUri("DaAPIApp", "https"));

            var selfUrl = Configuration.GetServiceUri("Identity", "https");
            if (selfUrl != null)
            {
                config.OpenIdConnectConfiguration.AuthorityUrl = selfUrl.ToString();
            }
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            AppConfiguration config = Configuration.GetSection("AppConfiguration").Get<AppConfiguration>();
#if DEBUG
            SetUrlsByTyeConfig(config);
#endif
            services.AddSingleton(config);

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            var mvcBuilder = services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    {
                        var assemblyName = new AssemblyName(typeof(SharedResources).GetTypeInfo().Assembly.FullName);
                        return factory.Create(nameof(SharedResources), assemblyName.Name);
                    };
                });

#if DEBUG
            mvcBuilder.AddRazorRuntimeCompilation();
#endif

            services.AddDbContext<BeerIdentityContext>(options =>
               options.UseNpgsql(Configuration.GetConnectionString("BeerIdentityContext"), (ib) => ib.MigrationsAssembly(typeof(BeerIdentityContext).Assembly.FullName)));


            var cString = Configuration.GetConnectionString("BeerIdentityDocumentStore");
            if (String.IsNullOrEmpty(cString) == false)
            {
                services.AddMarten(storeOptions =>
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
                    storeOptions.Connection(cString);
                    storeOptions.AutoCreateSchemaObjects = AutoCreate.All;
                });
            }

            services.AddScoped<IClientRepository, MartenBasedClientRepository>();

            services.AddIdentity<BeerUser, IdentityRole>()
                .AddEntityFrameworkStores<BeerIdentityContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                //// see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                //options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.GetApiResources)
                .AddClientStore<HybridClientStore>()

                .AddAspNetIdentity<BeerUser>().AddProfileService<CustomProfileService>()
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseNpgsql(Configuration.GetConnectionString("OperationalStorage"), (ib) => ib.MigrationsAssembly(typeof(Startup).Assembly.FullName));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                });

            if (Environment.IsDevelopment() || String.IsNullOrEmpty(config.IdentityServerOptions.SigningCertificate) == true)
            {
                // not recommended for production - you need to store your key material somewhere secure
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                builder.AddSigningCredential(config.IdentityServerOptions.SigningCertificate);
                builder.AddValidationKey(config.IdentityServerOptions.ValidationCertificate);
            }

            services.AddScoped<HybridClientStore>(sp => new HybridClientStore(sp.GetService<IClientRepository>(), new IdentityServer4.Models.Client[] {
                    Config.GetBlazorWasmClient(config.BeerAuthenticationClients["ControlCenter"].SetClientId(AuthenticationDefaults.BeerAppClientId).SetScopes(AuthenticationDefaults.ControlCenterManageScope, AuthenticationDefaults.BeerUserListScope, AuthenticationDefaults.BeerUserCreateScope, AuthenticationDefaults.BeerUserDeleteScope, AuthenticationDefaults.BeerUserResetPasswordScope,AuthenticationDefaults.BeerClientListScope,AuthenticationDefaults.BeerClientModifyScope,AuthenticationDefaults.BeerClientDeleteScope)),
                    Config.GetBlazorWasmClient(config.BeerAuthenticationClients["DaAPI"].SetClientId(AuthenticationDefaults.DaAPIAppClientId).SetScopes(AuthenticationDefaults.DaAPIMangeScope)),
                }));

            services.AddAuthentication();

            ConfigureAuthentication(services, config);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthenticationDefaults.LocalUserPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", AuthenticationDefaults.BeerUserListScope, AuthenticationDefaults.BeerUserCreateScope, AuthenticationDefaults.BeerUserDeleteScope, AuthenticationDefaults.BeerUserResetPasswordScope);
                });

                options.AddPolicy(AuthenticationDefaults.ClientPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", AuthenticationDefaults.BeerClientListScope, AuthenticationDefaults.BeerClientModifyScope, AuthenticationDefaults.BeerClientDeleteScope);
                });
            });

            services.AddScoped<ILocalUserService, LocalUserManagerService>();
            services.AddLogging();

            services.AddHttpContextAccessor();
            services.AddScoped<IUserIdTokenExtractor, HttpContextBasedUserIdTokenExtractor>();
            services.AddTransient<IProfilePictureService, SimpleProfilePictureService>();

            services.AddMediatR(typeof(Startup).Assembly);

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            List<String> clientUrls = new();
            foreach (var item in config.BeerAuthenticationClients)
            {
                clientUrls.AddRange(item.Value.Urls);
            }

            services.AddCors((builder) =>
            {
                builder.AddPolicy("clientApps", (pb) =>
                {
                    pb.AllowAnyHeader().AllowAnyMethod().WithOrigins(clientUrls.ToArray());
                });
            });
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var supportedCultures = new[] { "en", "de" };
            var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);

            app.UseStaticFiles();

            app.UseCors("clientApps");

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}

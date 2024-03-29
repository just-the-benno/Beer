﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Notifications;
using Beer.DaAPI.Core.Notifications.Actors;
using Beer.DaAPI.Core.Notifications.Conditions;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Service.API.Infrastrucutre;
using Beer.DaAPI.Infrastructure.FilterEngines.DHCPv4;
using Beer.DaAPI.Infrastructure.FilterEngines.DHCPv6;
using Beer.DaAPI.Infrastructure.InterfaceEngines;
using Beer.DaAPI.Infrastructure.LeaseEngines.DHCPv4;
using Beer.DaAPI.Infrastructure.LeaseEngines.DHCPv6;
using Beer.DaAPI.Infrastructure.NotificationEngine;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.MessageHandler;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.Services;
using Beer.DaAPI.Infrastructure.StorageEngine;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Shared.JsonConverters;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beer.Helper.API;
using Beer.DaAPI.Service.API.Application.Commands;
using Beer.DaAPI.Service.Infrastructure.StorageEngine;
using EventStore.Client;
using Beer.DaAPI.Infrastructure.Tracing;
using System.Net.Http;
using Beer.DaAPI.Service.API.Hubs.Tracing;

namespace Beer.DaAPI.Service.API
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

        protected virtual void SetUrlsByTyeConfig(AppSettings settings)
        {

            var authorityUrl = Configuration.GetServiceUri("identity", "https");
            if (authorityUrl != null)
            {
                settings.JwtTokenAuthenticationOptions.AuthorityUrl = authorityUrl.RemoveTrailingSlash();
            }

            var daapiSpaClientUrl = Configuration.GetServiceUri("DaAPIApp", "https");
            if (daapiSpaClientUrl != null)
            {
                settings.AppURIs["DaAPI-Blazor"] = daapiSpaClientUrl.RemoveTrailingSlash();
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
                services.AddSingleton(settings.EventStoreSettings);
            }

            return settings;
        }

        protected virtual void ConfigureAuthentication(IServiceCollection services, AppSettings appSettings)
        {
            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            {
                options.Authority = $"{appSettings.JwtTokenAuthenticationOptions.AuthorityUrl}";
                options.RequireHttpsMetadata = false;
                options.Audience = "daapi";
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = GetApplicationConfiguration(services);

            var mvcBuilder = services.AddControllersWithViews()
                .AddNewtonsoftJson((options) =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Converters.Add(new DHCPv6ScopePropertyRequestJsonConverter());
                    options.SerializerSettings.Converters.Add(new DHCPv4ScopePropertyRequestJsonConverter());
                    options.SerializerSettings.Converters.Add(new DUIDJsonConverter());
                });

#if DEBUG
            mvcBuilder.AddRazorRuntimeCompilation();
#endif
            ConfigureAuthentication(services, appSettings);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DaAPI-API", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "daapi.manage");
                });
            });

            services.AddDbContext<StorageContext>((x) => x.UseNpgsql(Configuration.GetConnectionString("DaAPIDb"), option =>
    option.MigrationsAssembly(typeof(StorageContext).Assembly.FullName)), ServiceLifetime.Transient, ServiceLifetime.Transient);

            services.AddSingleton<IServiceBus, MediaRBasedServiceBus>();
            services.AddSingleton<ISerializer, JSONBasedSerializer>();

            services.AddSingleton(sp =>
            {
                var storageEngine = sp.GetRequiredService<IDHCPv6StorageEngine>();
                DHCPv6RootScope scope = storageEngine.GetRootScope().GetAwaiter().GetResult();

                foreach (var item in scope.GetRootScopes())
                {
                    foreach (var childScope in item.GetAllChildScopes())
                    {
                        foreach (var lease in childScope.Leases.GetAllLeases())
                        {
                            if (lease.PrefixDelegation != null && lease.PrefixDelegation != DHCPv6PrefixDelegation.None)
                            {
                                Console.WriteLine($"lease {lease.Address} with prefix {lease.PrefixDelegation.NetworkAddress}/{lease.PrefixDelegation.Mask.Identifier.Value}");
                            }
                            else
                            {
                                Console.WriteLine($"lease {lease.Address} without  prefix ");
                            }
                        }
                    }
                }

                return scope;
            });

            services.AddTransient<IDHCPv6ServerPropertiesResolver, DatabaseDHCPv6ServerPropertiesResolver>();
            services.AddSingleton<IScopeResolverManager<DHCPv6Packet, IPv6Address>, DHCPv6ScopeResolverManager>();
            services.AddSingleton<IDHCPv6PacketFilterEngine, SimpleDHCPv6PacketFilterEngine>();
            services.AddTransient<IDHCPv6LeaseEngine, DHCPv6LeaseEngine>();

            services.AddSingleton<IDHCPv6InterfaceEngine, DHCPv6InterfaceEngine>();
            services.AddTransient<IDHCPv6StorageEngine, DHCPv6StorageEngine>();
            services.AddTransient<IDHCPv6ReadStore, StorageContext>();
            services.AddTransient<IReadStore, StorageContext>();

            var esdbSettings = EventStoreClientSettings.Create(Configuration.GetConnectionString("ESDB"));
            esdbSettings.DefaultCredentials = new UserCredentials(appSettings.EventStoreSettings.Username, appSettings.EventStoreSettings.Password);

            services.AddTransient<ITracingStore, DapperBasedTracingStore>(sp => new DapperBasedTracingStore(Configuration.GetConnectionString("DaAPIDb"), sp.GetService<ILogger<DapperBasedTracingStore>>()));

            services.AddSingleton(new EventStoreBasedStoreConnenctionOptions(new EventStoreClient(esdbSettings), appSettings.EventStoreSettings.Prefix));

            services.AddSingleton<IDHCPv6EventStore, EventStoreBasedStore>();

            services.AddTransient<INotificationHandler<DHCPv6PacketArrivedMessage>>(sp => new DHCPv6PacketArrivedMessageHandler(
               sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv6PacketFilterEngine>(), sp.GetService<ILogger<DHCPv6PacketArrivedMessageHandler>>()));

            services.AddTransient<INotificationHandler<ValidDHCPv6PacketArrivedMessage>>(sp => new ValidDHCPv6PacketArrivedMessageHandler(
              sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv6LeaseEngine>(), sp.GetService<ILogger<ValidDHCPv6PacketArrivedMessageHandler>>()));

            services.AddTransient<INotificationHandler<InvalidDHCPv6PacketArrivedMessage>>(sp => new InvalidDHCPv6PacketArrivedMessageHandler(
           sp.GetRequiredService<IDHCPv6StorageEngine>(), sp.GetService<ILogger<InvalidDHCPv6PacketArrivedMessageHandler>>()));

            services.AddTransient<INotificationHandler<DHCPv6PacketFilteredMessage>>(sp => new DHCPv6PacketFileteredMessageHandler(
              sp.GetRequiredService<IDHCPv6StorageEngine>(), sp.GetService<ILogger<DHCPv6PacketFileteredMessageHandler>>()));

            services.AddTransient<INotificationHandler<DHCPv6PacketReadyToSendMessage>>(sp => new DHCPv6PacketReadyToSendMessageHandler(
              sp.GetRequiredService<IDHCPv6InterfaceEngine>(), sp.GetService<ILogger<DHCPv6PacketReadyToSendMessageHandler>>()));

            services.AddTransient<INotificationHandler<NewTriggerHappendMessage>>(sp => new NewTriggerHappendMessageHandler(
              sp.GetRequiredService<INotificationEngine>(), sp.GetService<ILogger<NewTriggerHappendMessageHandler>>()));

            services.AddTransient<INotificationHandler<UnableToSentPacketMessage>>(sp => new WriteCriticalEventsToLogHandler(
                sp.GetService<ILogger<WriteCriticalEventsToLogHandler>>()));

            services.AddTransient<INotificationHandler<TracingRecordAppendedMessage>, TracingRecordAppendedMessageHandler>();
            services.AddTransient<INotificationHandler<TracingStreamStartedMessage>, TracingStreamStartedMessageHandler>();

            services.AddTransient<DHCPv6RateLimitBasedFilter>();
            services.AddTransient<DHCPv6PacketConsistencyFilter>();

            services.AddTransient<DHCPv4RateLimitBasedFilter>();
            services.AddTransient<UnkownDHCPv4RequestTypePaketFilter>();

            services.AddSingleton<INotificationEngine, NotificationEngine>();
            services.AddSingleton<INotificationActorFactory, ServiceProviderBasedNotificationActorFactory>();
            services.AddSingleton<INotificationConditionFactory, ServiceProviderBasedNotificationConditionFactory>();
            services.AddTransient<INxOsDeviceConfigurationService, HttpBasedNxOsDeviceConfigurationService>(sp => new HttpBasedNxOsDeviceConfigurationService(
               new HttpClient(new HttpClientHandler
               {
                   ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
               }), sp.GetService<ILogger<HttpBasedNxOsDeviceConfigurationService>>()
               ));

            services.AddTransient<NxOsStaticRouteUpdaterNotificationActor>();
            services.AddTransient<NxOsStaticRouteCleanerNotificationActor>();
            services.AddTransient<IDHCPv6PrefixCollector, StorageContext>();

            services.AddTransient<DHCPv6ScopeIdNotificationCondition>();
            services.AddTransient<TimerIntervalNotificationCondition>();

            services.AddSingleton(sp =>
            {
                var storageEngine = sp.GetRequiredService<IDHCPv4StorageEngine>();
                DHCPv4RootScope scope = storageEngine.GetRootScope().GetAwaiter().GetResult();
                return scope;
            });

            //services.AddTransient<IDHCPv4ServerPropertiesResolver, DatabaseDHCPv4ServerPropertiesResolver>();
            services.AddSingleton<IScopeResolverManager<DHCPv4Packet, IPv4Address>, DHCPv4ScopeResolverManager>();
            services.AddSingleton<IDHCPv4PacketFilterEngine, SimpleDHCPv4PacketFilterEngine>();
            services.AddTransient<IDHCPv4LeaseEngine, DHCPv4LeaseEngine>();

            services.AddSingleton<IDHCPv4InterfaceEngine, DHCPv4InterfaceEngine>();
            services.AddTransient<IDHCPv4StorageEngine, DHCPv4StorageEngine>();
            services.AddTransient<IDHCPv4ReadStore, StorageContext>();
            services.AddSingleton<IDHCPv4EventStore, EventStoreBasedStore>();

            services.AddTransient<INotificationHandler<DHCPv4PacketArrivedMessage>>(sp => new DHCPv4PacketArrivedMessageHandler(
                sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv4PacketFilterEngine>(), sp.GetService<ILogger<DHCPv4PacketArrivedMessageHandler>>()));

            services.AddTransient<INotificationHandler<InvalidDHCPv4PacketArrivedMessage>>(sp => new InvalidDHCPv4PacketArrivedMessageHandler(
                sp.GetRequiredService<IDHCPv4StorageEngine>(), sp.GetService<ILogger<InvalidDHCPv4PacketArrivedMessageHandler>>()));

            services.AddTransient<INotificationHandler<ValidDHCPv4PacketArrivedMessage>>(sp => new ValidDHCPv4PacketArrivedMessageHandler(
           sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv4LeaseEngine>(), sp.GetService<ILogger<ValidDHCPv4PacketArrivedMessageHandler>>()));

            services.AddTransient<INotificationHandler<DHCPv4PacketFilteredMessage>>(sp => new DHCPv4PacketFileteredMessageHandler(
              sp.GetRequiredService<IDHCPv4StorageEngine>(), sp.GetService<ILogger<DHCPv4PacketFileteredMessageHandler>>()));

            services.AddTransient<INotificationHandler<DHCPv4PacketReadyToSendMessage>>(sp => new DHCPv4PacketReadyToSendMessageHandler(
              sp.GetRequiredService<IDHCPv4InterfaceEngine>(), sp.GetService<ILogger<DHCPv4PacketReadyToSendMessageHandler>>()));

            services.AddSingleton<ServiceFactory>(p => p.GetService);
            services.AddLogging();

            services.AddHttpContextAccessor();
            services.AddScoped<IUserIdTokenExtractor, HttpContextBasedUserIdTokenExtractor>();
            services.AddMediatR((config) =>
            {
                config.AsScoped();
            },
                typeof(Startup).Assembly);
            services.AddHostedService<HostedService.LeaseTimerHostedService>();
            services.AddHostedService<HostedService.CleanupDatabaseTimerHostedService>();
            services.AddHostedService<HostedService.NotificationTimerHostedService>();

#if DEBUG
            services.AddHostedService<HostedService.FakeTracingStreamEmitter>();
#endif
            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            services.AddSingleton<IDeviceService>(new InMemoryDeviceService(null));

            services.AddCors((builder) =>
            {
                builder.AddPolicy("clientApps", (pb) =>
                {
                    pb.AllowAnyHeader().AllowAnyMethod().WithOrigins(appSettings.AppURIs.Values.ToArray());
                });
            });

            services.AddTransient<ITracingManager, TracingManager>();
            services.AddSignalR((opt) =>
           {

           });

        }

        public async void Configure(IApplicationBuilder app, IServiceProvider provider)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("clientApps");

            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<TracingHub>("/tracing");
            });

            var storageContext = provider.GetService<StorageContext>();
            if (storageContext.Database.IsNpgsql() == true)
            {
                storageContext.Database.Migrate();
                {
                    var packetsToMigrate = ((IQueryable<DHCPv4PacketHandledEntryDataModel>)storageContext.DHCPv4PacketEntries).Where(x => x.Version < 2).ToList();
                    foreach (var item in packetsToMigrate)
                    {
                        item.UpgradeToVersion2();
                    }
                }
                {
                    var packetsToMigrate = ((IQueryable<DHCPv6PacketHandledEntryDataModel>)storageContext.DHCPv6PacketEntries).Where(x => x.Version < 2).ToList();
                    foreach (var item in packetsToMigrate)
                    {
                        item.UpgradeToVersion2();
                    }
                }

                storageContext.SaveChanges();
            }

            var deviceService = provider.GetService<IDeviceService>() as IManagleableDeviceService;
            if (deviceService != null)
            {
                deviceService.AddDevices(storageContext.GetAllDevices());
            }
#if DEBUG
            //var seeder = new DatabaseSeeder();
            //seeder.SeedDatabase(false, storageContext).GetAwaiter().GetResult();
#endif
            IDHCPv6PacketFilterEngine dhcpv6PacketFilterEngine = provider.GetService<IDHCPv6PacketFilterEngine>();
            dhcpv6PacketFilterEngine.AddFilter(provider.GetRequiredService<DHCPv6RateLimitBasedFilter>());
            dhcpv6PacketFilterEngine.AddFilter(provider.GetRequiredService<DHCPv6PacketConsistencyFilter>());

            IDHCPv4PacketFilterEngine dhcpv4PacketFilterEngine = provider.GetService<IDHCPv4PacketFilterEngine>();
            dhcpv4PacketFilterEngine.AddFilter(provider.GetRequiredService<DHCPv4RateLimitBasedFilter>());
            dhcpv4PacketFilterEngine.AddFilter(provider.GetRequiredService<UnkownDHCPv4RequestTypePaketFilter>());

            var storage = provider.GetService<IDHCPv6ReadStore>();
            var serverproperties = storage.GetServerProperties().GetAwaiter().GetResult();
            if (serverproperties != null && serverproperties.ServerDuid != null)
            {
                dhcpv6PacketFilterEngine.AddFilter(new DHCPv6PacketServerIdentifierFilter(serverproperties.ServerDuid, provider.GetService<ILogger<DHCPv6PacketServerIdentifierFilter>>()));
            }

            var dhcpv6InterfaceEngine = provider.GetService<IDHCPv6InterfaceEngine>();
            dhcpv6InterfaceEngine.Initialize().GetAwaiter().GetResult();

            var dhcpv4InterfaceEngine = provider.GetService<IDHCPv4InterfaceEngine>();
            dhcpv4InterfaceEngine.Initialize().GetAwaiter().GetResult();

            var notificationEngine = provider.GetService<INotificationEngine>();
            notificationEngine.Initialize().GetAwaiter().GetResult();
        }
    }
}
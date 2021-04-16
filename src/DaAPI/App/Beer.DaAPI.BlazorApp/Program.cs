using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Beer.DaAPI.BlazorApp.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Beer.DaAPI.BlazorApp.Helper;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Beer.WASM.Helper;
using MudBlazor.Services;
using Microsoft.JSInterop;
using System.Globalization;
using FluentValidation;
using MudBlazor;
using Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes;

namespace Beer.DaAPI.BlazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<Beer.DaAPI.BlazorApp.App>("#app");

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 5000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

         
            builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton,
                (x =>  x.InterfaceType != typeof(String)));

            builder.Services.AddHttpClient<DaAPIService>(client =>
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                 .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services.AddSingleton<DHCPv6PacketOptionCodeToNameConverter>();
            builder.Services.AddSingleton<DHCPPacketResponseCodeHelper>();

            builder.Services.AddSingleton<DHCPv4PacketOptionCodeToNameConverter>();
            builder.Services.AddSingleton<DHCPPacketResponseCodeHelper>();
            builder.Services.AddSingleton<DHCPv4ScopePropertyTypeNameConverter>();
            builder.Services.AddSingleton<DHCPv4ScopeResolverPropertyValyeTypeNameConverter>();

            builder.Services.AddApiAuthorization((opt) =>
            {
                opt.ProviderOptions.ConfigurationEndpoint = "/Configuration/OidcClientConfig";
            });

            builder.Services.AddScoped<SignOutService>();

            builder.Services.AddLocalization((opt) =>
           {
               opt.ResourcesPath = "Resources";
           });

            HttpClient configurationLoader = new()
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            };

            var urlDict = (await configurationLoader.GetFromJsonAsync<Dictionary<String, String>>("/Configuration/APIs")).ToDictionary(x => x.Key.ToLower(), x => x.Value);


            builder.Services.AddHttpClient<DaAPIService>(client =>
                 client.BaseAddress = new Uri(urlDict["daapi"]))
                 .AddHttpMessageHandler((provider) => new ConfigurableAuthorizationMessageHandler(urlDict["daapi"], provider.GetRequiredService<IAccessTokenProvider>(), provider.GetRequiredService<NavigationManager>()));


            //builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            var host = builder.Build();
            var jsInterop = host.Services.GetRequiredService<IJSRuntime>();
            var result = await jsInterop.InvokeAsync<string>("getAppCulture");
            if (result != null)
            {
                var culture = new CultureInfo(result);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }

            await host.RunAsync();
        }
    }
}

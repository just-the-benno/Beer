using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;
using System.Globalization;
using Beer.ControlCenter.BlazorApp.Services;
using Microsoft.Extensions.Http;
using System.Linq;
using Beer.ControlCenter.BlazorApp.Util;
using Microsoft.AspNetCore.Components;
using FluentValidation;
using MudBlazor;
using Beer.WASM.Helper;

namespace Beer.ControlCenter.BlazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

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
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddApiAuthorization((opt) =>
            {
                opt.ProviderOptions.ConfigurationEndpoint = "/Configuration/OidcClientConfig";
            });

            builder.Services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            HttpClient configurationLoader = new()
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            };

            var urlDict = (await configurationLoader.GetFromJsonAsync<Dictionary<String, String>>("/Configuration/Apps")).ToDictionary(x => x.Key.ToLower(), x => x.Value);

            builder.Services.AddHttpClient<IBeerUserService, HttpClientBasedBeerUserService>(client =>
                 client.BaseAddress = new Uri(urlDict["beeridentity"]))
                 .AddHttpMessageHandler( (provider) => new ConfigurableAuthorizationMessageHandler(urlDict["beeridentity"], provider.GetRequiredService<IAccessTokenProvider>(),provider.GetRequiredService<NavigationManager>()));

            builder.Services.AddHttpClient<IControlCenterService, HttpClientBasedControlCenterService>(client =>
                 client.BaseAddress = new Uri(urlDict["controlcenterapi"]))
                 .AddHttpMessageHandler((provider) => new ConfigurableAuthorizationMessageHandler(urlDict["controlcenterapi"], provider.GetRequiredService<IAccessTokenProvider>(), provider.GetRequiredService<NavigationManager>()));


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

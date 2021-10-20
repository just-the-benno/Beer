using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Beer.BeerShark.BlazorApp.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor.Services;

namespace Beer.BeerShark.BlazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddMudServices();

            builder.Services.AddLocalization((opt) =>
            {
                opt.ResourcesPath = "Resources";
            });

            builder.Services.AddApiAuthorization((opt) =>
            {
                opt.ProviderOptions.ConfigurationEndpoint = "/Configuration/OidcClientConfig";
            });


            HttpClient configurationLoader = new()
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            };

            var apiUrlDict = (await configurationLoader.GetFromJsonAsync<Dictionary<String, String>>("/Configuration/APIs")).ToDictionary(x => x.Key.ToLower(), x => x.Value);
            var appUrlDict = (await configurationLoader.GetFromJsonAsync<Dictionary<String, String>>("/Configuration/Apps")).ToDictionary(x => x.Key.ToLower(), x => x.Value);

            builder.Services.AddSingleton(new BeerAppsService(appUrlDict));

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

using Beer.ControlCenter.BlazorHost.Configuration;
using Beer.OIDCOptionHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorHost.Controllers
{
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly AppConfiguration _appConfig;

        public ConfigurationController(AppConfiguration appConfig)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        [AllowAnonymous]
        [HttpGet("/Configuration/OidcClientConfig")]
        public IActionResult GetOidcClientConfig()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            options.Converters.Add(new OpenIdConnectionConfigurationJsonConverter());

            return base.Ok(JsonSerializer.Serialize(_appConfig.OpenIdConnection, options));
        }

        [AllowAnonymous]
        [HttpGet("/Configuration/Apps")]
        public IActionResult GetAppUrls()
        {
            return base.Ok(_appConfig.AppUrls);
        }
    }
}

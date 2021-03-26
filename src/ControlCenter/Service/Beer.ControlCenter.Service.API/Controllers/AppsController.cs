using Beer.ControlCenter.Service.API.Infrastrucutre;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.Service.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "ControlCenter-API")]
    [ApiController]
    public class AppsController : ControllerBase
    {
        private readonly AppSettings _settings;

        public AppsController(AppSettings settings)
        {
            this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        [HttpGet("/api/Apps/Urls")]
        public IActionResult GetUrls()
        {
            return base.Ok(_settings.AppURIs);
        }
    }
}

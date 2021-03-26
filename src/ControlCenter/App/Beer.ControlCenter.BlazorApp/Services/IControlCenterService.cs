using Beer.ControlCenter.BlazorApp.Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorApp.Services
{
    public interface IControlCenterService
    {
        Task<IDictionary<String,String>> GetAppUrls();
    }
}

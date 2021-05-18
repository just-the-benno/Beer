using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Services
{
    public class BeerAppsService
    {
        private readonly IDictionary<string, string> _appUrls;

        public BeerAppsService(IDictionary<String,String> appUrls)
        {
            this._appUrls = appUrls;
        }

        private String GetValueOrEmpty(String key) => _appUrls.ContainsKey(key) == true ? _appUrls[key] : String.Empty;

        public String GetControlCenterAppUrl() => GetValueOrEmpty("controlcenter");



    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Configuration
{
    public class BeerAuthenticationClient
    {
        public String Password { get; set; }
        public String ClientId {get; set; }
        public IEnumerable<String> Scopes { get; set; }
        public IEnumerable<String> Urls { get; set; }

        internal void SetUrlIfNotNull(Uri? controlCenterAppUrl)
        {
            if(controlCenterAppUrl != null)
            {
                var url = controlCenterAppUrl.ToString();
                if(url.EndsWith('/') == true)
                {
                    url = url.Substring(0, url.Length - 1);
                }

                Urls = new String[] { url };
            }
        }

        internal BeerAuthenticationClient SetClientId(string clientId)
        {
            ClientId = clientId;
            return this;
        }

        internal BeerAuthenticationClient SetScopes(params String[] scopes)
        {
            Scopes = new List<String>(scopes);
            return this;
        }

        internal BeerAuthenticationClient SetScopes(object daAPIManage)
        {
            throw new NotImplementedException();
        }
    }
}

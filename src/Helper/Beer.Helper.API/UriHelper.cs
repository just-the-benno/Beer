using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.Helper.API
{
    public static class UriHelper
    {
        public static String RemoveTrailingSlash(this Uri input)
        {
            var inputAsString = input.ToString();
            if (inputAsString.EndsWith('/') == true)
            {
                inputAsString = inputAsString.Substring(0, inputAsString.Length - 1);
            }

            return inputAsString;
        }
    }
}

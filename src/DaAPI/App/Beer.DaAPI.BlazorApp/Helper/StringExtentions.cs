using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public static class StringExtentions
    {
        public static IPv4Address AsIPv4Address(this String input)
        {
            try
            {
                return IPv4Address.FromString(input);
            }
            catch (Exception)
            {
                return IPv4Address.Empty;
            }
        }
    }
}

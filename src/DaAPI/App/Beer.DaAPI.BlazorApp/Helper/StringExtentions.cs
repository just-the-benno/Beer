using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
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

        public static IPv6Address AsIPv6Address(this String input)
        {
            try
            {
                return IPv6Address.FromString(input);
            }
            catch (Exception)
            {
                return IPv6Address.Empty;
            }
        }
    }
}

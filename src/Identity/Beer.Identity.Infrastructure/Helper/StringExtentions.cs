using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.Identity.Infrastructure.Helper
{
    public static class StringExtentions
    {
        public static String RemoveTrailingSlashIfNeeded(this String input)
        {
            if (input.EndsWith('/') == true)
            {
                return input.Remove(input.Length - 1);
            }
            else
            {
                return input;
            }
        }
    }
}

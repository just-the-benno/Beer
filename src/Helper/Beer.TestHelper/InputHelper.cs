using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.TestHelper
{
    public static class InputHelper
    {
        public static IEnumerable<String> GetEmptyStringInputs() =>
            new List<string> { String.Empty, null, " ", "\n" };
    }
}

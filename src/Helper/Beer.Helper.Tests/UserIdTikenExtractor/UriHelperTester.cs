using Beer.Helper.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Beer.Helper.Tests.UserIdTikenExtractor
{
    public class UriHelperTester
    {
        [Theory]
        [InlineData("https://localhost:5001/", "https://localhost:5001")]
        [InlineData("https://localhost:5001", "https://localhost:5001")]
        public void Blub(String input, String expectedOutput)
        {
            Uri uri = new Uri(input);

            String actual = UriHelper.RemoveTrailingSlash(uri);

            Assert.Equal(expectedOutput, actual);
        }

    }
}

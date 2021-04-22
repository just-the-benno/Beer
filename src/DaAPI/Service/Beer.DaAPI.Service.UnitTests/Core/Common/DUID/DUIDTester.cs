using Beer.DaAPI.Core.Common;
using Beer.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Beer.DaAPI.Service.UnitTests.Core.Common.DUID
{
    public class DUIDTester
    {
        [Fact]
        public void Equals_NotNull()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            UUIDDUID duid1 = new UUIDDUID(id);
            UUIDDUID duid2 = new UUIDDUID(id);

            Assert.True(duid1.Equals(duid2));
        }

        [Fact]
        public void Equals_Null()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            UUIDDUID duid1 = new UUIDDUID(id);
            UUIDDUID duid2 = null;

            Assert.False(duid1.Equals(duid2));
        }
    }
}

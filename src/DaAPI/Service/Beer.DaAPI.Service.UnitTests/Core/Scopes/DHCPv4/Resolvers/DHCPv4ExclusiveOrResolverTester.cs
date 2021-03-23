using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Services;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv4.Resolvers
{
    public class DHCPv4ExclusiveOrResolverTester : DHCPv4ScopeResolverWithLogicalOperationTesterBase
    {
        [Fact]
        public void DHCPv4ExclusiveOrResolver_GetDescription()
        {
            DHCPv4ExclusiveOrResolver resolver = new DHCPv4ExclusiveOrResolver();

            TestDescription(resolver, "DHCPv4ExclusiveOrResolver");
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]

        public void DHCPv4ExclusiveOrResolver_PacketMeetsCondition(Boolean a, Boolean b, Boolean expectedResult)
        {
            Random random = new Random();

            CheckMeetsConditions(
                () => new DHCPv4ExclusiveOrResolver(),
                (a,b,expectedResult),
                random
                );
        }
    }
}

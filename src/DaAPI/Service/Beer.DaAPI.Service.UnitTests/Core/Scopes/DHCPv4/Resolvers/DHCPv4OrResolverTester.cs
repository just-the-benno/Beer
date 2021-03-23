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
    public class DHCPv4OrResolverTester : DHCPv4ScopeResolverWithLogicalOperationTesterBase
    {
        [Fact]
        public void DHCPv4OrResolver_GetDescription()
        {
            DHCPv4OrResolver resolver = new DHCPv4OrResolver();

            TestDescription(resolver, "DHCPv4OrResolver");
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public void DHCPv4OrResolver_PacketMeetsCondition(Boolean a, Boolean b, Boolean expectedResult)
        {
            Random random = new Random();

            CheckMeetsConditions(
                () => new DHCPv4OrResolver(),
                (a,b,expectedResult),
                random
                );
        }
    }
}

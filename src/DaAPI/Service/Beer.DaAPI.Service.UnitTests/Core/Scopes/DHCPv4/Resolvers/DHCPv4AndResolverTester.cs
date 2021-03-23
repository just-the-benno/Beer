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
    public class DHCPv4AndResolverTester : DHCPv4ScopeResolverWithLogicalOperationTesterBase
    {
        [Fact]
        public void DHCPv4AndResolver_GetDescription()
        {
            DHCPv4AndResolver resolver = new DHCPv4AndResolver();
            TestDescription(resolver, "DHCPv4AndResolver");
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public void DHCPv4AndResolver_PacketMeetsCondition(Boolean a, Boolean b, Boolean expectedResult)
        {
            Random random = new Random();

            CheckMeetsConditions(
                () => new DHCPv4AndResolver(),
                (a,b,expectedResult),
                random
                );
        }
    }
}

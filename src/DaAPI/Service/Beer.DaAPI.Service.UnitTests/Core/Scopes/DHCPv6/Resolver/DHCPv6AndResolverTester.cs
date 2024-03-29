﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv6.Resolvers;
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

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6AndResolverTester : DHCPv6ScopeResolverWithLogicalOperationTesterBase
    {
        [Fact]
        public void GetDescription()
        {
            DHCPv6AndResolver resolver = new DHCPv6AndResolver();
            TestDescription(resolver, "DHCPv6AndResolver");
        }

        [Fact]
        public void HasUniqueIdentifier()
        {
            DHCPv6AndResolver resolver = new DHCPv6AndResolver();
            Assert.False(resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            DHCPv6AndResolver resolver = new DHCPv6AndResolver();
            Assert.ThrowsAny<Exception>(() => resolver.GetUniqueIdentifier(null));
        }

        [Fact]
        public void PacketMeetsCondition()
        {

            List<Tuple<Boolean, Boolean, Boolean>> inputs = new List<Tuple<bool, bool, bool>>
            {
                new Tuple<bool, bool, bool>(false,false,false),
                new Tuple<bool, bool, bool>(false,true,false),
                new Tuple<bool, bool, bool>(true,false,false),
                new Tuple<bool, bool, bool>(true,true,true),
            };

            Random random = new Random();

            CheckMeetsConditions(
                () => new DHCPv6AndResolver(),
                inputs
                );
        }

        [Fact]
        public void GetValues()
        {
            DHCPv6AndResolver resolver = new DHCPv6AndResolver();
            Assert.Throws<NotImplementedException>(() => resolver.GetValues());
        }
    }
}

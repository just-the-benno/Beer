using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv4.Resolvers
{
    public abstract class DHCPv4ScopeResolverWithLogicalOperationTesterBase
    {
        protected void TestDescription(IScopeResolver<DHCPv4Packet, IPv4Address> resolver, String expectedName)
        {
            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(expectedName, description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Single(description.Properties);

            ScopeResolverPropertyDescription propertyDescription = description.Properties.First();
            Assert.Equal("InnerResolvers", propertyDescription.PropertyName);
            Assert.Equal(ScopeResolverPropertyValueTypes.Resolvers, propertyDescription.PropertyValueType);
        }

        protected void CheckMeetsConditions(
            Func<DHCPv4ScopeResolverContainingOtherResolvers> resolverCreater,
            (Boolean, Boolean, Boolean) input, Random random)
        {
            DHCPv4ScopeResolverContainingOtherResolvers resolver = resolverCreater();

            DHCPv4Packet packet = new DHCPv4Packet(
                new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                random.NextBytes(6),
                (UInt32)random.Next(),
                IPv4Address.Empty,
                IPv4Address.Empty,
                IPv4Address.Empty
                );

            var firstInnerMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            firstInnerMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(input.Item1);

            var secondInnerMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            secondInnerMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(input.Item2);

            resolver.AddResolver(firstInnerMock.Object);
            resolver.AddResolver(secondInnerMock.Object);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(input.Item3, result);
        }
    }
}

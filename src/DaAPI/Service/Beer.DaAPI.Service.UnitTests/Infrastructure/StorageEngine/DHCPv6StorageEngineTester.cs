using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Beer.DaAPI.UnitTests.Infrastructure.StorageEngine
{
    public class DHCPv6StorageEngineTester
    {
        [Fact]
        public async Task GetDHCPv6Listener()
        {
            List<DHCPv6Listener> listeners = new List<DHCPv6Listener>();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetDHCPv6Listener()).ReturnsAsync(listeners).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6ReadStore))).Returns(readStoreMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(factoryMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6EventStore))).Returns(Mock.Of<IDHCPv6EventStore>());

            DHCPv6StorageEngine storageEngine = new DHCPv6StorageEngine(serviceProviderMock.Object);

            var actual = await storageEngine.GetDHCPv6Listener();
            Assert.Equal(listeners, actual);

            readStoreMock.Verify();
        }

        [Theory]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public async Task Save(Boolean eventStoreResult, Boolean projectionResult, Boolean expectedResult)
        {
            List<DummyEvent> events = new List<DummyEvent>();
            for (int i = 0; i < 10; i++)
            {
                events.Add(new DummyEvent());
            }

            var mockedAggregateRoot = new MockedAggregateRoot(events);

            Mock<IDHCPv6EventStore> eventStoreMock = new Mock<IDHCPv6EventStore>(MockBehavior.Strict);
            eventStoreMock.Setup(x => x.Save(mockedAggregateRoot, 20)).ReturnsAsync(eventStoreResult).Verifiable();

            var comparer = new DummyEventEqualityCompare();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            if (eventStoreResult == true)
            {
                readStoreMock.Setup(x => x.Project(It.Is<IEnumerable<DomainEvent>>(y =>
                    comparer.Equals(events, y.Cast<DummyEvent>())
                ))).ReturnsAsync(projectionResult).Verifiable();
            }

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6ReadStore))).Returns(readStoreMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(factoryMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6EventStore))).Returns(eventStoreMock.Object);

            DHCPv6StorageEngine storageEngine = new DHCPv6StorageEngine(serviceProviderMock.Object);

            Boolean actual = await storageEngine.Save(mockedAggregateRoot);
            Assert.Equal(expectedResult, actual);

            eventStoreMock.Verify();
            if (eventStoreResult == false)
            {
                readStoreMock.Verify();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteAggregateRoot(Boolean storeResult)
        {
            MockedAggregateRoot mockedAggregateRoot = new MockedAggregateRoot(null);

            Mock<IDHCPv6EventStore> eventStoreMock = new Mock<IDHCPv6EventStore>(MockBehavior.Strict);
            eventStoreMock.Setup(x => x.DeleteAggregateRoot<MockedAggregateRoot>(mockedAggregateRoot.Id)).ReturnsAsync(storeResult).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6ReadStore))).Returns(Mock.Of<IDHCPv6ReadStore>());
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6EventStore))).Returns(eventStoreMock.Object);

            serviceProviderMock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(factoryMock.Object);

            DHCPv6StorageEngine storageEngine = new DHCPv6StorageEngine(serviceProviderMock.Object);

            var actual = await storageEngine.DeleteAggregateRoot<MockedAggregateRoot>(mockedAggregateRoot.Id);
            Assert.Equal(actual, storeResult);

            eventStoreMock.Verify();
        }
    }
}

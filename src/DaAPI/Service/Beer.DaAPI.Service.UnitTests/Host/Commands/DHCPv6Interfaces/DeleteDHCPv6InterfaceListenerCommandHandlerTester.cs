﻿using Castle.Core.Logging;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv6Interfaces;
using Beer.DaAPI.Infrastructure.InterfaceEngines;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Beer.DaAPI.Core.Listeners.DHCPListenerEvents;
using Beer.DaAPI.Service.TestHelper;

namespace Beer.DaAPI.UnitTests.Host.Commands.DHCPv6Interfaces
{
    public class DeleteDHCPv6InterfaceListenerCommandHandlerTester
    {
        [Fact]
        public async Task Handle()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            DHCPv6Listener listener = new DHCPv6Listener();
            listener.Load(new DomainEvent[] {
                new DHCPv6ListenerCreatedEvent
                {
                    Id = id,
                    Address = random.GetIPv6Address().ToString(),
                }
            });

            var command = new DeleteDHCPv6InterfaceListenerCommand(id);

            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.CheckIfAggrerootExists<DHCPv6Listener>(id)).ReturnsAsync(true).Verifiable();
            storageMock.Setup(x => x.GetAggregateRoot<DHCPv6Listener>(id)).ReturnsAsync(listener).Verifiable();
            storageMock.Setup(x => x.Save(listener)).ReturnsAsync(true).Verifiable();


            Mock<IDHCPv6InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv6InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.CloseListener(listener)).Returns(true).Verifiable();

            var handler = new DeleteDHCPv6InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, storageMock.Object, Mock.Of<ILogger<DeleteDHCPv6InterfaceListenerCommandHandler>>());

            Boolean actual = await handler.Handle(command, CancellationToken.None);

            Assert.True(actual);

            storageMock.Verify();
            interfaceEngineMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_NotFound()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            DHCPv6Listener listener = new DHCPv6Listener();
            listener.Load(new DomainEvent[] {
                new DHCPv6ListenerCreatedEvent
                {
                    Id = id,
                    Address = random.GetIPv6Address().ToString(),
                }
            });

            var command = new DeleteDHCPv6InterfaceListenerCommand(id);

            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.CheckIfAggrerootExists<DHCPv6Listener>(id)).ReturnsAsync(false).Verifiable();

            var handler = new DeleteDHCPv6InterfaceListenerCommandHandler(
                Mock.Of<IDHCPv6InterfaceEngine>(MockBehavior.Strict), storageMock.Object, Mock.Of<ILogger<DeleteDHCPv6InterfaceListenerCommandHandler>>());

            Boolean actual = await handler.Handle(command, CancellationToken.None);

            Assert.False(actual);

            storageMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_NotSaved()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            DHCPv6Listener listener = new DHCPv6Listener();
            listener.Load(new DomainEvent[] {
                new DHCPv6ListenerCreatedEvent
                {
                    Id = id,
                    Address = random.GetIPv6Address().ToString(),
                }
            });

            var command = new DeleteDHCPv6InterfaceListenerCommand(id);

            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.CheckIfAggrerootExists<DHCPv6Listener>(id)).ReturnsAsync(true).Verifiable();
            storageMock.Setup(x => x.GetAggregateRoot<DHCPv6Listener>(id)).ReturnsAsync(listener).Verifiable();
            storageMock.Setup(x => x.Save(listener)).ReturnsAsync(false).Verifiable();

            var handler = new DeleteDHCPv6InterfaceListenerCommandHandler(
                Mock.Of< IDHCPv6InterfaceEngine >(MockBehavior.Strict), storageMock.Object, Mock.Of<ILogger<DeleteDHCPv6InterfaceListenerCommandHandler>>());

            Boolean actual = await handler.Handle(command, CancellationToken.None);

            Assert.False(actual);

            storageMock.Verify();
        }
    }
}

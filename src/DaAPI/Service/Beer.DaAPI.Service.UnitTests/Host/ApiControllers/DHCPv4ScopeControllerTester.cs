﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Service.API.ApiControllers;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv4Scopes;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.UnitTests.Host.ApiControllers
{
    public class DHCPv4ScopeControllerTester
    {

        private void GenerateScopeTree(
   Double randomValue, Random random, List<Guid> parents,
   ICollection<DomainEvent> events
   )
        {
            if (randomValue > 0)
            {
                return;
            }

            Int32 scopeAmount = random.Next(3, 10);
            Guid directParentId = parents.Last();
            for (int i = 0; i < scopeAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();
                IPv4Address start = random.GetIPv4Address();
                IPv4Address end = start + 100;
                var excluded = new[] { start + 1, start + 3 };

                DHCPv4ScopeProperties properties = null;
                if (random.NextBoolean() == true)
                {
                    DHCPv4AddressListScopeProperty gwAddress = new(
                        DHCPv4OptionTypes.Router, new[] { start + 0, start + 30 });
                    properties = new DHCPv4ScopeProperties(gwAddress);
                }
                else
                {
                    properties = new();
                }

                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                {
                    Id = scopeId,
                    Name = random.GetAlphanumericString(),
                    ParentId = directParentId,
                    AddressProperties = new DHCPv4ScopeAddressProperties(start, end, excluded,
                    maskLength: random.NextBoolean() == true ? (Byte)random.Next(0, 32) : null),
                    ScopeProperties = properties
                }));

                List<Guid> newParentList = new List<Guid>(parents)
                {
                    scopeId
                };

                GenerateScopeTree(
                    randomValue + random.NextDouble(), random,
                    newParentList, events);
            }
        }

        [Fact]
        public void GetScopesAsList()
        {
            Random random = new Random();

            var events = new List<DomainEvent>();
            Int32 rootScopeAmount = random.Next(10, 30);
            List<Guid> rootScopeIds = new List<Guid>(rootScopeAmount);
            for (int i = 0; i < rootScopeAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();
                IPv4Address start = random.GetIPv4Address();
                IPv4Address end = start + 100;
                var excluded = new[] { start + 1, start + 3 };

                DHCPv4ScopeProperties properties = null;
                if (random.NextBoolean() == true)
                {
                    DHCPv4AddressListScopeProperty gwAddress = new(
                        DHCPv4OptionTypes.Router, new[] { start + 0, start + 30 });
                    properties = new DHCPv4ScopeProperties(gwAddress);
                }
                else
                {
                    properties = new();
                }

                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                {
                    Id = scopeId,
                    Name = random.GetAlphanumericString(),
                    AddressProperties = new DHCPv4ScopeAddressProperties(start, end, excluded,
                    maskLength: random.NextBoolean() == true ? (Byte)random.Next(0, 32) : null),
                    ScopeProperties = properties
                }));

                rootScopeIds.Add(scopeId);

                GenerateScopeTree(
                    random.NextDouble(), random,
                    new List<Guid> { scopeId }, events);
            }

            var scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            scopeResolverMock.Setup(x => x.InitializeResolver(It.IsAny<CreateScopeResolverInformation>())).Returns(Mock.Of<IScopeResolver<DHCPv4Packet, IPv4Address>>());

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(events);

            var controller = new DHCPv4ScopeController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict),
                rootScope);
            var actionResult = controller.GetScopesAsList();
            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv4ScopeItem>>(true);

            Assert.Equal(events.Count, result.Count());
            for (int i = 0; i < events.Count; i++)
            {
                var scope = result.ElementAt(i);
                var @event = (DHCPv4ScopeAddedEvent)events[i];
                Assert.Equal(@event.Instructions.Name, scope.Name);
                Assert.Equal(@event.Instructions.Id, scope.Id);
                Assert.Equal(@event.Instructions.AddressProperties.Start.ToString(), scope.StartAddress);
                Assert.Equal(@event.Instructions.AddressProperties.End.ToString(), scope.EndAddress);
                Assert.Equal(@event.Instructions.AddressProperties.ExcludedAddresses.Select(x => x.ToString()).ToArray(), scope.ExcludedAddresses);
                Assert.Equal((@event.Instructions.ScopeProperties?.Properties ?? Array.Empty<DHCPv4ScopeProperty>()).Where(x => x.OptionIdentifier == (Byte)DHCPv4OptionTypes.Router).Cast<DHCPv4AddressListScopeProperty>().Select(x => x.Addresses.First().ToString()).FirstOrDefault(), scope.FirstGatewayAddress);
            }
        }

        private void CheckTreeItem(DHCPv4Scope item, DHCPv4ScopeTreeViewItem viewItem)
        {
            Assert.Equal(item.Name, viewItem.Name);
            Assert.Equal(item.Id, viewItem.Id);
            Assert.Equal(item.AddressRelatedProperties.Start.ToString(), viewItem.StartAddress);
            Assert.Equal(item.AddressRelatedProperties.End.ToString(), viewItem.EndAddress);
            Assert.Equal(item.AddressRelatedProperties.ExcludedAddresses.Select(x => x.ToString()).ToArray(), viewItem.ExcludedAddresses);
            Assert.Equal((item.Properties?.Properties ?? Array.Empty<DHCPv4ScopeProperty>()).Where(x => x.OptionIdentifier == (Byte)DHCPv4OptionTypes.Router).Cast<DHCPv4AddressListScopeProperty>().Select(x => x.Addresses.First().ToString()).FirstOrDefault(), viewItem.FirstGatewayAddress);

            if (item.GetChildScopes().Any() == true)
            {
                Assert.Equal(item.GetChildScopes().Count(), viewItem.ChildScopes.Count());
                Int32 index = 0;
                foreach (var childScope in item.GetChildScopes())
                {
                    var childViewItem = viewItem.ChildScopes.ElementAt(index);
                    CheckTreeItem(childScope, childViewItem);

                    index++;
                }
            }
            else
            {
                Assert.Empty(viewItem.ChildScopes);
            }
        }

        [Fact]
        public void GetScopesAsTreeView()
        {
            Random random = new Random();

            var events = new List<DomainEvent>();
            Int32 rootScopeAmount = random.Next(10, 30);
            List<Guid> rootScopeIds = new List<Guid>(rootScopeAmount);
            for (int i = 0; i < rootScopeAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();
                IPv4Address start = random.GetIPv4Address();
                IPv4Address end = start + 100;
                var excluded = new[] { start + 1, start + 3 };

                DHCPv4ScopeProperties properties = null;
                if (random.NextBoolean() == true)
                {
                    DHCPv4AddressListScopeProperty gwAddress = new(
                        DHCPv4OptionTypes.Router, new[] { start + 0, start + 30 });
                    properties = new DHCPv4ScopeProperties(gwAddress);
                }
                else
                {
                    properties = new();
                }

                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                {
                    Id = scopeId,
                    Name = random.GetAlphanumericString(),
                    AddressProperties = new DHCPv4ScopeAddressProperties(start, end, excluded,
                    maskLength: random.NextBoolean() == true ? (Byte)random.Next(0, 32) : null),
                    ScopeProperties = properties
                }));

                rootScopeIds.Add(scopeId);

                GenerateScopeTree(
                    random.NextDouble(), random,
                    new List<Guid> { scopeId }, events);
            }

            var scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            scopeResolverMock.Setup(x => x.InitializeResolver(It.IsAny<CreateScopeResolverInformation>())).Returns(Mock.Of<IScopeResolver<DHCPv4Packet, IPv4Address>>());

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(events);

            var controller = new DHCPv4ScopeController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict),
                rootScope);
            var actionResult = controller.GetScopesAsTreeView();
            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv4ScopeTreeViewItem>>(true);

            Assert.Equal(rootScopeAmount, result.Count());
            Int32 index = 0;
            foreach (var item in rootScope.GetRootScopes())
            {
                var scope = result.ElementAt(index);

                CheckTreeItem(item, scope);

                index++;
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateScope(Boolean mediatorResultShouldBeSuccessful)
        {
            Random random = new Random();
            Guid? id = mediatorResultShouldBeSuccessful == true ? random.NextGuid() : new Guid?();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();
            Guid? parentId = random.NextBoolean() == true ? random.NextGuid() : new Guid?();

            var addressProperties = new DHCPv4ScopeAddressPropertyReqest();
            var resolverInfo = new CreateScopeResolverRequest
            {
                Typename = random.GetAlphanumericString(),
                PropertiesAndValues = new Dictionary<String, String>()
            };

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict), factoryMock.Object);

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<CreateDHCPv4ScopeCommand>(y =>
            y.Name == name && y.Description == description && y.ParentId == parentId &&
            y.AddressProperties == addressProperties && y.Resolver == resolverInfo
            ), It.IsAny<CancellationToken>())).ReturnsAsync(id).Verifiable();

            var request = new CreateOrUpdateDHCPv4ScopeRequest
            {
                Name = name,
                Description = description,
                AddressProperties = addressProperties,
                ParentId = parentId,
                Resolver = resolverInfo,
            };

            var controller = new DHCPv4ScopeController(mediatorMock.Object,
                Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict),
                rootScope);

            var actionResult = await controller.CreateScope(request);
            if (mediatorResultShouldBeSuccessful == true)
            {
                Guid result = actionResult.EnsureOkObjectResult<Guid>(true);
                Assert.Equal(id, result);
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to create scope");
            }

            mediatorMock.Verify();
        }

        private async Task CheckModelState(Func<DHCPv4ScopeController, Task<IActionResult>> controllerExecuter)
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            Random random = new Random();
            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict), factoryMock.Object);

            var controller = new DHCPv4ScopeController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict),
                rootScope
                );

            String modelErrorKey = "a" + random.GetAlphanumericString();
            String modelErrorMessage = random.GetAlphanumericString();
            controller.ModelState.AddModelError(modelErrorKey, modelErrorMessage);

            var result = await controllerExecuter(controller);

            result.EnsureBadRequestObjectResultForError(modelErrorKey, modelErrorMessage);
        }

        [Fact]
        public async Task CreateScope_ModelStateError()
        {
            await CheckModelState((controller) => controller.CreateScope(null));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Delete(Boolean mediatorResultShouldBeSuccessful)
        {
            Random random = new Random();

            Guid id = random.NextGuid();
            Boolean includeChildren = random.NextBoolean();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict), factoryMock.Object);

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<DeleteDHCPv4ScopeCommand>(y =>
            y.ScopeId == id && y.IncludeChildren == includeChildren
            ), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResultShouldBeSuccessful).Verifiable();

            var controller = new DHCPv4ScopeController(mediatorMock.Object,
                Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict),
                rootScope);

            var actionResult = await controller.DeleteScope(id, includeChildren);
            if (mediatorResultShouldBeSuccessful == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to execute service operation");
            }

            mediatorMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateScope(Boolean mediatorResultShouldBeSuccessful)
        {
            Random random = new Random();

            Guid id = random.NextGuid();
            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();
            Guid? parentId = random.NextBoolean() == true ? random.NextGuid() : new Guid?();

            var addressProperties = new DHCPv4ScopeAddressPropertyReqest();
            var resolverInfo = new CreateScopeResolverRequest
            {
                Typename = random.GetAlphanumericString(),
                PropertiesAndValues = new Dictionary<String, String>()
            };

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict), factoryMock.Object);

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<UpdateDHCPv4ScopeCommand>(y =>
            y.ScopeId == id &&
            y.Name == name && y.Description == description && y.ParentId == parentId &&
            y.AddressProperties == addressProperties && y.Resolver == resolverInfo
            ), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResultShouldBeSuccessful).Verifiable();

            var request = new CreateOrUpdateDHCPv4ScopeRequest
            {
                Name = name,
                Description = description,
                AddressProperties = addressProperties,
                ParentId = parentId,
                Resolver = resolverInfo,
            };

            var controller = new DHCPv4ScopeController(mediatorMock.Object,
                Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict),
                rootScope);

            var actionResult = await controller.UpdateScope(request, id);
            if (mediatorResultShouldBeSuccessful == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to execute service operation");
            }

            mediatorMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateScopeParent(Boolean mediatorResultShouldBeSuccessful)
        {
            Random random = new Random();

            Guid id = random.NextGuid();
            Guid? parentId = random.NextBoolean() == true ? random.NextGuid() : new Guid?();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict), factoryMock.Object);

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<UpdateDHCPv4ScopeParentCommand>(y =>
            y.ScopeId == id && y.ParentScopeId == parentId
            ), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResultShouldBeSuccessful).Verifiable();

            var controller = new DHCPv4ScopeController(mediatorMock.Object,
                Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict),
                rootScope);

            var actionResult = await controller.UpdateScopeParent(id, parentId);
            if (mediatorResultShouldBeSuccessful == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to execute service operation");
            }

            mediatorMock.Verify();
        }
    }
}

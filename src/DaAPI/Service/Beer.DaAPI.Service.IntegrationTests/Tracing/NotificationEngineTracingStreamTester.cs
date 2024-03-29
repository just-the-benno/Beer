﻿using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Notifications.Actors;
using Beer.DaAPI.Core.Notifications.Conditions;
using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Notifications;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Core.Tracing;
using Beer.DaAPI.Infrastructure.NotificationEngine;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.Services;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Infrastructure.StorageEngine;
using Beer.DaAPI.Infrastructure.Tracing;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Beer.DaAPI.Service.Infrastructure.StorageEngine;

namespace Beer.DaAPI.Service.IntegrationTests.Tracing
{
    public class NotificationEngineTracingStreamTester
    {
        protected DHCPv6RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var rootScope = new DHCPv6RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);
            return rootScope;
        }

        [Fact]
        public async Task TracingStream_EdgePrefixBinding()
        {
            Random random = new Random();
            //String dbName = random.GetAlphanumericString();
            String dbName = "DaAPI";

            String dbConnectionString = $"{ConfigurationUtility.GetConnectionString("DaAPIDatabaseTestConnection")};Database={dbName};";

            var context = DatabaseTestingUtility.GetTestDatabaseContext(dbName);
            var wrtieStore = new DapperBasedTracingStore(dbConnectionString, Mock.Of<ILogger<DapperBasedTracingStore>>());
            try
            {
                context.Database.Migrate();

                Mock<IDHCPv6StorageEngine> storeEngineMock = new Mock<IDHCPv6StorageEngine>();
                storeEngineMock.Setup(x => x.Save(It.IsAny<NotificationPipeline>())).ReturnsAsync(true);

                var tracingManager = new TracingManager(Mock.Of<IServiceBus>(), wrtieStore);

                NotificationEngine engine = new NotificationEngine(
                    storeEngineMock.Object, tracingManager);

                Guid scopeId = random.NextGuid();
                PrefixBinding oldBinding = new PrefixBinding(IPv6Address.FromString("2acd:2222:1111::"), new IPv6SubnetMaskIdentifier(48), IPv6Address.FromString("2222::1111"));
                PrefixBinding newBinding = new PrefixBinding(IPv6Address.FromString("2acd:2222:3333::"), new IPv6SubnetMaskIdentifier(48), IPv6Address.FromString("2222::3333"));

                var rootScope = GetRootScope();
                rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

                var serializerMock = new Mock<ISerializer>();
                serializerMock.Setup(x => x.Deserialze<Boolean>("conding-IncludesChildren")).Returns(true);
                serializerMock.Setup(x => x.Deserialze<IEnumerable<Guid>>("conding-ScopeIds")).Returns(new[] { scopeId });

                var condition = new DHCPv6ScopeIdNotificationCondition(rootScope, serializerMock.Object, Mock.Of<ILogger<DHCPv6ScopeIdNotificationCondition>>());
                condition.ApplyValues(new Dictionary<String, String> {
                { "IncludesChildren", "conding-IncludesChildren" },
                { "ScopeIds", "conding-ScopeIds" },
            });

                var handlerMock = new Mock<HttpMessageHandler>();
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"{ ""result"": ""something"", ""error"": null}"),
                };

                handlerMock
                   .Protected()
                   .Setup<Task<HttpResponseMessage>>(
                      "SendAsync",
                      ItExpr.IsAny<HttpRequestMessage>(),
                      ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(response);

                HttpBasedNxOsDeviceConfigurationService deviceConfigService = new HttpBasedNxOsDeviceConfigurationService(new HttpClient(handlerMock.Object), Mock.Of<ILogger<HttpBasedNxOsDeviceConfigurationService>>());

                var actor = new NxOsStaticRouteUpdaterNotificationActor(deviceConfigService, Mock.Of<ILogger<NxOsStaticRouteUpdaterNotificationActor>>());
                actor.ApplyValues(new Dictionary<String, String> {
                { "Url", "https://10.10.10.10" },
                { "Password", "P@assw0rd123!@$" },
                { "Username", "testUser" },
            });

                Mock<INotificationConditionFactory> conditionFactoryMock = new Mock<INotificationConditionFactory>();
                conditionFactoryMock.Setup(x => x.Initilize(It.IsAny<NotificationConditionCreateModel>())).Returns(condition);

                Mock<INotificationActorFactory> actorFactoryMock = new Mock<INotificationActorFactory>();
                actorFactoryMock.Setup(x => x.Initilize(It.IsAny<NotificationActorCreateModel>())).Returns(actor);

                var pipeline = NotificationPipeline.Create(
                    NotificationPipelineName.FromString("test pipeline"),
                    NotificationPipelineDescription.FromString("a description for test purpose"),
                    nameof(PrefixEdgeRouterBindingUpdatedTrigger),
                    condition, actor, Mock.Of<ILogger<NotificationPipeline>>(), conditionFactoryMock.Object, actorFactoryMock.Object
                    );

                PrefixEdgeRouterBindingUpdatedTrigger trigger =
                    PrefixEdgeRouterBindingUpdatedTrigger.WithOldAndNewBinding(scopeId, oldBinding, newBinding);

                storeEngineMock.Setup(x => x.GetAllNotificationPipeleines()).ReturnsAsync(new[] { pipeline });

                await engine.Initialize();

                for (int i = 0; i < 1000; i++)
                {
                    await engine.HandleTrigger(trigger);
                }
            }
            finally
            {
                await DatabaseTestingUtility.DeleteDatabase(dbName);
            }
        }

    }
}

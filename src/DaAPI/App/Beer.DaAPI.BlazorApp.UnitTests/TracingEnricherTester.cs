using Beer.DaAPI.BlazorApp.Services.TracingEnricher;
using Beer.DaAPI.Shared.Helper;
using Microsoft.Extensions.Localization;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.BlazorApp.UnitTests
{
    public class TracingEnricherTester
    {
        [Fact]
        public void GetRecordDetails()
        {
            Mock<ITracingEntityCache> entryCacheMock = new Mock<ITracingEntityCache>();
            entryCacheMock.Setup(x => x.GetDHCPv6ScopeLink(It.IsAny<String>())).Returns(Guid.NewGuid().ToString());

            NotificationSystemNewTriggerTracingEnricher enricher = new NotificationSystemNewTriggerTracingEnricher(
                entryCacheMock.Object, Mock.Of<IStringLocalizer<NotificationSystemNewTriggerTracingEnricher>>());

            String fileContent = File.ReadAllText("Files/TracingRecod.json");

            var records = JsonSerializer.Deserialize<IEnumerable<TracingStreamRecord>>(fileContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            foreach (var item in records)
            {
                enricher.GetRecordDetails(item);
            }
        }

        [Fact]
        public void GetProcedureIdentifierFirstItemPreview()
        {
            var localizerMock = new Mock<IStringLocalizer<NotificationSystemNewTriggerTracingEnricher>>();
            localizerMock.Setup(x => x["PrefixEdgeRouterBindingUpdatedTriggerFirstItem_OnlyNewBinding"]).Returns(new LocalizedString("something", "{0} {1} {2}"));

            NotificationSystemNewTriggerTracingEnricher enricher = new NotificationSystemNewTriggerTracingEnricher(
                Mock.Of<ITracingEntityCache>(), localizerMock.Object);


            String fileContent = File.ReadAllText("Files/TracingStreamOverview.json");

            var records = JsonSerializer.Deserialize<FilteredResult<TracingStreamOverview>>(fileContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            String content = enricher.GetProcedureIdentifierFirstItemPreview(records.Result.First());

            Assert.Equal("2a0c:cac2:202:b920:: 60 2a0c:cac6:2000:202:abef:853f:2f82:a604", content);
        }
    }
}

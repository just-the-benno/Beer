using Beer.DaAPI.Shared.Responses;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static Beer.DaAPI.BlazorApp.Services.TracingEnricher.Models.NotificationSystemRecords.V1;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.BlazorApp.Services.TracingEnricher
{
    public class NotificationSystemNewTriggerTracingEnricher : ProcedureTracingEnricher
    {
        private readonly ITracingEntityCache _entityCache;
        private readonly IStringLocalizer<NotificationSystemNewTriggerTracingEnricher> _localizer;

        public NotificationSystemNewTriggerTracingEnricher(
            ITracingEntityCache cache,
            IStringLocalizer<NotificationSystemNewTriggerTracingEnricher> localizer) : base(1)
        {
            this._entityCache = cache;
            this._localizer = localizer;
        }

        public override string GetProcedureIdentifierFirstItemPreview(TracingStreamOverview stream)
        {
            if (stream.FirstEntryData == null || stream.FirstEntryData.ContainsKey("name") == false)
            {

                return String.Empty;
            }

            try
            {
                var name = stream.FirstEntryData["name"];
                var dictAsJson = NormalizeJsonOutput(stream.FirstEntryData);

                switch (name)
                {
                    case "PrefixEdgeRouterBindingUpdatedTrigger":
                        var trigger = JsonSerializer.Deserialize<PrefixEdgeRouterBindingUpdatedTrigger>(dictAsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        Console.WriteLine(trigger.Name);
                        if (trigger.OldBinding != null && trigger.NewBinding != null)
                        {
                            return String.Format(_localizer["PrefixEdgeRouterBindingUpdatedTriggerFirstItem_BothBindings"],
                           trigger.OldBinding.Network, trigger.OldBinding.Mask, trigger.OldBinding.Host, trigger.NewBinding.Network, trigger.NewBinding.Mask, trigger.NewBinding.Host);
                        }
                        else if (trigger.OldBinding != null)
                        {
                            return String.Format(_localizer["PrefixEdgeRouterBindingUpdatedTriggerFirstItem_OnlyOldBinding"],
                           trigger.OldBinding.Network, trigger.OldBinding.Mask, trigger.OldBinding.Host);
                        }
                        else
                        {
                            return String.Format(_localizer["PrefixEdgeRouterBindingUpdatedTriggerFirstItem_OnlyNewBinding"],
                                trigger.NewBinding.Network, trigger.NewBinding.Mask, trigger.NewBinding.Host);
                        }

                    default:
                        return name;
                }
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        public override string GetProcedureIdentifierName() => _localizer["ProcedureIdentifier"];

        public override IEnumerable<ProceduceTracingRecordValue> GetRecordDetails(TracingStreamRecord record)
        {
            var rawData = NormalizeJsonOutput(record.AddtionalData);
            Type dataType = null;
            switch (record.Identifier)
            {
                case "10.1.6.1":
                case "10.1.4":
                    dataType = typeof(SimplePipeline);
                    break;
                case "10.1":
                case "10.1.5":
                    dataType = typeof(PrefixEdgeRouterBindingUpdatedTrigger);
                    break;
                case "10.1.6.2.1.1":
                    dataType = typeof(DHCPv6ScopeIdConditionSimpleView);
                    break;
                case "10.1.6.4.1.1":
                    dataType = typeof(NxOsDeviceConnectionRequest);
                    break;
                case "10.1.6.4.1.3":
                    dataType = typeof(NxOsDeviceRemoveOldBindindRequest);
                    break;
                case "10.1.6.4.1.3.1.1":
                case "10.1.6.4.1.6.1.1":
                    dataType = typeof(NxOsAPIBindingUpater);
                    break;
                case "10.1.6.4.1.3.1.2":
                case "10.1.6.4.1.6.1.2":
                    dataType = typeof(NxOsAPIBindingUpaterResult);
                    break;
                case "10.1.6.4.1.3.1.3":
                case "10.1.6.4.1.6.1.3":
                    dataType = typeof(NxOsAPIBindingUpaterNotAuthorizeResult);
                    break;
                case "10.1.6.4.1.3.1.4":
                case "10.1.6.4.1.6.1.4":
                    dataType = typeof(NxOsAPIBindingUpaterErrorResult);
                    break;
                case "10.1.6.4.1.6":
                case "10.1.6.4.1.7":
                case "10.1.6.4.1.8":
                    dataType = typeof(NxOsDeviceNewBindindRequest);
                    break;
                default:
                    break;
            }

            if (dataType == null)
            {
                return Array.Empty<StringBasedProceduceTracingRecordValue>();
            }

            var data = (TracingInfoData)JsonSerializer.Deserialize(rawData, dataType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(data.GetType().Name);
            return data.GetValues(_localizer, _entityCache);
        }

        public override string GetRecordTitle(TracingStreamRecord record) => _localizer[record.Identifier];
    }
}

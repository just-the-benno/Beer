using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Services.TracingEnricher.Models
{
    public static class NotificationSystemRecords
    {
        public static class V1
        {
            public abstract class TracingInfoData
            {
                public abstract IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache);
            }

            public class PrefixEdgeRouterBinding
            {
                public String Host { get; set; }
                public String Network { get; set; }
                public String Mask { get; set; }
            }

            public class PrefixEdgeRouterBindingUpdatedTrigger : TracingInfoData
            {
                public String Name { get; set; }
                public PrefixEdgeRouterBinding NewBinding { get; set; }
                public PrefixEdgeRouterBinding OldBinding { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache)
                {
                    var result = new List<ProceduceTracingRecordValue>
                    {
                        new StringBasedProceduceTracingRecordValue(localzier["PrefixEdgeRouterBindingUpdated_Name"],Name),
                    };

                    if (NewBinding != null)
                    {
                        result.Add(new StringBasedProceduceTracingRecordValue(localzier["PrefixEdgeRouterBindingUpdated_NewBinding"], $"{NewBinding.Network}/{NewBinding.Mask} via {NewBinding.Host}"));
                    }
                    if (OldBinding != null)
                    {
                        result.Add(new StringBasedProceduceTracingRecordValue(localzier["PrefixEdgeRouterBindingUpdated_OldBinding"], $"{OldBinding.Network}/{OldBinding.Mask} via {OldBinding.Host}"));
                    }

                    return result;
                }
            }

            public class Condition
            {
                public String Name { get; set; }
            }

            public class Actor
            {
                public String Name { get; set; }
            }

            public class SimplePipeline : TracingInfoData
            {
                public String Id { get; set; }
                public String Name { get; set; }
                public String Trigger { get; set; }
                public Condition Condition { get; set; }
                public Actor Actor { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache) => new ProceduceTracingRecordValue[]
                {
                    new StringBasedProceduceTracingRecordValue(localzier["SimplePipeline_Name"],Name),
                    new LinkBasedProceduceTracingRecordValue(localzier["SimplePipeline_Id"],cache.GetPipelineName(Id),cache.GetPipelineLink(Id)),
                    new StringBasedProceduceTracingRecordValue(localzier["SimplePipeline_Trigger"],Trigger),
                    new StringBasedProceduceTracingRecordValue(localzier["SimplePipeline_Condition"],Condition.Name),
                    new StringBasedProceduceTracingRecordValue(localzier["SimplePipeline_Actor"],Actor.Name),
                };
            }

            public class DHCPv6ScopeIdConditionSimpleView : TracingInfoData
            {
                public String ScopeId { get; set; }
                public IEnumerable<String> ScopeIds { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache) => new ProceduceTracingRecordValue[]
                    {
                        new LinkBasedProceduceTracingRecordValue(localzier["DHCPv6ScopeIdConditionSimpleView_ScopeId"],cache.GetDHCPv6ScopeName(ScopeId),cache.GetDHCPv6ScopeLink(ScopeId)),
                        new LinkListBasedProceduceTracingRecordValue(localzier["DHCPv6ScopeIdConditionSimpleView_ScopeIds"], ScopeIds.ToDictionary(x => cache.GetDHCPv6ScopeLink(x), x => cache.GetDHCPv6ScopeName(x))),
                    };
            }

            public class NxOsDeviceConnectionRequest : TracingInfoData
            {
                public String Url { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache) => new[]
                {
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsDeviceConnectionRequest_Url"],Url),
                };
            }

            public class NxOsDeviceRemoveOldBindindRequest : TracingInfoData
            {
                public String Url { get; set; }
                public PrefixEdgeRouterBinding OldBinding { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache) => new[]
                 {
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsDeviceRemoveOldBindindRequest_Url"],Url),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsDeviceRemoveOldBindindRequest_OldBinding"],$"{OldBinding.Network}/{OldBinding.Mask} via {OldBinding.Host}"),
                };
            }

            public class NxOsAPIBindingUpater : TracingInfoData
            {
                public String Url { get; set; }
                public String Command { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache) => new[]
            {
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpater_Url"],Url),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpater_Command"],Command),
                };
            }

            public class NxOsAPIBindingUpaterResult : TracingInfoData
            {
                public String Url { get; set; }
                public String Command { get; set; }
                public String StatusCode { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache) => new[]
                {
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterResult_Url"],Url),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterResult_Command"],Command),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterResult_StatusCode"],StatusCode),

                };
            }

            public class NxOsAPIBindingUpaterNotAuthorizeResult : TracingInfoData
            {
                public String Url { get; set; }
                public String Command { get; set; }
                public String Username { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache) => new[]
                {
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterNotAuthorizeResult_Url"],Url),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterNotAuthorizeResult_Command"],Command),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterNotAuthorizeResult_Username"],Username),
                };
            }
            
            public class NxOsAPIBindingUpaterErrorResult : TracingInfoData
            {
                public String Url { get; set; }
                public String Command { get; set; }
                public String StatusCode { get; set; }
                public String ErrMsg { get; set; }
                public String ErrMsgExtented { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache) => new[]
               {
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterErrorResult_Url"],Url),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterErrorResult_Command"],Command),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterErrorResult_StatusCode"],StatusCode),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterErrorResult_ErrMsg"],ErrMsg),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsAPIBindingUpaterErrorResult_ErrMsgExtented"],ErrMsgExtented),
                };

            }

            public class NxOsDeviceNewBindindRequest : TracingInfoData
            {
                public String Url { get; set; }
                public PrefixEdgeRouterBinding NewBinding { get; set; }

                public override IEnumerable<ProceduceTracingRecordValue> GetValues(IStringLocalizer localzier, ITracingEntityCache cache) => new[]
                 {
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsDeviceNewBindindRequest_Url"],Url),
                    new StringBasedProceduceTracingRecordValue(localzier["NxOsDeviceNewBindindRequest_NewBinding"],$"{NewBinding.Network}/{NewBinding.Mask} via {NewBinding.Host}"),
                };
            }
        }
    }
}

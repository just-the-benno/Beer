using Beer.DaAPI.Core.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.Tracing
{
    public class TracingManager : ITracingManager
    {
        public TracingStream NewTrace(int systemIdentifier, int procedureIdentfier, ITracingRecord firstRecord)
        {
            return new TracingStream(systemIdentifier, procedureIdentfier, new TracingRecord($"{systemIdentifier}.{procedureIdentfier}", firstRecord), null);
        }
    }

    public static class TracingManagerConstants
    {
        public static class Modules
        {
            public static Int32 NotificationEngine => 10;
        }

        public static class NotifcationEngineSubModels
        {
            public static Int32 HandleTriggerStarted => 1;
            public static Int32 CheckingTriggers => 2;
            public static Int32 PipelineCanHandleTrigger => 3;
            public static Int32 PipelineCanNotHandleTrigger => 4;
            public static Int32 TriggerHandled => 5;
            public static Int32 ExecutionPipelineStarted => 6;

        }
    }
}

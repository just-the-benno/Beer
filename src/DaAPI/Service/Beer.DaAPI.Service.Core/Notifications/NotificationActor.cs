using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Tracing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Notifications
{
    public abstract class NotificationActor : Value, ITracingRecord
    {
        public static Int32 NxOsStaticRouteUpdaterNotificationTraceIdenifier => 1;
        public static Int32 NxOsStaticRouteCleanerNotificationTraceIdenifier => 2;

        public static NotificationActor Invalid => null;

        internal protected abstract Task<Boolean> Handle(NotifcationTrigger trigger, TracingStream tracingStream);

        public abstract NotificationActorCreateModel ToCreateModel();

        protected String GetQuotedString(String value) => $"\"{value}\"";
        protected String GetValueWithoutQuota(String value) => value.StartsWith('\"') == false ? value : value[1..^1];

        public abstract Boolean ApplyValues(IDictionary<String, String> propertiesAndValues);
        public abstract Int32 GetTracingIdentifier();

        public abstract IDictionary<string, string> GetTracingRecordDetails();
        public bool HasIdentity() => false;
        public Guid? Id => null;

    }
}

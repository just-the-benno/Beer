using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Tracing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Notifications
{
    public abstract class NotificationCondition : Value, ITracingRecord
    {
        public static Int32 NotificationTrueConditionIdentifier = 0;
        public static Int32 DHCPv6ScopeIdConditionTracingIdentifier = 1;

        private class NotificationTrueCondition : NotificationCondition
        {
            public override Task<Boolean> IsValid(NotifcationTrigger trigger, TracingStream tracingStream) => Task.FromResult(true);

            public override NotificationConditionCreateModel ToCreateModel() => new NotificationConditionCreateModel
            {
                Typename = nameof(NotificationTrueCondition),
                PropertiesAndValues = new Dictionary<String, String>(),
            };

            public override bool ApplyValues(IDictionary<string, string> propertiesAndValues) => true;

            public override int GetTracingIdentifier() => NotificationCondition.NotificationTrueConditionIdentifier;
            public override IDictionary<string, string> GetTracingRecordDetails() => new Dictionary<string, string>();
        }

        public static NotificationCondition True => new NotificationTrueCondition();

        public abstract Task<Boolean> IsValid(NotifcationTrigger trigger, TracingStream tracingStream);
        public abstract NotificationConditionCreateModel ToCreateModel();

        public abstract Boolean ApplyValues(IDictionary<String, String> propertiesAndValues);

        public static NotificationCondition Invalid = null;

        public abstract Int32 GetTracingIdentifier();

        public abstract IDictionary<string, string> GetTracingRecordDetails();
        public bool HasIdentity() => false;
        public Guid? Id => null;
    }
}

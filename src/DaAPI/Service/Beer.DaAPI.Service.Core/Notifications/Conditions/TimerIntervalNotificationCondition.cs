using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Core.Tracing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Notifications.Conditions
{
    public class TimerIntervalNotificationCondition : NotificationCondition
    {
        private readonly ILogger<DHCPv6ScopeIdNotificationCondition> _logger;
        private readonly ISerializer _serializer;

        private DateTime _lastTriggered = new DateTime();

        public TimeSpan Interval { get; private set; }

        public TimerIntervalNotificationCondition(
            ISerializer serializer,
            ILogger<DHCPv6ScopeIdNotificationCondition> logger)
        {
            _serializer = serializer;
            this._logger = logger;
        }


        public override async Task<Boolean> IsValid(NotifcationTrigger trigger, TracingStream tracingStream)
        {
            if (trigger is TimeIntervallTrigger == false)
            {
                _logger.LogError("condition {name} has invalid trigger. expected trigger type is {expectedType} actual is {type}",
                    nameof(DHCPv6ScopeIdNotificationCondition), typeof(PrefixEdgeRouterBindingUpdatedTrigger), trigger.GetType());

                return false;
            }

            await tracingStream.Append(1, TracingRecordStatus.Informative, new Dictionary<String, String>
            {
               { "Interval", JsonSerializer.Serialize(Interval) }
            });

            DateTime now = DateTime.Now;
            TimeSpan delta = now - _lastTriggered;
            if (delta > Interval)
            {
                await tracingStream.Append(2, TracingRecordStatus.Informative);
                _logger.LogDebug("delta of {delta} found. Condition is valid", delta);
                _lastTriggered = now;
                return true;
            }
            else
            {
                await tracingStream.Append(3, TracingRecordStatus.Informative);
                _logger.LogDebug("delta is {delta} found. Less then interval condition is false", delta);
                return false;
            }
        }

        public override NotificationConditionCreateModel ToCreateModel() => new NotificationConditionCreateModel
        {
            Typename = nameof(TimerIntervalNotificationCondition),
            PropertiesAndValues = new Dictionary<String, String>
            {
                { nameof(Interval), Interval.Ticks.ToString() },
            }
        };

        public override bool ApplyValues(IDictionary<string, string> propertiesAndValues)
        {
            try
            {
                var intervalInTicks = _serializer.Deserialze<String>(propertiesAndValues[nameof(Interval)]);
                Interval = new TimeSpan(Int64.Parse(intervalInTicks));
                return Interval.TotalSeconds > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override int GetTracingIdentifier() => NotificationCondition.TimeIntervalConditionTracingIdentifier;

        public override IDictionary<string, string> GetTracingRecordDetails() => new Dictionary<String, String>
        {
            { "Name", nameof(TimerIntervalNotificationCondition) },
            { nameof(Interval), _serializer.Seralize(Interval) },
        };
    }
}

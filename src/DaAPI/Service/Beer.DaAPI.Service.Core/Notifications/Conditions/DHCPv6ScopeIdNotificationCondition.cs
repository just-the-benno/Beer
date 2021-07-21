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
    public class DHCPv6ScopeIdNotificationCondition : NotificationCondition
    {
        private readonly DHCPv6RootScope _rootScope;
        private readonly ILogger<DHCPv6ScopeIdNotificationCondition> _logger;
        private readonly ISerializer _serializer;

        public Boolean IncludesChildren { get; private set; }
        public IEnumerable<Guid> ScopeIds { get; private set; }

        public DHCPv6ScopeIdNotificationCondition(
            DHCPv6RootScope rootScope,
            ISerializer serializer,
            ILogger<DHCPv6ScopeIdNotificationCondition> logger)
        {
            this._rootScope = rootScope;
            _serializer = serializer;
            this._logger = logger;
        }

        private Boolean SearchChildScope(DHCPv6Scope scope, Guid expectedId)
        {
            if (scope.Id == expectedId)
            {
                return true;
            }
            else
            {
                foreach (var item in scope.GetChildScopes())
                {
                    Boolean result = SearchChildScope(item, expectedId);
                    if (result == true)
                    {
                        return result;
                    }
                }

                return false;
            }
        }

        public override async Task<Boolean> IsValid(NotifcationTrigger trigger, TracingStream tracingStream)
        {
            if (trigger is PrefixEdgeRouterBindingUpdatedTrigger == false)
            {
                _logger.LogError("condition {name} has invalid trigger. expected trigger type is {expectedType} actual is {type}",
                    nameof(DHCPv6ScopeIdNotificationCondition), typeof(PrefixEdgeRouterBindingUpdatedTrigger), trigger.GetType());

                return false;
            }

            var castedTrigger = (PrefixEdgeRouterBindingUpdatedTrigger)trigger;

            await tracingStream.Append(1, new Dictionary<String, String>
            {
               { "ScopeId", castedTrigger.ScopeId.ToString()  },
               { "ScopeIds", JsonSerializer.Serialize(ScopeIds) }
            });

            if (ScopeIds.Contains(castedTrigger.ScopeId) == true)
            {
                await tracingStream.Append(2);

                _logger.LogDebug("triggers scope id {scopeId} found in condtition. Condition is true", castedTrigger.ScopeId);
                return true;
            }
            else
            {
                _logger.LogDebug("triggers scope id {scopeId} not found scope list. Checking if children are included", castedTrigger.ScopeId);
                if (IncludesChildren == false)
                {
                    await tracingStream.Append(3);

                    _logger.LogDebug("children shouldn't be included. Conditition evalutated to false");
                    return false;
                }
                else
                {
                    foreach (var scopeId in ScopeIds)
                    {
                        _logger.LogDebug("checking scopes recursivly for machting id");
                        _logger.LogDebug("check if {triggerId} is a child of {scopeId}", castedTrigger.ScopeId, castedTrigger.ScopeId);

                        await tracingStream.Append(4);

                        var scope = _rootScope.GetScopeById(scopeId);
                        if(scope == null)
                        {
                            _logger.LogError("scope with id {scopeId} not found", scopeId);
                            continue;
                        }

                        foreach (var item in scope.GetChildScopes())
                        {
                            Boolean found = SearchChildScope(item, castedTrigger.ScopeId);
                            if (found == true)
                            {
                                await tracingStream.Append(5);
                                return true;
                            }
                        }
                    }

                    await tracingStream.Append(6);

                    _logger.LogDebug("no child found. Condition evaluted to false");
                    return false;

                }
            }
        }

        public override NotificationConditionCreateModel ToCreateModel() => new NotificationConditionCreateModel
        {
            Typename = nameof(DHCPv6ScopeIdNotificationCondition),
            PropertiesAndValues = new Dictionary<String, String>
            {
                { nameof(IncludesChildren), _serializer.Seralize(IncludesChildren) },
                { nameof(ScopeIds), _serializer.Seralize(ScopeIds) },
            }
        };

        public override bool ApplyValues(IDictionary<string, string> propertiesAndValues)
        {
            try
            {
                IncludesChildren = _serializer.Deserialze<Boolean>(propertiesAndValues[nameof(IncludesChildren)]);
                ScopeIds = _serializer.Deserialze<IEnumerable<Guid>>(propertiesAndValues[nameof(ScopeIds)]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public override int GetTracingIdentifier() => NotificationCondition.DHCPv6ScopeIdConditionTracingIdentifier;

        public override IDictionary<string, string> GetTracingRecordDetails() => new Dictionary<String, String>
        {
            { "Name", "DHCPv6ScopeIdNotificationCondition" },
            { "IncludesChildren", JsonSerializer.Serialize(IncludesChildren) },
            { "ScopeIds", JsonSerializer.Serialize(ScopeIds) }
        };
    }
}

using Beer.DaAPI.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.NotificationPipelineResponses.V1.NotificationCondititonDescription;

namespace Beer.DaAPI.BlazorApp.Pages.Notifications
{
    public class SelectedableValue<T>
    {
        public T Value { get; set; }
        public Boolean IsSelected { get; set; }
    }

    public class NotificationPipelineConditionPropertyEntryViewModel
    {
        public ConditionsPropertyTypes Type { get; }
        public String Name { get; }

        public IList<SelectedableValue<String>> Values { get; set; }
        public String Value { get; set; }
        public Boolean ValueAsBoolean { get; set; }

        public NotificationPipelineConditionPropertyEntryViewModel(String name, ConditionsPropertyTypes type)
        {
            Name = name;
            Type = type;
        }

        public String GetSerializedValues() =>
            Type switch
            {
                ConditionsPropertyTypes.Boolean => JsonSerializer.Serialize(ValueAsBoolean),
                ConditionsPropertyTypes.DHCPv6ScopeList => JsonSerializer.Serialize(Values.Where(x => x.IsSelected == true).Select(x => x.Value).ToArray()),
                _ => String.Empty,
            };

        internal void SetScopes(IEnumerable<DHCPv6ScopeResponses.V1.DHCPv6ScopeItem> scopes)
        {
            Values = scopes.Select(x => new SelectedableValue<string> { Value = x.Id.ToString(), IsSelected = false }).ToList();
        }
    }
}

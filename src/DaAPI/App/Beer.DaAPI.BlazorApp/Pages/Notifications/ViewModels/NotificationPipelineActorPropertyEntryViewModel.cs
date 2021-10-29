using Beer.DaAPI.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.NotificationPipelineResponses.V1.NotifcationActorDescription;

namespace Beer.DaAPI.BlazorApp.Pages.Notifications
{
    public class NotificationPipelineActorPropertyEntryViewModel
    {
        public ActorPropertyTypes Type { get; }
        public String Name { get; }
        public IList<SelectedableValue<String>> Values { get; set; }
        public Boolean ValueAsBoolean { get; set; }

        public String Value { get; set; }

        public NotificationPipelineActorPropertyEntryViewModel(String name, ActorPropertyTypes type)
        {
            Name = name;
            Type = type;
        }

        public String GetSerializedValues() =>
          Type switch
          {
              ActorPropertyTypes.Boolean => JsonSerializer.Serialize(ValueAsBoolean),
              ActorPropertyTypes.DHCPv6ScopeList => JsonSerializer.Serialize(Values.Where(x => x.IsSelected == true).Select(x => x.Value).ToArray()),
              _ => "\"" + Value + "\"",
          };



        internal void SetScopes(IEnumerable<DHCPv6ScopeResponses.V1.DHCPv6ScopeItem> scopes)
        {
            Values = scopes.Select(x => new SelectedableValue<string> { Value = x.Id.ToString(), IsSelected = false }).ToList();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.NotificationPipelineResponses.V1.NotifcationActorDescription;

namespace Beer.DaAPI.BlazorApp.Pages.Notifications
{
    public class NotificationPipelineActorPropertyEntryViewModel
    {
        public ActorPropertyTypes Type { get; }
        public String Name { get; }

        public String Value { get; set; }

        public NotificationPipelineActorPropertyEntryViewModel(String name, ActorPropertyTypes type)
        {
            Name = name;
            Type = type;
        }

        public String GetSerializedValues() => "\"" + Value + "\"";
    }
}

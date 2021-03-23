using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Notifications
{
    public static class NotificationsEvent
    {
        public static class V1
        {
            public class NotificationPipelineCreatedEvent : DomainEvent
            {
                public Guid Id { get; set; }
                public String Name { get; set; }
                public String Description { get; set; }
                public String TriggerIdentifier { get; set; }
                public NotificationActorCreateModel ActorCreateInfo { get; set; }
                public NotificationConditionCreateModel ConditionCreateInfo { get; set; }
            }

            public class NotificationPipelineDeletedEvent: DomainEvent
            {
                public Guid Id { get; set; }

                public NotificationPipelineDeletedEvent()
                {

                }

                public NotificationPipelineDeletedEvent(Guid id) : this()
                {
                    Id = id;
                }
            }
        }
    }
}

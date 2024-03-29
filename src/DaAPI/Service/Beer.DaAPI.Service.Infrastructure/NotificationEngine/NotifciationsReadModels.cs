﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Infrastructure.NotificationEngine
{
    public static class NotifciationsReadModels
    {
        public static class V1
        {
            public class NotificationPipelineReadModel
            {
                public Guid Id { get; set; }
                public String Name { get; set; }
                public String TrigerName { get; set; }
                public String ConditionName { get; set; }
                public String ActorName { get; set; }
            }

            public class NotificationPipelineTriggerMapperEntry
            {
                public String TriggerName { get; set; }
                public IEnumerable<String> CompactibleConditions { get; set; }
                public IEnumerable<String> CompactibleActors { get; set; }
            }

            public class NotificationTriggerDescription
            {
                public String Name { get; set; }
            }

            public class NotifcationCondititonDescription
            {
                public enum ConditionsPropertyTtpes
                {
                    Boolean = 1,
                    DHCPv6ScopeList = 2,
                    Timespan = 3,
                }

                public String Name { get; set; }
                public IDictionary<String, ConditionsPropertyTtpes> Properties { get; set; }
            }


            public class NotifcationActorDescription
            {
                public enum ActorPropertyTtpes
                {
                    Endpoint = 1,
                    Username = 2,
                    Password = 3,
                    Boolean = 4,
                    DHCPv6ScopeList = 5,
                }

                public String Name { get; set; }
                public IDictionary<String, ActorPropertyTtpes> Properties { get; set; }
            }

            public class NotificationPipelineDescriptions
            {
                public IEnumerable<NotificationTriggerDescription> Trigger { get; set; }
                public IEnumerable<NotifcationCondititonDescription> Conditions { get; set; }
                public IEnumerable<Object> Actors { get; set; }

                public IEnumerable<NotificationPipelineTriggerMapperEntry> MapperEnries { get; set; }
            }
        }

    }
}

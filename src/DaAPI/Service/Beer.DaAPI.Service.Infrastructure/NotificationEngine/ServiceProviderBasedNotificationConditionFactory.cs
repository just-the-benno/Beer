using Beer.DaAPI.Core.Notifications;
using Beer.DaAPI.Core.Notifications.Actors;
using Beer.DaAPI.Core.Notifications.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.NotificationEngine
{
    public class ServiceProviderBasedNotificationConditionFactory : INotificationConditionFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly Dictionary<String, Type> _typeResolver = new Dictionary<string, Type>
        {
            { nameof(DHCPv6ScopeIdNotificationCondition), typeof(DHCPv6ScopeIdNotificationCondition) },
            { nameof(TimerIntervalNotificationCondition), typeof(TimerIntervalNotificationCondition) },

        };

        public ServiceProviderBasedNotificationConditionFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public NotificationCondition Initilize(NotificationConditionCreateModel conditionCreateInfo)
        {
            Console.WriteLine($"ServiceProviderBasedNotificationConditionFactory: Initilize of {conditionCreateInfo.Typename}");

            if(_typeResolver.ContainsKey(conditionCreateInfo.Typename) == false)
            {
                Console.WriteLine($"ServiceProviderBasedNotificationConditionFactory: Initilize of {conditionCreateInfo.Typename} failed");
                return NotificationCondition.Invalid;

            }

            var condition = (NotificationCondition)_serviceProvider.GetService(_typeResolver[conditionCreateInfo.Typename]);

            Boolean applied = condition.ApplyValues(conditionCreateInfo.PropertiesAndValues);
            if(applied == false)
            {
                Console.WriteLine($"ServiceProviderBasedNotificationConditionFactory: Applying values for {conditionCreateInfo.Typename} failed");

                return NotificationCondition.Invalid;
            }

            Console.WriteLine($"ServiceProviderBasedNotificationConditionFactory: Values for {conditionCreateInfo.Typename} applied");


            return condition;
        }
    }
}

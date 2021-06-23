using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Application.Commands
{
    public abstract class DHCPv6ScopeTriggerAwareCommandHandler
    {
        protected IDHCPv6StorageEngine Store { get; private set; }
        protected  IServiceBus ServiceBus { get; private set; }
        protected  DHCPv6RootScope RootScope { get; private set; }

        public DHCPv6ScopeTriggerAwareCommandHandler(IDHCPv6StorageEngine store,
            IServiceBus serviceBus,
            DHCPv6RootScope rootScope)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
            ServiceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            RootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
        }

        public virtual async Task<Boolean> SaveRootAndTriggerEvents()
        {
            Boolean result = await Store.Save(RootScope);

            if (result == true)
            {
                var triggers = RootScope.GetTriggers();

                if (triggers.Any() == true)
                {
                    await ServiceBus.Publish(new NewTriggerHappendMessage(triggers));

                    RootScope.ClearTriggers();
                }
            }

            return result;
        }

    }
}

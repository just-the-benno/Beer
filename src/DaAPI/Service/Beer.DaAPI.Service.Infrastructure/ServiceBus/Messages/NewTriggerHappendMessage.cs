using Beer.DaAPI.Core.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Infrastructure.ServiceBus.Messages
{
    public record NewTriggerHappendMessage(IEnumerable<NotifcationTrigger> Triggers) : IMessage;
}

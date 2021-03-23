using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Notifications
{
    public interface INotificationActorFactory
    {
        NotificationActor Initilize(NotificationActorCreateModel actorCreateInfo);
    }
}

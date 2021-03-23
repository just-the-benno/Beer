using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Notifications
{
    public interface INotificationTriggerSource
    {
         IEnumerable<NotifcationTrigger> GetTriggers();
         void ClearTriggers();
    }
}

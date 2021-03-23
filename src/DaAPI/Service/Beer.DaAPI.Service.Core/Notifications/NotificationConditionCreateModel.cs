using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Notifications
{
    public class NotificationConditionCreateModel : IDataTransferObject
    {
        public String Typename { get; set; }
        public IDictionary<String,String> PropertiesAndValues { get; set; }

        public NotificationConditionCreateModel()
        {
            PropertiesAndValues = new Dictionary<String, String>();
        }

    }
}

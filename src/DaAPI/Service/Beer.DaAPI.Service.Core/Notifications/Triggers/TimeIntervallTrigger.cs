using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using System.Text.Json;

namespace Beer.DaAPI.Core.Notifications.Triggers
{
    public class TimeIntervallTrigger : NotifcationTrigger
    {
        public TimeIntervallTrigger()
        {
        }

        public override String GetTypeIdentifier() => nameof(TimeIntervallTrigger);
        public override Boolean IsEmpty() => false;

        public override IDictionary<string, string> GetTracingRecordDetails() => new Dictionary<String, String>
        {
        };
    }
}

using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Tracing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Notifications
{
    public abstract class NotifcationTrigger : Value, ITracingRecord
    {
        public Guid? Id => null;
        public abstract IDictionary<string, string> GetTracingRecordDetails();

        public abstract String GetTypeIdentifier();

        public bool HasIdentity() => false;
        public virtual bool IsEmpty() => false;
    }
}

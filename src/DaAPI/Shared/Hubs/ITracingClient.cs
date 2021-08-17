using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.Shared.Hubs
{
    public interface ITracingClient
    {
        public Task StreamStarted(TracingStreamOverview stream);
        public Task RecordAppended(TracingStreamRecord record, Boolean lastEvent, Guid streamId);
    }
}

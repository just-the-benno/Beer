using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Tracing;
using Beer.DaAPI.Shared.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.TracingRequests.V1;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    public interface IReadStore
    {
        Task<Boolean> Project(IEnumerable<DomainEvent> events);
        Task<IEnumerable<Guid>> GetAllNotificationPipelineIds();

        Task<Boolean> DeleteLeaseRelatedEventsOlderThan(DateTime leaseThreshold);
        Task<Boolean> DeletePacketHandledEventsOlderThan(DateTime handledEventThreshold);
        Task<Boolean> DeletePacketHandledEventMoreThan(UInt32 amount);
       
        Task<Boolean> AddTracingStream(TracingStream stream);
        Task<Boolean> AddTracingRecord(TracingRecord record);
        Task<Boolean> CloseTracingStream(Guid streamId);
        Task<Boolean> RemoveTracingStreamsOlderThan(DateTime tracingStreamThreshold);
        Task<FilteredResult<TracingStreamOverview>> GetTracingOverview(FilterTracingRequest request);
        Task<IEnumerable<TracingStreamRecord>> GetTracingStreamRecords(Guid traceid, Guid? entityId);
    }
}

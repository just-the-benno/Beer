using Beer.DaAPI.Core.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.ServiceBus.Messages
{
    public record TracingStreamStartedMessage(TracingStream Stream) : IMessage;
    public record TracingRecordAppended(TracingRecord Record, Boolean StreamClosed) : IMessage;

    
}

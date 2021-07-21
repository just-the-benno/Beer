using Beer.DaAPI.Core.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.Tracing
{
    public interface  ITracingManager
    {
        TracingStream NewTrace(Int32 systemIdentifier, Int32 procedureIdentfier, ITracingRecord firstRecordData);
    }
}

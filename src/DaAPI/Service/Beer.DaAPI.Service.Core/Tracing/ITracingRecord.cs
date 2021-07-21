using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Tracing
{
    public interface ITracingRecord
    {
        Guid? Id { get; }

        IDictionary<String, String> GetTracingRecordDetails();
        Boolean HasIdentity();
    }
}

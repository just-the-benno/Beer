using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.BlazorApp.Services.TracingEnricher
{
    public abstract class ProcedureTracingEnricher
    {
        protected ProcedureTracingEnricher(Int32 identifier)
        {
            ProcedureIdentifier = identifier;
        }

        public Int32 ProcedureIdentifier { get; private set; }

        public abstract string GetProcedureIdentifierName();

        public abstract string GetRecordTitle(TracingStreamRecord record);
        public abstract IEnumerable<ProceduceTracingRecordValue> GetRecordDetails(TracingStreamRecord record);
        public abstract string GetProcedureIdentifierFirstItemPreview(TracingStreamOverview stream);

        protected String NormalizeJsonOutput(IDictionary<String, String> input)
        {
            var dict = input.Where(x => String.IsNullOrEmpty(x.Value) == false).ToDictionary(x => x.Key, x => x.Value.Replace("\\u0022", "\"").Replace("\"{", "{").Replace("}\"", "}").Replace("\"[", "[").Replace("]\"", "]"));
            var raw = JsonSerializer.Serialize(dict);
            return raw.Replace("\"{", "{").Replace("}\"", "}").Replace("\"[", "[").Replace("]\"", "]").Replace("\\u0022", "\"").Replace("\"null\"","null");
        }
    }
}

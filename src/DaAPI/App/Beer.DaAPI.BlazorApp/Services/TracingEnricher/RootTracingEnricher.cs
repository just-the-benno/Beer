using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.BlazorApp.Services.TracingEnricher
{
    public abstract class RootTracingEnricher
    {
        protected RootTracingEnricher(Int32 identifier, IEnumerable<ProcedureTracingEnricher> enrichers)
        {
            ModuleIdentifier = identifier;
            _procedures = enrichers;
        }

        public Int32 ModuleIdentifier { get; init; }
        public IEnumerable<ProcedureTracingEnricher> _procedures;

        internal string GetProcedureIdentifierName(int procedureIdentifier)
        {
            foreach (var item in _procedures)
            {
                if (item.ProcedureIdentifier == procedureIdentifier)
                {
                    return item.GetProcedureIdentifierName();
                }
            }

            return $"{ModuleIdentifier}.{procedureIdentifier}";
        }

        public virtual string GetModuleIdentifierName() => $"{ModuleIdentifier}";
        public string GetRecordTitle(TracingStreamRecord record)
        {
            String[] parts = record.Identifier.Split('.');
            Int32 procedureIdentifier = Int32.Parse(parts[1]);

            foreach (var item in _procedures)
            {
                if (item.ProcedureIdentifier == procedureIdentifier)
                {
                    return item.GetRecordTitle(record);
                }
            }

            return record.Identifier;
        }

        internal IEnumerable<ProceduceTracingRecordValue> GetRecordDetails(TracingStreamRecord record)
        {
            String[] parts = record.Identifier.Split('.');
            Int32 procedureIdentifier = Int32.Parse(parts[1]);

            foreach (var item in _procedures)
            {
                if (item.ProcedureIdentifier == procedureIdentifier)
                {
                    return item.GetRecordDetails(record);
                }
            }

            return Array.Empty<ProceduceTracingRecordValue>();
        }

        internal string GetProcedureIdentifierFirstItemPreview(TracingStreamOverview stream)
        {
            foreach (var item in _procedures)
            {
                if (item.ProcedureIdentifier == stream.ProcedureIdentifier)
                {
                    return item.GetProcedureIdentifierFirstItemPreview(stream);
                }
            }

            Console.WriteLine("GetProcedureIdentifierFirstItemPreview - RootTracingEnricher");

            return String.Empty;
        }
    }
}

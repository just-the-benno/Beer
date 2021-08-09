using Beer.DaAPI.Shared.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.BlazorApp.Services.TracingEnricher
{
    public abstract record ProceduceTracingRecordValue(String Label);
    public record StringBasedProceduceTracingRecordValue(String Label, String Value) : ProceduceTracingRecordValue(Label);
    public record LinkBasedProceduceTracingRecordValue(String Label, String Title, String Url) : ProceduceTracingRecordValue(Label);
    public record LinkListBasedProceduceTracingRecordValue(String Label, IDictionary<String,String> Links) : ProceduceTracingRecordValue(Label);

    public class TracingEnricherService
    {
        public TracingEnricherService(IServiceProvider provider)
        {
            _enricher.AddRange(provider.GetServices<RootTracingEnricher>());
        }

        public Int32 NotificationSystemIdentifier { get; set; } = 10;

        private List<RootTracingEnricher> _enricher = new();

        internal string GetModuleIdentifierName(int identifier)
        {
            foreach (var item in _enricher)
            {
                if (item.ModuleIdentifier == identifier)
                {
                    return item.GetModuleIdentifierName();
                }
            }

            return identifier.ToString();
        }

        internal string GetProcedureIdentifierName(int moduleIdentifier, int procedureIdentifier)
        {
            foreach (var item in _enricher)
            {
                if (item.ModuleIdentifier == moduleIdentifier)
                {
                    return item.GetProcedureIdentifierName(procedureIdentifier);
                }
            }

            return $"{moduleIdentifier}.{procedureIdentifier}";
        }

        internal string GetRecordTitle(TracingStreamRecord record)
        {
            String[] parts = record.Identifier.Split('.');
            Int32 moduleIdentifier = Int32.Parse(parts[0]);

            foreach (var item in _enricher)
            {
                if (item.ModuleIdentifier == moduleIdentifier)
                {
                    return item.GetRecordTitle(record);
                }
            }

            return record.Identifier;
        }

        internal string GetProcedureIdentifierFirstItemPreview(TracingStreamOverview strem)
        {
            foreach (var item in _enricher)
            {
                if (item.ModuleIdentifier == strem.ModuleIdentifier)
                {
                    return item.GetProcedureIdentifierFirstItemPreview(strem);
                }
            }

            Console.WriteLine("GetProcedureIdentifierFirstItemPreview - TracingEnricherService");
            return String.Empty;
        }

        internal IEnumerable<ProceduceTracingRecordValue> GetRecordDetails(TracingStreamRecord record)
        {
            String[] parts = record.Identifier.Split('.');
            Int32 moduleIdentifier = Int32.Parse(parts[0]);

            foreach (var item in _enricher)
            {
                if (item.ModuleIdentifier == moduleIdentifier)
                {
                    return item.GetRecordDetails(record);
                }
            }

            return Array.Empty<ProceduceTracingRecordValue>();
        }
    }
}

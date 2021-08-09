using Beer.DaAPI.BlazorApp.Services;
using Beer.DaAPI.BlazorApp.Services.TracingEnricher;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.TracingRequests.V1;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.BlazorApp.Components
{
    public partial class DaAPITracingStream
    {
        private IEnumerable<TracingStreamOverview> _streams;
        private Dictionary<Guid, IEnumerable<TracingStreamRecord>> _cachedRecords = new();

        private Int32 _start = 0;
        private Int32 _amount = 50;
        private Int32 _total = 0;

        [Inject]
        public DaAPIService Service { get; set; }

        [Inject]
        public TracingEnricherService TracingEnricher { get; set; }

        [Inject]
        public IStringLocalizer<DaAPITracingStream> L { get; set; }

        [Parameter] public Int32? ModuleIdentifier { get; set; }
        [Parameter] public Int32? ProcedureIdentiifier { get; set; }
        [Parameter] public Guid? EntityId { get; set; }

        [Parameter] public Boolean ShowModuleIdentifier { get; set; }
        [Parameter] public Boolean ShowProcedureIdentifier { get; set; }
        
        private async Task LoadItems()
        {
            var result = await Service.GetTracingOverview(new FilterTracingRequest
            {
                Amount = _amount,
                EntitiyId = EntityId,
                ModuleIdentifier = ModuleIdentifier,
                ProcedureIdentifier = ProcedureIdentiifier,
                Start = _start,
            });

            _streams = result.Result;
            _total = result.Total;
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            await LoadItems();
        }

        private String GetModuleIdentifierName(Int32 identifier) => TracingEnricher.GetModuleIdentifierName(identifier);
        private String GetProcedureIdentifierName(Int32 moduleIdentifier, Int32 procedureIdentifier) => TracingEnricher.GetProcedureIdentifierName(moduleIdentifier, procedureIdentifier);
        private String GetProcedureIdentifierFirstItemPreview(TracingStreamOverview stream) => TracingEnricher.GetProcedureIdentifierFirstItemPreview(stream);
        private String GetRecordTitle(TracingStreamRecord record) => TracingEnricher.GetRecordTitle(record);
        private IEnumerable<ProceduceTracingRecordValue> GetRecordDetails(TracingStreamRecord record) => TracingEnricher.GetRecordDetails(record);

        private async Task LoadStream(Boolean e, Guid streamId)
        {
            if(e == false) { return; }

            if(_cachedRecords.ContainsKey(streamId) == true) { return; }

            var result = await Service.GetTracingStreamRecords(streamId, EntityId);
            _cachedRecords.Add(streamId, result);
        }

        private async Task GotoPage(Int32 page)
        {
            _streams = null;
            StateHasChanged();

            _start = (page - 1) * _amount;
            await LoadItems();
        }
    }
}

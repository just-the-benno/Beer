using Beer.DaAPI.BlazorApp.Services;
using Beer.DaAPI.BlazorApp.Services.TracingEnricher;
using Beer.DaAPI.Shared.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.TracingRequests.V1;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.BlazorApp.Components
{
    public partial class DaAPITracingStream : IAsyncDisposable
    {
        private IList<TracingStreamOverview> _streams;
        private Dictionary<Guid, ICollection<TracingStreamRecord>> _cachedRecords = new();

        private Int32 _start = 0;
        private Int32 _amount = 50;
        private Int32 _total = 0;

        private HubConnection _hubConnection;

        private String _subscripedGroup;


        [Inject] public DaAPIService Service { get; set; }
        [Inject] public TracingEnricherService TracingEnricher { get; set; }
        [Inject] public EndpointOptions Endpoints { get; set; }

        [Inject] public IStringLocalizer<DaAPITracingStream> L { get; set; }

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

            _streams = result.Result.ToList();
            _total = result.Total;
        }

        protected override async Task OnInitializedAsync()
        {

            await base.OnInitializedAsync();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(Endpoints.HubEndpoint, "tracing"))
                .Build();

            _hubConnection.On<TracingStreamOverview>(nameof(ITracingClient.StreamStarted), (stream) =>
            {
                if (_start == 0 && _streams != null)
                {
                    if(_streams.FirstOrDefault(x => x.Id == stream.Id) == null)
					{
                        _streams.Insert(0, stream);
                        InvokeAsync(StateHasChanged);
                    }
                }
            });

            _hubConnection.On<TracingStreamRecord, Boolean, Guid>(nameof(ITracingClient.RecordAppended), (record, close, streamId) =>
             {
                 if (_cachedRecords.ContainsKey(streamId) == false) { return; }

                 _cachedRecords[streamId].Add(record);

                 var stream = _streams.FirstOrDefault(x => x.Id == streamId);

                 if (stream != null)
                 {
                     if (record.Status == TracingRecordStatusForResponses.Error)
                     {
                         stream.Status = TracingRecordStatusForResponses.Error;
                     }
                     else if (record.Status == TracingRecordStatusForResponses.Success)
                     {
                         stream.Status = TracingRecordStatusForResponses.Success;
                     }

                     stream.RecordAmount += 1;
                     if (close == true)
                     {
                         stream.IsInProgress = false;
                     }
                 }

                 InvokeAsync(StateHasChanged);
             });

            //await _hubConnection.StartAsync();
            await Task.Delay(10);

        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            await LoadItems();

            string groupToJoin = null;

            if (EntityId.HasValue == true)
            {
                groupToJoin = EntityId.Value.ToString();
            }
            else
            {
                if (ProcedureIdentiifier.HasValue == true)
                {
                    groupToJoin = $"{ModuleIdentifier.Value}.{ProcedureIdentiifier.Value}";
                }
                else
                {
                    groupToJoin = $"{ModuleIdentifier.Value}.*";
                }
            }

            if (_subscripedGroup != groupToJoin)
            {
                if (String.IsNullOrEmpty(_subscripedGroup) == false)
                {
                    await UnsubscribeOfGroup();
                }

                if (String.IsNullOrEmpty(groupToJoin) == false)
                {
                    await SubscribeToGroup(groupToJoin);
                }
            }
        }

        private String GetModuleIdentifierName(Int32 identifier) => TracingEnricher.GetModuleIdentifierName(identifier);
        private String GetProcedureIdentifierName(Int32 moduleIdentifier, Int32 procedureIdentifier) => TracingEnricher.GetProcedureIdentifierName(moduleIdentifier, procedureIdentifier);
        private String GetProcedureIdentifierFirstItemPreview(TracingStreamOverview stream) => TracingEnricher.GetProcedureIdentifierFirstItemPreview(stream);
        private String GetRecordTitle(TracingStreamRecord record) => TracingEnricher.GetRecordTitle(record);
        private IEnumerable<ProceduceTracingRecordValue> GetRecordDetails(TracingStreamRecord record) => TracingEnricher.GetRecordDetails(record);

        private async Task LoadStream(Boolean e, Guid streamId)
        {
            if (e == false) { return; }

            if (_cachedRecords.ContainsKey(streamId) == true) { return; }

            var result = await Service.GetTracingStreamRecords(streamId, EntityId);
            _cachedRecords.Add(streamId, result.ToList());
        }

        private async Task GotoPage(Int32 page)
        {
            _streams = null;
            StateHasChanged();

            _start = (page - 1) * _amount;
            await LoadItems();
        }

        private Color GetTimelineColorBasedOnRecord(TracingStreamRecord record) => record.Status switch
        {
            TracingRecordStatusForResponses.Error => Color.Error,
            TracingRecordStatusForResponses.Success => Color.Success,
            _ => Color.Info,
        };

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_hubConnection != null)
                {
                    await UnsubscribeOfGroup();
                    await _hubConnection.DisposeAsync();
                }
            }
            catch (Exception)
            {

            }
        }

        private async Task UnsubscribeOfGroup()
        {
            //await _hubConnection.SendAsync("Unsubscribe", _subscripedGroup);
            //_subscripedGroup = String.Empty;
            await Task.Delay(10);
        }

        private async Task SubscribeToGroup(String groupName)
        {
            //await _hubConnection.SendAsync("Subscribe", groupName);
            //_subscripedGroup = groupName;
            await Task.Delay(10);
        }
    }
}

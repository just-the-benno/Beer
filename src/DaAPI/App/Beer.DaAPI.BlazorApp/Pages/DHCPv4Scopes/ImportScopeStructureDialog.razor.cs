using Beer.DaAPI.BlazorApp.Dialogs;
using Beer.DaAPI.BlazorApp.Services;
using Beer.DaAPI.Shared.JsonConverters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public partial class ImportScopeStructureDialog : DaAPIDialogBase
    {
        [Inject] DaAPIService Service { get; set; }
        [CascadingParameter] MudDialogInstance Instance {get; set; }

        private String _content;
        private Int32 _numberOfScopes = 0;
        private Int32 _currentScope = 0;
        private Boolean _inProgress = false;

        private Boolean IsInputValid() => IsInputValid(_content);

        private JsonSerializerSettings _jsonSettings;

        public ImportScopeStructureDialog()
        {
            _jsonSettings =
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

            _jsonSettings.Converters.Add(new DHCPv6ScopePropertyRequestJsonConverter());
            _jsonSettings.Converters.Add(new DHCPv4ScopePropertyRequestJsonConverter());

        }

        private Boolean IsInputValid(String input)
        {
            try
            {
                var requests =  JsonConvert.DeserializeObject<List<ExportDHCPRequest>>(input, _jsonSettings);
                Console.WriteLine(requests.Count);

                return requests.Count > 1;   
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private  async  Task StartImport()
        {
            if(IsInputValid() == false) { return; }

            var requests = JsonConvert.DeserializeObject<List<ExportDHCPRequest>>(_content, _jsonSettings);

            _numberOfScopes = requests.Count();
            _inProgress = true;
            StateHasChanged();

            Dictionary<Guid, Guid> parentIdMapper = new();

            foreach (var request in requests)
            {
                if(request.Request.ParentId.HasValue == true)
                {
                    request.Request.ParentId = parentIdMapper[request.Request.ParentId.Value];
                }

                var result = await Service.CreateDHCPv4Scope(request.Request);
                _currentScope++;
                StateHasChanged();

                if (result != default)
                {
                    parentIdMapper.Add(request.Id, result);
                }
                else
                {
                   
                }
            }

            Instance.Close(DialogResult.Ok(true));
        }
    }
}
